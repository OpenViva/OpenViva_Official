#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;

/// <summary>
/// Desktop hand driver matching old OpenViva's physical hand system.
/// 
/// Physics hands (Rigidbody + Collider) are separate root GameObjects that collide with the world.
/// Visual hands (skinned mesh on wrist bones) are overridden in LateUpdate to follow the physics hands.
/// 
/// This creates the old OpenViva feel: hands are always physical, they push objects,
/// stop at walls, and interact with everything without needing to click.
/// 
/// Target positions are calculated from stored camera-relative offsets (NOT from bone positions)
/// to avoid feedback loops since LateUpdate overrides bone positions.
/// </summary>
public class DesktopHandDriver : MonoBehaviour
{
    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Hand Systems")]
    public HandGrabSystem leftHandGrab;
    public HandGrabSystem rightHandGrab;

    [Header("Wrist Bones (visual mesh follows physics via LateUpdate)")]
    public Transform leftWristBone;
    public Transform rightWristBone;

    [Header("Reach Extension")]
    public float maxExtend = 0.5f;
    public float maxRetract = 0.1f;
    public float scrollStep = 0.04f;
    public float reachSmoothSpeed = 10f;

    // Camera-local offsets captured at Start (before any LateUpdate override)
    Vector3 leftCameraOffset;
    Vector3 rightCameraOffset;
    Quaternion leftRotOffset;
    Quaternion rightRotOffset;

    float currentReach;
    float targetReach;
    bool initialized;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform;

        // Capture wrist bone positions relative to camera BEFORE any physics override.
        // These stored offsets are used every frame for targets, never reading back from bones.
        if (cameraTransform != null)
        {
            if (leftWristBone != null)
            {
                leftCameraOffset = cameraTransform.InverseTransformPoint(leftWristBone.position);
                leftRotOffset = Quaternion.Inverse(cameraTransform.rotation) * leftWristBone.rotation;
            }
            if (rightWristBone != null)
            {
                rightCameraOffset = cameraTransform.InverseTransformPoint(rightWristBone.position);
                rightRotOffset = Quaternion.Inverse(cameraTransform.rotation) * rightWristBone.rotation;
            }
        }

        // Subscribe to grab events for freeze-to-head system
        if (leftHandGrab != null)
        {
            leftHandGrab.OnItemGrabbed += OnLeftGrabbed;
            leftHandGrab.OnItemReleased += OnLeftReleased;
        }
        if (rightHandGrab != null)
        {
            rightHandGrab.OnItemGrabbed += OnRightGrabbed;
            rightHandGrab.OnItemReleased += OnRightReleased;
        }

        initialized = true;
    }

    void OnDestroy()
    {
        if (leftHandGrab != null)
        {
            leftHandGrab.OnItemGrabbed -= OnLeftGrabbed;
            leftHandGrab.OnItemReleased -= OnLeftReleased;
        }
        if (rightHandGrab != null)
        {
            rightHandGrab.OnItemGrabbed -= OnRightGrabbed;
            rightHandGrab.OnItemReleased -= OnRightReleased;
        }
    }

    void FixedUpdate()
    {
        if (!initialized || cameraTransform == null) return;

        currentReach = Mathf.Lerp(currentReach, targetReach, reachSmoothSpeed * Time.fixedDeltaTime);

        UpdateHandTarget(leftHandGrab, leftCameraOffset, leftRotOffset);
        UpdateHandTarget(rightHandGrab, rightCameraOffset, rightRotOffset);
    }

    void UpdateHandTarget(HandGrabSystem hand, Vector3 camOffset, Quaternion rotOffset)
    {
        if (hand == null) return;

        if (hand.FrozenLocalPos.HasValue)
        {
            // Frozen to head (holding item)
            hand.TargetPosition = cameraTransform.TransformPoint(hand.FrozenLocalPos.Value);
            hand.TargetRotation = cameraTransform.rotation * hand.FrozenLocalRot;
        }
        else
        {
            // Camera-relative target from stored offset + reach extension
            Vector3 targetPos = cameraTransform.TransformPoint(camOffset);
            targetPos += cameraTransform.forward * currentReach;
            hand.TargetPosition = targetPos;
            hand.TargetRotation = cameraTransform.rotation * rotOffset;
        }
    }

    /// <summary>
    /// After Animator runs, override wrist bone world positions to match physics hands.
    /// This makes the visual skinned mesh react to physics — hands stop at walls,
    /// push objects, and feel alive instead of static like a PNG overlay.
    /// </summary>
    void LateUpdate()
    {
        if (!initialized || cameraTransform == null) return;

        SyncBone(leftWristBone, leftHandGrab, leftCameraOffset, leftRotOffset);
        SyncBone(rightWristBone, rightHandGrab, rightCameraOffset, rightRotOffset);
    }

    /// <summary>
    /// Sync visual wrist bone to physics hand. If the physics hand drifted
    /// too far (e.g. player sprinting faster than physics can follow),
    /// snap the visual bone to the expected camera-relative position so
    /// hands never visually disappear.
    /// </summary>
    void SyncBone(Transform bone, HandGrabSystem hand, Vector3 camOffset, Quaternion rotOffset)
    {
        if (bone == null || hand == null) return;

        Vector3 expectedPos = cameraTransform.TransformPoint(camOffset)
                            + cameraTransform.forward * currentReach;
        Vector3 physicsPos = hand.transform.position;

        // If physics hand is close to where it should be, use physics position
        // (so collisions visually affect hand). Otherwise snap to expected.
        float driftSqr = (physicsPos - expectedPos).sqrMagnitude;
        if (driftSqr < 0.25f) // 0.5m threshold
        {
            bone.position = physicsPos;
            bone.rotation = hand.transform.rotation;
        }
        else
        {
            bone.position = expectedPos;
            bone.rotation = cameraTransform.rotation * rotOffset;
        }
    }

    #region Reach Controls (called by PlayerKB_GrabController)

    public void ExtendHands()
    {
        targetReach = Mathf.Min(targetReach + scrollStep, maxExtend);
    }

    public void RetractHands()
    {
        targetReach = Mathf.Max(targetReach - scrollStep, -maxRetract);
    }

    #endregion

    #region Freeze-to-Head (old: freezeKeyboardLocalPosition)

    void OnLeftGrabbed(GrabbableItem item)
    {
        if (leftHandGrab == null || cameraTransform == null) return;
        leftHandGrab.FrozenLocalPos = cameraTransform.InverseTransformPoint(leftHandGrab.transform.position);
        leftHandGrab.FrozenLocalRot = Quaternion.Inverse(cameraTransform.rotation) * leftHandGrab.transform.rotation;
    }

    void OnLeftReleased(GrabbableItem item)
    {
        if (leftHandGrab != null)
            leftHandGrab.FrozenLocalPos = null;
    }

    void OnRightGrabbed(GrabbableItem item)
    {
        if (rightHandGrab == null || cameraTransform == null) return;
        rightHandGrab.FrozenLocalPos = cameraTransform.InverseTransformPoint(rightHandGrab.transform.position);
        rightHandGrab.FrozenLocalRot = Quaternion.Inverse(cameraTransform.rotation) * rightHandGrab.transform.rotation;
    }

    void OnRightReleased(GrabbableItem item)
    {
        if (rightHandGrab != null)
            rightHandGrab.FrozenLocalPos = null;
    }

    #endregion
}

#endif
