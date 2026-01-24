using UnityEngine;

public class AnimationIndexes : MonoBehaviour
{

    private AnimationHandler _animationHandler = new AnimationHandler();

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
        }
    }
}
