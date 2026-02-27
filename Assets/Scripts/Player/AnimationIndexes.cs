using UnityEngine;

public class AnimationIndexes : MonoBehaviour
{

    // Tells the animation handler which animation to play based on the index provided and handedness

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
            case 2:
                _animationHandler.HoldPeach("Left");
                break;
            case 3:
                _animationHandler.HoldStrawberry("Left");
                break;
            case 4:
                _animationHandler.HoldCantaloupe("Left");
                break;
            case 5:
                _animationHandler.HoldBlueberry("Left");
                break;
            case 6:
                _animationHandler.HoldWheat("Left");
                break;
            case 7:
                _animationHandler.HoldFlashlight("Left");
                break;
            case 8:
                _animationHandler.HoldEgg("Left");
                break;
            case 9:
                _animationHandler.HoldFlourJar("Left");
                break;
            case 10:
                _animationHandler.HoldKnife("Left");
                break;
            case 11:
                _animationHandler.HoldLantern("Left");
                break;
            case 12:
                _animationHandler.HoldMilkCanister("Left");
                break;
            case 13:
                _animationHandler.HoldMixingBowl("Left");
                break;
            case 14:
                _animationHandler.HoldMixingSpoon("Left");
                break;
            case 15:
                _animationHandler.HoldMortar("Left");
                break;
            case 16:
                _animationHandler.HoldPestle("Left");
                break;
            case 17:
                _animationHandler.HoldPot("Left");
                break;
            case 18:
                _animationHandler.HoldSoap("Left");
                break;
            case 19:
                _animationHandler.HoldTowel("Left");
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
            case 2:
                _animationHandler.HoldPeach("Right");
                break;
            case 3:
                _animationHandler.HoldStrawberry("Right");
                break;
            case 4:
                _animationHandler.HoldCantaloupe("Right");
                break;
            case 5:
                _animationHandler.HoldBlueberry("Right");
                break;
            case 6:
                _animationHandler.HoldWheat("Right");
                break;
            case 7:
                _animationHandler.HoldFlashlight("Right");
                break;
            case 8:
                _animationHandler.HoldEgg("Right");
                break;
            case 9:
                _animationHandler.HoldFlourJar("Right");
                break;
            case 10:
                _animationHandler.HoldKnife("Right");
                break;
            case 11:
                _animationHandler.HoldLantern("Right");
                break;
            case 12:
                _animationHandler.HoldMilkCanister("Right");
                break;
            case 13:
                _animationHandler.HoldMixingBowl("Right");
                break;
            case 14:
                _animationHandler.HoldMixingSpoon("Right");
                break;
            case 15:
                _animationHandler.HoldMortar("Right");
                break;
            case 16:
                _animationHandler.HoldPestle("Right");
                break;
            case 17:
                _animationHandler.HoldPot("Right");
                break;
            case 18:
                _animationHandler.HoldSoap("Right");
                break;
            case 19:
                _animationHandler.HoldTowel("Right");
                break;
        }
    }
}
