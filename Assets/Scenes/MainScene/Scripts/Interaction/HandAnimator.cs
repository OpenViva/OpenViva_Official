using UnityEngine;

/// <summary>
/// Controls hand animations via Animator clip playback + code-driven finger bone curling.
/// 
/// Architecture:
/// - Animator plays hold clips (holdBag, holdKnife, etc.) via Play() — no parameters needed.
/// - When NOT holding: finger bones are driven by code in LateUpdate:
///   * VR: grip → thumb/middle/ring/pinky curl, trigger → index curl
///   * Gestures override finger pose entirely (point, wave, come, present, headpat, poke)
/// - When HOLDING: the Animator's hold clip drives finger bones, code does not override.
///
/// Replicates old OpenViva's FingerAnimator + HandAnimationSystem with code-driven poses
/// since the new project has no gesture animation clips.
/// </summary>
public class HandAnimator : MonoBehaviour
{
    public enum Gesture
    {
        None,
        Point,
        Wave,
        Come,
        PresentRight,
        PresentLeft,
        Headpat,
        HeadpatScrub,
        Poke,
        ThumbsUp,
        Fist
    }

    [Header("References")]
    public Animator handAnimator;
    public bool isLeftHand;

    [Header("Finger Bones")]
    [Tooltip("5 fingers × 3 bones = 15. Order: thumb(3), index(3), middle(3), ring(3), pinky(3)")]
    public Transform[] fingerBones = new Transform[15];
    public Transform wrist;

    [Header("Pose Blending")]
    public float poseBlendSpeed = 10f;
    [Tooltip("Max curl angle per joint in degrees")]
    public float maxCurlAngle = 90f;
    [Tooltip("Thumb max curl (less than fingers)")]
    public float thumbMaxCurl = 60f;

    // Current grip/trigger from VR controller (0-1)
    float currentGrip;
    float currentTrigger;
    float targetGrip;
    float targetTrigger;

    // Hold state
    bool isHolding;

    // Gesture state
    Gesture currentGesture = Gesture.None;
    float gestureTime;
    float gestureDuration;

    // Rest pose (captured at startup)
    Quaternion[] restPose = new Quaternion[15];
    bool hasRestPose;

    void Start()
    {
        SaveRestPose();
    }

    void Update()
    {
        // Smooth blend grip/trigger values
        currentGrip = Mathf.MoveTowards(currentGrip, targetGrip, poseBlendSpeed * Time.deltaTime);
        currentTrigger = Mathf.MoveTowards(currentTrigger, targetTrigger, poseBlendSpeed * Time.deltaTime);

        // Advance gesture timer
        if (currentGesture != Gesture.None)
        {
            gestureTime += Time.deltaTime;
            // Auto-expire timed gestures (wave, come)
            if (gestureDuration > 0f && gestureTime >= gestureDuration)
                currentGesture = Gesture.None;
        }
    }

    void LateUpdate()
    {
        if (!hasRestPose || fingerBones == null || fingerBones.Length < 15) return;
        if (fingerBones[0] == null) return;

        // When holding an item, the Animator's hold clip drives finger bones — don't override
        if (isHolding) return;

        if (currentGesture != Gesture.None)
            ApplyGesturePose();
        else
            ApplyGripTriggerPose();
    }

    #region Public API

    /// <summary>Set the grip amount (0=open, 1=fist). VR controller input.</summary>
    public void SetGrip(float value)
    {
        targetGrip = Mathf.Clamp01(value);
    }

    /// <summary>Set the trigger/index finger amount (0=extended, 1=curled). VR controller input.</summary>
    public void SetTrigger(float value)
    {
        targetTrigger = Mathf.Clamp01(value);
    }

    /// <summary>Play the appropriate hold animation for a grabbed item.</summary>
    public void SetHoldPose(GrabbableItem item)
    {
        if (item == null) { ClearHoldPose(); return; }

        isHolding = true;

        if (handAnimator != null)
        {
            string clipName = GetHoldClipName(item.holdPose);
            if (!string.IsNullOrEmpty(clipName))
                handAnimator.Play(clipName, 0, 0f);
        }
    }

