#if UNITY_EDITOR || UNITY_ANDROID

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// VR gesture support — maps grip/trigger button events to HandAnimator gestures.
/// This supplements PlayerVR_GrabController (which handles analog grip/trigger for finger curling).
/// Provides gestures like point (grip only), fist (trigger only), etc.
/// Also handles VR-specific gesture buttons (e.g., thumbstick click for wave/come).
/// </summary>
public class PlayerVR_BasicActions : MonoBehaviour
{
    [Header("Hand Animators")]
    [SerializeField] private HandAnimator leftHandAnimator;
    [SerializeField] private HandAnimator rightHandAnimator;

    [Header("VR Gesture Input Actions")]
    [Tooltip("Optional button inputs for gestures (e.g., A/B/X/Y buttons)")]
    [SerializeField] private InputActionReference leftPrimaryButtonAction;
    [SerializeField] private InputActionReference rightPrimaryButtonAction;
    [SerializeField] private InputActionReference leftSecondaryButtonAction;
    [SerializeField] private InputActionReference rightSecondaryButtonAction;

    void OnEnable()
    {
        EnableAction(leftPrimaryButtonAction);
        EnableAction(rightPrimaryButtonAction);
        EnableAction(leftSecondaryButtonAction);
        EnableAction(rightSecondaryButtonAction);

        BindAction(leftPrimaryButtonAction, OnLeftPrimary, OnLeftPrimaryRelease);
        BindAction(rightPrimaryButtonAction, OnRightPrimary, OnRightPrimaryRelease);
        BindAction(leftSecondaryButtonAction, OnLeftSecondary, null);
        BindAction(rightSecondaryButtonAction, OnRightSecondary, null);
    }

    void OnDisable()
    {
        UnbindAction(leftPrimaryButtonAction, OnLeftPrimary, OnLeftPrimaryRelease);
        UnbindAction(rightPrimaryButtonAction, OnRightPrimary, OnRightPrimaryRelease);
        UnbindAction(leftSecondaryButtonAction, OnLeftSecondary, null);
        UnbindAction(rightSecondaryButtonAction, OnRightSecondary, null);

        DisableAction(leftPrimaryButtonAction);
        DisableAction(rightPrimaryButtonAction);
        DisableAction(leftSecondaryButtonAction);
        DisableAction(rightSecondaryButtonAction);
    }

    // Primary button (A/X) = Point gesture (hold)
    void OnLeftPrimary(InputAction.CallbackContext ctx)
    {
        if (leftHandAnimator != null && !leftHandAnimator.IsHolding)
            leftHandAnimator.PlayGesture(HandAnimator.Gesture.Point);
    }

    void OnLeftPrimaryRelease(InputAction.CallbackContext ctx)
    {
        if (leftHandAnimator != null && leftHandAnimator.CurrentGesture == HandAnimator.Gesture.Point)
            leftHandAnimator.ClearGesture();
    }

    void OnRightPrimary(InputAction.CallbackContext ctx)
    {
        if (rightHandAnimator != null && !rightHandAnimator.IsHolding)
            rightHandAnimator.PlayGesture(HandAnimator.Gesture.Point);
    }

    void OnRightPrimaryRelease(InputAction.CallbackContext ctx)
    {
        if (rightHandAnimator != null && rightHandAnimator.CurrentGesture == HandAnimator.Gesture.Point)
            rightHandAnimator.ClearGesture();
    }

    // Secondary button (B/Y) = Wave gesture (timed)
    void OnLeftSecondary(InputAction.CallbackContext ctx)
    {
        if (leftHandAnimator != null && !leftHandAnimator.IsHolding)
            leftHandAnimator.PlayGesture(HandAnimator.Gesture.Wave, 1.5f);
    }

    void OnRightSecondary(InputAction.CallbackContext ctx)
    {
        if (rightHandAnimator != null && !rightHandAnimator.IsHolding)
            rightHandAnimator.PlayGesture(HandAnimator.Gesture.Wave, 1.5f);
    }

    #region Utility

    void EnableAction(InputActionReference actionRef)
    {
        if (actionRef != null && actionRef.action != null) actionRef.action.Enable();
    }

    void DisableAction(InputActionReference actionRef)
    {
        if (actionRef != null && actionRef.action != null) actionRef.action.Disable();
    }

    void BindAction(InputActionReference actionRef,
                     System.Action<InputAction.CallbackContext> performed,
                     System.Action<InputAction.CallbackContext> canceled)
    {
        if (actionRef == null || actionRef.action == null) return;
        if (performed != null) actionRef.action.performed += performed;
        if (canceled != null) actionRef.action.canceled += canceled;
    }

    void UnbindAction(InputActionReference actionRef,
                      System.Action<InputAction.CallbackContext> performed,
                      System.Action<InputAction.CallbackContext> canceled)
    {
        if (actionRef == null || actionRef.action == null) return;
        if (performed != null) actionRef.action.performed -= performed;
        if (canceled != null) actionRef.action.canceled -= canceled;
    }

    #endregion
}

#endif