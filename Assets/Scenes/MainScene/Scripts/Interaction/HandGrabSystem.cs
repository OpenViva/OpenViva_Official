using UnityEngine;

/// <summary>
/// Physics-based hand grab system matching old OpenViva's OccupyState + RigidBodyBlend.
/// Hands are REAL Rigidbodies that interact with the world physically.
/// Items are grabbed via ConfigurableJoint (spring-driven) or FixedJoint.
/// Blend system ramps massScale from 0→heldMassScale over blendDuration.
/// Works for both Desktop and VR — the hand Rigidbody is ALWAYS required.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class HandGrabSystem : MonoBehaviour
{
    [Header("Hand Setup")]
    public bool isLeftHand;
    public Transform gripPoint;
    public Collider handCollider;

    [Header("Hand Physics (old values: mass=5, gravity=false)")]
    [Tooltip("Force multiplier for hand chasing target position (old: 64)")]
    public float positionForce = 64f;
    [Tooltip("Torque multiplier for hand chasing target rotation (old: 128)")]
    public float rotationForce = 128f;
    [Tooltip("Distance threshold to teleport hand instead of force-chase (old: ~0.86)")]
    public float teleportThreshold = 0.75f;

    [Header("Detection")]
    public float grabRadius = 0.15f;
    public LayerMask grabLayerMask = ~0;
    public float grabRayDistance = 0.15f;

    [Header("Grab Physics (old: spring=100000, damper=10, massScale ramps 0→10)")]
    [Tooltip("Grab blend duration in seconds (old: 0.6)")]
    public float blendDuration = 0.6f;
    [Tooltip("Release blend duration (old: 0.4)")]
    public float releaseBlendDuration = 0.4f;

    [Header("Throw")]
    public int velocityFrameWindow = 6;

    // Events
    public System.Action<GrabbableItem> OnItemGrabbed;
    public System.Action<GrabbableItem> OnItemReleased;

    // State
    public GrabbableItem HeldItem { get; private set; }
    public bool IsHolding => HeldItem != null;
    public Rigidbody HandRigidbody { get; private set; }

    // Target transform that the hand chases (set by DesktopHandDriver or VR tracker)
    public Vector3 TargetPosition { get; set; }
    public Quaternion TargetRotation { get; set; }

    // Frozen head-local position when holding item (desktop freeze-to-head)
    public Vector3? FrozenLocalPos;
    public Quaternion FrozenLocalRot;

    // Nearest item
    GrabbableItem nearestItem;

    // Physics joint for grab
    ConfigurableJoint grabJoint;
    FixedJoint fixedGrabJoint;
    Quaternion jointStartRotation;

    // Blend
    float blendProgress;
    float blendTarget;
    float blendSpeed;

    // Velocity tracking
    Vector3[] velocityHistory;
    Vector3[] angularVelocityHistory;
    int velocityIndex;
    Vector3 lastPosition;
    Quaternion lastRotation;

    // Held item layer management
    int originalItemLayer;

    // Player collider to ignore when holding items
    Collider playerCollider;
    Collider[] heldItemColliders;

    void Awake()
    {
        HandRigidbody = GetComponent<Rigidbody>();
        if (handCollider == null)
            handCollider = GetComponent<Collider>();
        if (gripPoint == null)
            gripPoint = transform;

        // Old OpenViva hand Rigidbody setup
        HandRigidbody.mass = 5f;
        HandRigidbody.useGravity = false;
        HandRigidbody.isKinematic = false;
        HandRigidbody.linearDamping = 0f;
        HandRigidbody.angularDamping = 0f;
        HandRigidbody.maxAngularVelocity = 64f;
        HandRigidbody.maxDepenetrationVelocity = 0.001f;
        HandRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        HandRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        velocityHistory = new Vector3[velocityFrameWindow];
        angularVelocityHistory = new Vector3[velocityFrameWindow];
        lastPosition = transform.position;
        lastRotation = transform.rotation;

        TargetPosition = transform.position;
        TargetRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        ApplyHandPhysics();
        UpdateVelocityTracking();
        UpdateBlend();
    }

    void Update()
    {
        if (!IsHolding)
            UpdateNearestItem();
    }

    #region Hand Physics Following (old: ApplyRigidBodyTransform)

    /// <summary>
    /// Move hand toward target by directly setting velocity each step.
    /// Unlike AddForce (which accumulates and oscillates), this converges cleanly.
    /// Still respects physics — walls block movement, objects get pushed.
    /// </summary>
    void ApplyHandPhysics()
    {
        float dt = Time.fixedDeltaTime;
        if (dt <= 0f) return;

        float forceMult = IsHolding ? blendProgress : 1f;
        Vector3 posDelta = TargetPosition - transform.position;

        // Teleport safety net — instant snap when too far (e.g. sprinting)
        if (posDelta.sqrMagnitude > teleportThreshold * teleportThreshold)
        {
            HandRigidbody.position = TargetPosition;
            HandRigidbody.rotation = TargetRotation;
            HandRigidbody.linearVelocity = Vector3.zero;
            HandRigidbody.angularVelocity = Vector3.zero;
            return;
        }

        // Position: set velocity to reach target in one step
        // No accumulation → no oscillation. Physics solver still resolves collisions.
        HandRigidbody.linearVelocity = posDelta / dt * forceMult;

        // Rotation: compute angular velocity from rotation delta
        Quaternion rotDelta = TargetRotation * Quaternion.Inverse(transform.rotation);
        rotDelta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        if (float.IsInfinity(axis.x) || float.IsNaN(axis.x))
        {
            HandRigidbody.angularVelocity = Vector3.zero;
            return;
        }
        HandRigidbody.angularVelocity = axis.normalized * (angle * Mathf.Deg2Rad / dt) * forceMult;
    }

    #endregion

    #region Grab / Release

    public bool AttemptGrab()
    {
        if (IsHolding) return false;

        var item = FindBestGrabbable();
        if (item == null) return false;

        return GrabItem(item);
    }

    public bool GrabItem(GrabbableItem item)
    {
        if (item == null || !item.canBePickedUp) return false;

        // If another hand is holding it, force drop
        if (item.IsHeld && !item.allowMultipleHolders)
            item.heldByHand.ReleaseItem(false);

        if (IsHolding)
            ReleaseItem(false);

        HeldItem = item;
        item.OnGrabbed(this);
        ClearHighlight();

        // Save original layer and move to held items layer
        originalItemLayer = item.gameObject.layer;

        // Ignore collision between held item and player capsule
        IgnorePlayerCollision(item, true);

        // Snap item to grip point
        item.GetHoldTransform(isLeftHand, out Vector3 localPos, out Quaternion localRot);
        item.transform.SetParent(gripPoint);
        item.transform.localPosition = localPos;
        item.transform.localRotation = localRot;

        // Create physics joint (old: BeginRigidBodyGrab → RigidBodyBlend)
        CreateGrabJoint(item);

        // Start blend 0→1
        blendProgress = 0f;
        blendTarget = 1f;
        blendSpeed = 1f / Mathf.Max(blendDuration, 0.01f);

        OnItemGrabbed?.Invoke(item);
        return true;
    }

    public void ReleaseItem(bool applyThrow = true)
    {
        if (!IsHolding) return;

        var item = HeldItem;
        HeldItem = null;

        DestroyGrabJoint();

        item.transform.SetParent(null);

        // Restore item layer
        item.gameObject.layer = originalItemLayer;

        // Restore collision between item and player
        IgnorePlayerCollision(item, false);

        // Throw velocity
        Vector3 throwVel = Vector3.zero;
        Vector3 angularVel = Vector3.zero;
        if (applyThrow)
            GetAverageVelocity(out throwVel, out angularVel);

        item.OnReleased(throwVel, angularVel);

        // Start blend out
        blendTarget = 0f;
        blendSpeed = 1f / releaseBlendDuration;

        // Clear freeze
        FrozenLocalPos = null;

        OnItemReleased?.Invoke(item);
    }

    #endregion

    #region Joint Management (old: RigidBodyBlend)

    void CreateGrabJoint(GrabbableItem item)
    {
        jointStartRotation = item.transform.localRotation;

        if (item.useFixedJoint)
        {
            fixedGrabJoint = gripPoint.gameObject.AddComponent<FixedJoint>();
            fixedGrabJoint.connectedBody = item.rb;
            fixedGrabJoint.breakForce = item.breakForce;
            fixedGrabJoint.breakTorque = item.breakForce;
            fixedGrabJoint.enablePreprocessing = false;
        }
        else
        {
            // Old OpenViva ConfigurableJoint setup
            grabJoint = gripPoint.gameObject.AddComponent<ConfigurableJoint>();
            grabJoint.connectedBody = item.rb;
            grabJoint.autoConfigureConnectedAnchor = false;
            grabJoint.anchor = Vector3.zero;
            grabJoint.connectedAnchor = Vector3.zero;

            // Linear: X locked, Y/Z limited (old setup)
            grabJoint.xMotion = ConfigurableJointMotion.Locked;
            grabJoint.yMotion = ConfigurableJointMotion.Limited;
            grabJoint.zMotion = ConfigurableJointMotion.Limited;

            var linearLimit = new SoftJointLimit { limit = 10f };
            grabJoint.linearLimit = linearLimit;

            // Spring drives (old: spring=100000, damper=10)
            var drive = new JointDrive
            {
                positionSpring = item.holdSpring,
                positionDamper = item.holdDamper,
                maximumForce = float.MaxValue
            };
            grabJoint.xDrive = drive;
            grabJoint.yDrive = drive;
            grabJoint.zDrive = drive;

            // Rotation via slerp (old: same spring/damper)
            grabJoint.rotationDriveMode = RotationDriveMode.Slerp;
            grabJoint.slerpDrive = drive;

            grabJoint.angularXMotion = ConfigurableJointMotion.Free;
            grabJoint.angularYMotion = ConfigurableJointMotion.Free;
            grabJoint.angularZMotion = ConfigurableJointMotion.Free;

            grabJoint.targetRotation = Quaternion.identity;

            grabJoint.breakForce = item.breakForce;
            grabJoint.breakTorque = item.breakForce;
            grabJoint.enablePreprocessing = false;
            grabJoint.massScale = 0.0001f; // starts at 0, blends up
        }
    }

    void DestroyGrabJoint()
    {
        if (grabJoint != null)
        {
            Destroy(grabJoint);
            grabJoint = null;
        }
        if (fixedGrabJoint != null)
        {
            Destroy(fixedGrabJoint);
            fixedGrabJoint = null;
        }
    }

    void OnJointBreak(float breakForce)
    {
        if (IsHolding)
            ReleaseItem(false);
    }

    #endregion

    #region Player Collision Ignore

    /// <summary>
    /// Finds the player capsule collider (cached) and ignores/restores
    /// collision with all colliders on the held item.
    /// Prevents the knife from bouncing off your own body when sprinting.
    /// </summary>
    void IgnorePlayerCollision(GrabbableItem item, bool ignore)
    {
        if (playerCollider == null)
            FindPlayerCollider();
        if (playerCollider == null) return;

        var cols = item.GetComponentsInChildren<Collider>();
        foreach (var col in cols)
        {
            if (col != null && col != handCollider)
                Physics.IgnoreCollision(playerCollider, col, ignore);
        }

        if (ignore)
            heldItemColliders = cols;
        else
            heldItemColliders = null;
    }

    void FindPlayerCollider()
    {
        // Walk up from hand to find the player root with a CapsuleCollider
        var movement = Object.FindFirstObjectByType<PlayerKB_Movement>();
        if (movement != null)
            playerCollider = movement.GetComponent<CapsuleCollider>();
    }

    #endregion

    #region Blend (old: blendProgress 0→1 over 0.6s, massScale ramps)

    void UpdateBlend()
    {
        if (Mathf.Approximately(blendProgress, blendTarget)) return;

        blendProgress = Mathf.MoveTowards(blendProgress, blendTarget, blendSpeed * Time.fixedDeltaTime);

        // Ramp massScale with blend (old: 1.0 + blendValue * additionalConnectedMassScale)
        if (grabJoint != null && HeldItem != null)
        {
            grabJoint.massScale = 1f + blendProgress * HeldItem.heldMassScale;
        }
    }

    #endregion

    #region Detection

    GrabbableItem FindBestGrabbable()
    {
        // Raycast from hand (old: CalculateNearbyGrabCollider, 0.15m range)
        if (Physics.Raycast(gripPoint.position, -gripPoint.forward + gripPoint.up, out RaycastHit hit, grabRayDistance, grabLayerMask))
        {
            var item = hit.collider.GetComponentInParent<GrabbableItem>();
            if (item != null && item.canBePickedUp && !item.IsHeld)
                return item;
        }

        // Overlap sphere fallback
        var colliders = Physics.OverlapSphere(gripPoint.position, grabRadius, grabLayerMask);
        GrabbableItem closest = null;
        float closestDist = float.MaxValue;

        foreach (var col in colliders)
        {
            var item = col.GetComponentInParent<GrabbableItem>();
            if (item == null || !item.canBePickedUp || item.IsHeld) continue;

            float dist = Vector3.Distance(gripPoint.position, col.ClosestPoint(gripPoint.position));
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = item;
            }
        }

        return closest;
    }

    void UpdateNearestItem()
    {
        var best = FindBestGrabbable();
        if (best != nearestItem)
        {
            if (nearestItem != null)
                nearestItem.SetHighlight(false);
            nearestItem = best;
            if (nearestItem != null)
                nearestItem.SetHighlight(true);
        }
    }

    void ClearHighlight()
    {
        if (nearestItem != null)
        {
            nearestItem.SetHighlight(false);
            nearestItem = null;
        }
    }

    public GrabbableItem GetNearestItem() => nearestItem;

    #endregion

    #region Velocity Tracking

    void UpdateVelocityTracking()
    {
        float dt = Time.fixedDeltaTime;
        if (dt <= 0f) return;

        velocityHistory[velocityIndex] = (gripPoint.position - lastPosition) / dt;

        Quaternion deltaRot = gripPoint.rotation * Quaternion.Inverse(lastRotation);
        deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        angularVelocityHistory[velocityIndex] = axis * (angle * Mathf.Deg2Rad / dt);

        lastPosition = gripPoint.position;
        lastRotation = gripPoint.rotation;
        velocityIndex = (velocityIndex + 1) % velocityFrameWindow;
    }

    void GetAverageVelocity(out Vector3 velocity, out Vector3 angularVelocity)
    {
        velocity = Vector3.zero;
        angularVelocity = Vector3.zero;

        for (int i = 0; i < velocityFrameWindow; i++)
        {
            velocity += velocityHistory[i];
            angularVelocity += angularVelocityHistory[i];
        }

        velocity /= velocityFrameWindow;
        angularVelocity /= velocityFrameWindow;
    }

    #endregion

    void OnDrawGizmosSelected()
    {
        var gp = gripPoint != null ? gripPoint : transform;
        Gizmos.color = IsHolding ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(gp.position, grabRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(gp.position, (-gp.forward + gp.up).normalized * grabRayDistance);
    }
}