    /// <summary>Return to idle hand pose.</summary>
    public void ClearHoldPose()
    {
        isHolding = false;
        currentGesture = Gesture.None;

        if (handAnimator != null)
            handAnimator.Play("handIdle", 0, 0f);
    }

    /// <summary>Start a gesture. Duration=0 means hold until ClearGesture/other gesture.</summary>
    public void PlayGesture(Gesture gesture, float duration = 0f)
    {
        if (isHolding) return; // Can't gesture while holding
        currentGesture = gesture;
        gestureTime = 0f;
        gestureDuration = duration;

        // Play idle animation as base — fingers will be overridden in LateUpdate
        if (handAnimator != null)
            handAnimator.Play("handIdle", 0, 0f);
    }

    /// <summary>Stop current gesture, return to grip/trigger-driven pose.</summary>
    public void ClearGesture()
    {
        currentGesture = Gesture.None;
    }

    /// <summary>Get current gesture.</summary>
    public Gesture CurrentGesture => currentGesture;

    /// <summary>Is the hand currently holding an item?</summary>
    public bool IsHolding => isHolding;

    #endregion

    #region Finger Curling

    /// <summary>Apply VR grip/trigger to finger bones.</summary>
    void ApplyGripTriggerPose()
    {
        // Thumb follows grip partially
        CurlFinger(0, currentGrip * 0.7f, thumbMaxCurl);
        // Index follows trigger
        CurlFinger(1, currentTrigger, maxCurlAngle);
        // Middle, ring, pinky follow grip
        CurlFinger(2, currentGrip, maxCurlAngle);
        CurlFinger(3, currentGrip, maxCurlAngle);
        CurlFinger(4, currentGrip, maxCurlAngle);
    }

    /// <summary>Apply gesture-specific finger pose.</summary>
    void ApplyGesturePose()
    {
        switch (currentGesture)
        {
            case Gesture.Point:
            case Gesture.Poke:
                // Index extended, others curled
                CurlFinger(0, 0.8f, thumbMaxCurl);
                CurlFinger(1, 0f, maxCurlAngle);
                CurlFinger(2, 1f, maxCurlAngle);
                CurlFinger(3, 1f, maxCurlAngle);
                CurlFinger(4, 1f, maxCurlAngle);
                break;

            case Gesture.Wave:
                // Hand open, slight wave oscillation on fingers
                float wave = Mathf.Sin(gestureTime * 6f) * 0.15f;
                CurlFinger(0, 0.1f + wave, thumbMaxCurl);
                CurlFinger(1, 0.05f + wave * 0.8f, maxCurlAngle);
                CurlFinger(2, 0.05f + wave, maxCurlAngle);
                CurlFinger(3, 0.05f - wave * 0.5f, maxCurlAngle);
                CurlFinger(4, 0.1f - wave, maxCurlAngle);
                break;

            case Gesture.Come:
                // Beckoning: index and middle curl in/out rhythmically
                float beck = (Mathf.Sin(gestureTime * 5f) + 1f) * 0.5f; // 0→1
                CurlFinger(0, 0.3f, thumbMaxCurl);
                CurlFinger(1, beck, maxCurlAngle);
                CurlFinger(2, beck * 0.8f, maxCurlAngle);
                CurlFinger(3, 0.6f, maxCurlAngle);
                CurlFinger(4, 0.7f, maxCurlAngle);
                break;

            case Gesture.PresentRight:
            case Gesture.PresentLeft:
                // Hand flat open, palm up/forward
                CurlFinger(0, 0.15f, thumbMaxCurl);
                CurlFinger(1, 0f, maxCurlAngle);
                CurlFinger(2, 0f, maxCurlAngle);
                CurlFinger(3, 0f, maxCurlAngle);
                CurlFinger(4, 0.05f, maxCurlAngle);
                break;

            case Gesture.Headpat:
                // Fingers slightly spread and curled, relaxed patting pose
                CurlFinger(0, 0.2f, thumbMaxCurl);
                CurlFinger(1, 0.25f, maxCurlAngle);
                CurlFinger(2, 0.3f, maxCurlAngle);
                CurlFinger(3, 0.3f, maxCurlAngle);
                CurlFinger(4, 0.35f, maxCurlAngle);
                break;

            case Gesture.HeadpatScrub:
                // Like headpat but with slight oscillation (scrubbing)
                float scrub = Mathf.Sin(gestureTime * 8f) * 0.1f;
                CurlFinger(0, 0.25f + scrub, thumbMaxCurl);
                CurlFinger(1, 0.3f + scrub, maxCurlAngle);
                CurlFinger(2, 0.35f + scrub, maxCurlAngle);
                CurlFinger(3, 0.35f - scrub, maxCurlAngle);
                CurlFinger(4, 0.4f - scrub, maxCurlAngle);
                break;

            case Gesture.ThumbsUp:
                // Thumb extended, all others curled
                CurlFinger(0, 0f, thumbMaxCurl);
                CurlFinger(1, 1f, maxCurlAngle);
                CurlFinger(2, 1f, maxCurlAngle);
                CurlFinger(3, 1f, maxCurlAngle);
                CurlFinger(4, 1f, maxCurlAngle);
                break;

            case Gesture.Fist:
                // Everything curled
                CurlFinger(0, 1f, thumbMaxCurl);
                CurlFinger(1, 1f, maxCurlAngle);
                CurlFinger(2, 1f, maxCurlAngle);
                CurlFinger(3, 1f, maxCurlAngle);
                CurlFinger(4, 1f, maxCurlAngle);
                break;
        }
    }

