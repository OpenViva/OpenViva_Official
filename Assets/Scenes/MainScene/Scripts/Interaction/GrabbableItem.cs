using UnityEngine;

/// <summary>
/// Attach to any object that can be picked up. Replaces the old per-item PlayerKB_GrabObject.
/// Defines how an item behaves when grabbed: hold pose, physics properties, throw behavior.
/// </summary>
public class GrabbableItem : MonoBehaviour
{
    public enum HoldPose
    {
        Default,    // generic grip
        Bag,
        RubberDucky,
        Peach,
        Strawberry,
        Cantaloupe,
        Blueberry,
        Wheat,
        Flashlight,
        Egg,
        FlourJar,
        Knife,
        Lantern,
        MilkCanister,
        MixingBowl,
        MixingSpoon,
        Mortar,
        Pestle,
        Pot,
        Soap,
        Towel
    }

    [Header("Item Identity")]
    [Tooltip("Which hand animation pose to use when held")]
    public HoldPose holdPose = HoldPose.Default;

    [Header("Hold Offsets (Left Hand)")]
    public Vector3 holdPositionLeft = Vector3.zero;
    public Vector3 holdRotationLeft = Vector3.zero;

    [Header("Hold Offsets (Right Hand)")]
    public Vector3 holdPositionRight = Vector3.zero;
    public Vector3 holdRotationRight = Vector3.zero;

    [Header("Physics (old OpenViva values)")]
    [Tooltip("Mass scale when held — ramps from 0 to this during grab blend (old: 10)")]
    public float heldMassScale = 10f;
    [Tooltip("Spring force of the ConfigurableJoint holding this item (old: 100000)")]
    public float holdSpring = 100000f;
    [Tooltip("Damping of the ConfigurableJoint (old: 10)")]
    public float holdDamper = 10f;
    [Tooltip("Maximum force before the joint breaks and item drops")]
    public float breakForce = 5000f;
    [Tooltip("Use a fixed joint instead of a spring joint (rigid connection)")]
    public bool useFixedJoint;
    [Tooltip("Can multiple characters hold this item at once")]
    public bool allowMultipleHolders;

    [Header("Grab Settings")]
    [Tooltip("Toggle holding mode — click to grab, click again to drop (desktop)")]
    public bool toggleHolding = true;
    [Tooltip("Time (seconds) to blend the grab joint to full strength (old: 0.6)")]
    public float grabBlendDuration = 0.6f;
    [Tooltip("Item can be picked up")]
    public bool canBePickedUp = true;

    [Header("Throw")]
    [Tooltip("Multiplier for throw velocity")]
    public float throwForceMultiplier = 1.2f;
    [Tooltip("Maximum throw velocity magnitude")]
    public float maxThrowSpeed = 15f;

    // Runtime state
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public HandGrabSystem heldByHand;

    int originalLayer;
    bool originalGravity;
    bool originalKinematic;
    CollisionDetectionMode originalCollisionMode;
    Outline outline;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        outline = GetComponent<Outline>();
    }

    /// <summary>
    /// Called when a hand grabs this item.
    /// </summary>
    public void OnGrabbed(HandGrabSystem hand)
    {
        heldByHand = hand;
        originalLayer = gameObject.layer;
        originalGravity = rb.useGravity;
        originalKinematic = rb.isKinematic;
        originalCollisionMode = rb.collisionDetectionMode;

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (outline != null)
            outline.enabled = false;
    }

    /// <summary>
    /// Called when the hand releases this item. Applies throw velocity.
    /// </summary>
    public void OnReleased(Vector3 throwVelocity, Vector3 angularVelocity)
    {
        heldByHand = null;
        gameObject.layer = originalLayer;
        rb.isKinematic = originalKinematic;
        rb.useGravity = originalGravity;
        rb.collisionDetectionMode = originalCollisionMode;
        rb.interpolation = RigidbodyInterpolation.None;

        // Apply throw
        if (!rb.isKinematic)
        {
            var vel = throwVelocity * throwForceMultiplier;
            if (vel.magnitude > maxThrowSpeed)
                vel = vel.normalized * maxThrowSpeed;

            rb.linearVelocity = vel;
            rb.angularVelocity = angularVelocity;
        }
    }

    /// <summary>
    /// Show/hide the outline highlight.
    /// </summary>
    public void SetHighlight(bool show)
    {
        if (outline != null)
            outline.enabled = show;
    }

    /// <summary>
    /// Returns the hold position for the given hand side.
    /// </summary>
    public void GetHoldTransform(bool isLeftHand, out Vector3 pos, out Quaternion rot)
    {
        if (isLeftHand)
        {
            pos = holdPositionLeft;
            rot = Quaternion.Euler(holdRotationLeft);
        }
        else
        {
            pos = holdPositionRight;
            rot = Quaternion.Euler(holdRotationRight);
        }
    }

    public bool IsHeld => heldByHand != null;
}
