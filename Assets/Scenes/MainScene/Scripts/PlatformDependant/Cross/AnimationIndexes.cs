using UnityEngine;

public class AnimationIndexes : MonoBehaviour
{
    
    private AnimationHandler _animationHandler;

    private void Start()
    {
        _animationHandler = GetComponent<AnimationHandler>();
    }

    public void PlayAnimationLeft(int index)
    {
        switch (index)
        {
            case -1:
                _animationHandler.Idle("Left");
                break;
            case 0:
                _animationHandler.HoldBag("Left");
                break;
            case 1:
                _animationHandler.HoldRubberDucky("Left");
                break;
        }
    }

    public void PlayAnimationRight(int index)
    {
        switch (index)
        {
            case -1:
                _animationHandler.Idle("Right");
                break;
            case 0:
                _animationHandler.HoldBag("Right");
                break;
            case 1:
                _animationHandler.HoldRubberDucky("Right");
                break;
        }
    }
}