    /// <summary>Curl one finger (3 joints) by an amount (0=rest, 1=max curl).</summary>
    void CurlFinger(int fingerIndex, float curl, float maxAngle)
    {
        curl = Mathf.Clamp01(curl);
        for (int j = 0; j < 3; j++)
        {
            int boneIndex = fingerIndex * 3 + j;
            if (boneIndex >= fingerBones.Length || fingerBones[boneIndex] == null) continue;

            Quaternion rest = restPose[boneIndex];
            // Curl around local X axis (standard finger bend axis)
            // DIP/PIP joints curl more than MCP on middle joints
            float jointScale = (j == 0) ? 0.7f : 1f;
            Quaternion curled = rest * Quaternion.Euler(curl * maxAngle * jointScale, 0f, 0f);

            fingerBones[boneIndex].localRotation = Quaternion.Slerp(
                fingerBones[boneIndex].localRotation,
                curled,
                Time.deltaTime * poseBlendSpeed
            );
        }
    }

    #endregion

    #region Utility

    void SaveRestPose()
    {
        if (fingerBones == null || fingerBones.Length < 15) return;
        for (int i = 0; i < Mathf.Min(fingerBones.Length, restPose.Length); i++)
        {
            if (fingerBones[i] != null)
                restPose[i] = fingerBones[i].localRotation;
        }
        hasRestPose = true;
    }

    string GetHoldClipName(GrabbableItem.HoldPose pose)
    {
        return pose switch
        {
            GrabbableItem.HoldPose.Bag => "holdBag",
            GrabbableItem.HoldPose.RubberDucky => "holdRubberDucky",
            GrabbableItem.HoldPose.Peach => "holdPeach",
            GrabbableItem.HoldPose.Strawberry => "holdStrawberry",
            GrabbableItem.HoldPose.Cantaloupe => "holdCantaloupe",
            GrabbableItem.HoldPose.Blueberry => "holdBlueberry",
            GrabbableItem.HoldPose.Wheat => "holdWheat",
            GrabbableItem.HoldPose.Flashlight => "holdFlashlight",
            GrabbableItem.HoldPose.Egg => "holdEgg",
            GrabbableItem.HoldPose.FlourJar => "holdFlourJar",
            GrabbableItem.HoldPose.Knife => "holdKnife",
            GrabbableItem.HoldPose.Lantern => "holdLantern",
            GrabbableItem.HoldPose.MilkCanister => "holdMilkCanister",
            GrabbableItem.HoldPose.MixingBowl => "holdMixingBowl",
            GrabbableItem.HoldPose.MixingSpoon => "holdMixingSpoon",
            GrabbableItem.HoldPose.Mortar => "holdMortar",
            GrabbableItem.HoldPose.Pestle => "holdPestle",
            GrabbableItem.HoldPose.Pot => "holdPot",
            GrabbableItem.HoldPose.Soap => "holdSoap",
            GrabbableItem.HoldPose.Towel => "holdTowel",
            _ => "handIdle"
        };
    }

    #endregion
}
