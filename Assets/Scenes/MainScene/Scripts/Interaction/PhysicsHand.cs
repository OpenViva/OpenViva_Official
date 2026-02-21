using UnityEngine;

/// <summary>
/// [DEPRECATED] Physics hand following is now built into HandGrabSystem.ApplyHandPhysics().
/// This script is kept for backward compatibility but is no longer needed.
/// HandGrabSystem now drives its own Rigidbody with velocity forces matching old OpenViva.
/// </summary>
public class PhysicsHand : MonoBehaviour
{
    // All hand physics is now in HandGrabSystem. This class is intentionally empty.
    // Remove this component from any GameObjects that have it.
}
