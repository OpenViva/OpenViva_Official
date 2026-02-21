#if UNITY_EDITOR || UNITY_ANDROID

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// VR grab controller. Uses grip button to grab/release items via HandGrabSystem.
/// Integrates with PhysicsHand for realistic velocity-driven hand movement.
/// Trigger is used for interaction (use items), grip for grab/release.
/// </summary>
public class PlayerVR_GrabController : MonoBehaviour
{
    [Header("Hand Systems")]
    public HandGrabSystem leftHandGrab;
    public HandGrabSystem rightHandGrab;
    public PhysicsHand leftPhysicsHand;
    public PhysicsHand rightPhysicsHand;

    [Header("Hand Animators")]
    public HandAnimator leftHandAnimator;
    public HandAnimator rightHandAnimator;

    [Header("VR Input Actions")]
    [Tooltip("0: Left Grip, 1: Left Trigger, 2: Right Grip, 3: Right Trigger")]
    [SerializeField] private InputActionReference leftGripAction;
    [SerializeField] private InputActionReference leftTriggerAction;
    [SerializeField] private InputActionReference rightGripAction;
    [SerializeField] private InputActionReference rightTriggerAction;

    [Header("Grab Threshold")]
    [Tooltip("Grip value above this triggers a grab")]
    public float gripThreshold = 0.7f;

    // Grip state tracking
    bool leftGripActive;
    bool rightGripActive;
    float leftGripValue;
    float rightGripValue;
    float leftTriggerValue;
    float rightTriggerValue;

    void OnEnable()
    {
        EnableAction(leftGripAction);
        EnableAction(leftTriggerAction);
        EnableAction(rightGripAction);
        EnableAction(rightTriggerAction);

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
    }

    void OnDisable()
    {
        DisableAction(leftGripAction);
        DisableAction(leftTriggerAction);
        DisableAction(rightGripAction);
        DisableAction(rightTriggerAction);

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

    void Update()
    {
        // Read continuous grip/trigger values
        leftGripValue = ReadActionValue(leftGripAction);
        leftTriggerValue = ReadActionValue(leftTriggerAction);
        rightGripValue = ReadActionValue(rightGripAction);
        rightTriggerValue = ReadActionValue(rightTriggerAction);

        // Update hand animations with analog values
        if (leftHandAnimator != null)
        {
            leftHandAnimator.SetGrip(leftGripValue);
            leftHandAnimator.SetTrigger(leftTriggerValue);
        }
        if (rightHandAnimator != null)
        {
            rightHandAnimator.SetGrip(rightGripValue);
            rightHandAnimator.SetTrigger(rightTriggerValue);
        }

        // Handle left hand grab/release with hysteresis
        ProcessGrip(leftGripValue, ref leftGripActive, leftHandGrab);
        ProcessGrip(rightGripValue, ref rightGripActive, rightHandGrab);
    }

    void ProcessGrip(float gripValue, ref bool gripActive, HandGrabSystem hand)
    {
        if (hand == null) return;

        if (!gripActive && gripValue >= gripThreshold)
        {
            gripActive = true;
            if (!hand.IsHolding)
                hand.AttemptGrab();
        }
        else if (gripActive && gripValue < gripThreshold * 0.5f) // Hysteresis: release at half threshold
        {
            gripActive = false;
            if (hand.IsHolding)
                hand.ReleaseItem(true);
        }
    }

    #region Grab/Release Callbacks

    void OnLeftGrabbed(GrabbableItem item)
    {
        if (leftHandAnimator != null)
            leftHandAnimator.SetHoldPose(item);
    }

    void OnLeftReleased(GrabbableItem item)
    {
        if (leftHandAnimator != null)
            leftHandAnimator.ClearHoldPose();
    }

    void OnRightGrabbed(GrabbableItem item)
    {
        if (rightHandAnimator != null)
            rightHandAnimator.SetHoldPose(item);
    }

    void OnRightReleased(GrabbableItem item)
    {
        if (rightHandAnimator != null)
            rightHandAnimator.ClearHoldPose();
    }

    #endregion

    #region Utility

    float ReadActionValue(InputActionReference actionRef)
    {
        if (actionRef == null || actionRef.action == null) return 0f;
        return actionRef.action.ReadValue<float>();
    }

    void EnableAction(InputActionReference actionRef)
    {
        if (actionRef != null && actionRef.action != null)
            actionRef.action.Enable();
    }

    void DisableAction(InputActionReference actionRef)
    {
        if (actionRef != null && actionRef.action != null)
            actionRef.action.Disable();
    }

    #endregion
}

#endif
