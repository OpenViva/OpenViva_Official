using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    [SerializeField] private Animator _leftWrist;
    [SerializeField] private Animator _rightWrist;

    private void PlayAnimation(string clipname, string hand)
    {
        if (hand == "Left")
        {
            _leftWrist.Play(clipname);
        }
        else if (hand == "Right")
        {
            _rightWrist.Play(clipname);
        }
    }

    public void Idle(string hand)
    {
        PlayAnimation("handIdle", hand);
    }

    public void HoldBag(string hand)
    {
        PlayAnimation("holdBag", hand);
    }

    public void HoldRubberDucky(string hand)
    {
        PlayAnimation("holdRubberDucky", hand);
    }
}
