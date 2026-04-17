using FIMSpace.FProceduralAnimation;
using NaughtyAttributes;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    private RagdollAnimator2 _ragdoll;

    [Header("Options")]
    public bool _useMuscleReduction = true;

    private void Awake()
    {
        _ragdoll = GetComponent<RagdollAnimator2>();

        if (_ragdoll == null)
        {
            Debug.LogError("RagdollAnimator2 component not found on this GameObject!");
        }
    }

    [Button("Turn Ragdoll ON (Falling Mode)", EButtonEnableMode.Playmode)]
    public void TurnRagdollOn()
    {
        _ragdoll.User_SwitchFallState(RagdollHandler.EAnimatingMode.Falling);

        if (_useMuscleReduction)
        {
            _ragdoll.User_FadeMusclesPower();
        }

        Debug.Log("Ragdoll ON - Falling mode activated");
    }

    [Button("Reset to Standing / Animated Pose", EButtonEnableMode.Playmode)]
    public void ResetToStanding()
    {
        _ragdoll.User_SwitchFallState(RagdollHandler.EAnimatingMode.Standing);

        if (_useMuscleReduction)
        {
            _ragdoll.User_FadeMusclesPower(1f, muscleResetDuration, 0f);
        }

        Debug.Log("Reset to Standing - animated pose + full muscle strength");
    }

    [Button("Fully Disable Ragdoll System", EButtonEnableMode.Playmode)]
    public void DisableRagdollSystem()
    {
        if (_ragdoll == null) return;

        _ragdoll.enabled = false;

        Debug.Log("Ragdoll system completely disabled");
    }

    [Header("Live Blend Testing")]
    [Range(0f, 1f)]
    public float liveBlend = 1f;

    [Header("Muscle Reset Duration")]
    [Range(0.1f, 1f)]
    public float muscleResetDuration = 1f;

    private void Update()
    {
        if (_ragdoll != null && Application.isPlaying)
        {
            if (Mathf.Abs(_ragdoll.RagdollBlend - liveBlend) > 0.01f)
            {
                _ragdoll.RagdollBlend = liveBlend;

                // Auto-switch modes when dragging the slider
                if (liveBlend > 50f && _ragdoll.AnimatingMode != RagdollHandler.EAnimatingMode.Falling)
                {
                    _ragdoll.User_SwitchFallState(false);
                }
                else if (liveBlend < 10f && _ragdoll.AnimatingMode != RagdollHandler.EAnimatingMode.Standing)
                {
                    _ragdoll.User_SwitchFallState(true);
                }
            }
        }
    }
}
