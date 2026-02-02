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

    public void HoldPeach(string hand)
    {
        PlayAnimation("holdPeach", hand);
    }

    public void HoldStrawberry(string hand)
    {
        PlayAnimation("holdStrawberry", hand);
    }

    public void HoldCantaloupe(string hand)
    {
        PlayAnimation("holdCantaloupe", hand);
    }

    public void HoldBlueberry(string hand)
    {
        PlayAnimation("holdBlueberry", hand);
    }

    public void HoldWheat(string hand)
    {
        PlayAnimation("holdWheat", hand);
    }

    public void HoldFlashlight(string hand)
    {
        PlayAnimation("holdFlashlight", hand);
    }

    public void HoldEgg(string hand)
    {
        PlayAnimation("holdEgg", hand);
    }

    public void HoldFlourJar(string hand)
    {
        PlayAnimation("holdFlourJar", hand);
    }

    public void HoldKnife(string hand)
    {
        PlayAnimation("holdKnife", hand);
    }

    public void HoldLantern(string hand)
    {
        PlayAnimation("holdLantern", hand);
    }

    public void HoldMilkCanister(string hand)
    {
        PlayAnimation("holdMilkCanister", hand);
    }

    public void HoldMixingBowl(string hand)
    {
        PlayAnimation("holdMixingBowl", hand);
    }

    public void HoldMixingSpoon(string hand)
    {
        PlayAnimation("holdMixingSpoon", hand);
    }

    public void HoldMortar(string hand)
    {
        PlayAnimation("holdMortar", hand);
    }

    public void HoldPestle(string hand)
    {
        PlayAnimation("holdPestle", hand);
    }

    public void HoldPot(string hand)
    {
        PlayAnimation("holdPot", hand);
    }

    public void HoldSoap(string hand)
    {
        PlayAnimation("holdSoap", hand);
    }

    public void HoldTowel(string hand)
    {
        PlayAnimation("holdTowel", hand);
    }
}
