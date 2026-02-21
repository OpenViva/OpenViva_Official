#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Desktop gesture system — replicates old OpenViva keyboard gestures.
/// F = Wave, G = Follow/Beckon, T = Point (hold), H = Present, Y = Headpat, U = Poke, J = ThumbsUp.
/// Uses direct Keyboard polling to avoid modifying the DesktopInput action asset.
/// Triggers hand animations via HandAnimator.PlayGesture() and notifies nearby NPCs.
/// </summary>
public class DesktopGestures : MonoBehaviour
{
    [Header("Hand Animators")]
    public HandAnimator leftHandAnimator;
    public HandAnimator rightHandAnimator;

    [Header("Hand Grab Systems")]
    public HandGrabSystem leftHandGrab;
    public HandGrabSystem rightHandGrab;

    [Header("Settings")]
    [Tooltip("Gesture cooldown in seconds")]
    public float gestureCooldown = 0.5f;

    float cooldownTimer;

    void Update()
    {
        if (Globals.isMenuOpen) return;
        if (Keyboard.current == null) return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // F = Wave (timed gesture)
        if (Keyboard.current.fKey.wasPressedThisFrame && cooldownTimer <= 0f)
        {
            PerformGesture(HandAnimator.Gesture.Wave, 1.5f);
            cooldownTimer = gestureCooldown;
        }

        // G = Follow / Beckon (timed gesture)
        if (Keyboard.current.gKey.wasPressedThisFrame && cooldownTimer <= 0f)
        {
            PerformGesture(HandAnimator.Gesture.Come, 1.2f);
            cooldownTimer = gestureCooldown;
        }

        // T = Point (hold to point, release to stop)
        if (Keyboard.current.tKey.wasPressedThisFrame)
            StartHoldGesture(HandAnimator.Gesture.Point);
        if (Keyboard.current.tKey.wasReleasedThisFrame)
            StopHoldGesture(HandAnimator.Gesture.Point);

        // H = Present hand (toggle — press to present, press again to stop)
        if (Keyboard.current.hKey.wasPressedThisFrame && cooldownTimer <= 0f)
        {
            TogglePresent();
            cooldownTimer = gestureCooldown;
        }

        // Y = Headpat pose (hold)
        if (Keyboard.current.yKey.wasPressedThisFrame)
            StartHoldGesture(HandAnimator.Gesture.Headpat);
        if (Keyboard.current.yKey.wasReleasedThisFrame)
            StopHoldGesture(HandAnimator.Gesture.Headpat);

        // U = Poke (hold — same as point but for NPC interaction)
        if (Keyboard.current.uKey.wasPressedThisFrame)
            StartHoldGesture(HandAnimator.Gesture.Poke);
        if (Keyboard.current.uKey.wasReleasedThisFrame)
            StopHoldGesture(HandAnimator.Gesture.Poke);

        // J = Thumbs up (timed)
        if (Keyboard.current.jKey.wasPressedThisFrame && cooldownTimer <= 0f)
        {
            PerformGesture(HandAnimator.Gesture.ThumbsUp, 1.5f);
            cooldownTimer = gestureCooldown;
        }
    }

    /// <summary>Play a timed gesture on the free hand.</summary>
    void PerformGesture(HandAnimator.Gesture gesture, float duration)
    {
        HandAnimator animator = GetFreeHandAnimator();
        if (animator != null)
        {
            animator.PlayGesture(gesture, duration);
            BroadcastGesture(gesture);
        }
    }

    /// <summary>Start a hold gesture (held while key is pressed).</summary>
    void StartHoldGesture(HandAnimator.Gesture gesture)
    {
        HandAnimator animator = GetFreeHandAnimator();
        if (animator != null)
        {
            animator.PlayGesture(gesture); // duration=0 → hold indefinitely
            BroadcastGesture(gesture);
        }
    }

    /// <summary>Stop a specific hold gesture if it's currently active.</summary>
    void StopHoldGesture(HandAnimator.Gesture gesture)
    {
        if (rightHandAnimator != null && rightHandAnimator.CurrentGesture == gesture)
            rightHandAnimator.ClearGesture();
        if (leftHandAnimator != null && leftHandAnimator.CurrentGesture == gesture)
            leftHandAnimator.ClearGesture();
    }

    /// <summary>Toggle present gesture (old OpenViva: present hand palm-up).</summary>
    void TogglePresent()
    {
        HandAnimator animator = GetFreeHandAnimator();
        if (animator == null) return;

        if (animator.CurrentGesture == HandAnimator.Gesture.PresentRight ||
            animator.CurrentGesture == HandAnimator.Gesture.PresentLeft)
        {
            animator.ClearGesture();
        }
        else
        {
            var gesture = (animator == rightHandAnimator)
                ? HandAnimator.Gesture.PresentRight
                : HandAnimator.Gesture.PresentLeft;
            animator.PlayGesture(gesture);
            BroadcastGesture(gesture);
        }
    }

    /// <summary>Get the free hand animator (prefer right, fallback left).</summary>
    HandAnimator GetFreeHandAnimator()
    {
        bool rightFree = rightHandGrab == null || !rightHandGrab.IsHolding;
        bool leftFree = leftHandGrab == null || !leftHandGrab.IsHolding;

        if (rightFree && rightHandAnimator != null) return rightHandAnimator;
        if (leftFree && leftHandAnimator != null) return leftHandAnimator;
        return null;
    }

    /// <summary>Broadcasts a gesture event to nearby NPCs.</summary>
    void BroadcastGesture(HandAnimator.Gesture gesture)
    {
        string gestureName = gesture.ToString().ToLower();
        var colliders = Physics.OverlapSphere(transform.position, 8f);
        foreach (var col in colliders)
        {
            var receiver = col.GetComponentInParent<IGestureReceiver>();
            if (receiver != null)
                receiver.OnPlayerGesture(gestureName, transform);
        }
    }
}

#endif
