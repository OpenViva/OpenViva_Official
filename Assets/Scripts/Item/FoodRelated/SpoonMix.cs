using UnityEngine;

public class SpoonMix : PlayerKB_GrabObject
{
    private bool _isMixing = false;
    private BowlLogic _currentBowl;

    protected override void Start()
    {
        base.Start();
        AssignInputs();

        OnGrabbedLeft += ShowHintLeft;
        OnGrabbedRight += ShowHintRight;
    }

    private void Update()
    {
        if (_isMixing &&  _currentBowl != null)
        {
            _currentBowl.MixBatter();
        }
    }

    private void Mix(bool useLeft)
    {
        if (useLeft && !_isGrabbedInRight) { return; }
        else if (!useLeft && !_isGrabbedInLeft) { return; }

        GameObject objectInOther;
        if (useLeft)
        {
            objectInOther = PlayerManager.Instance.GetObjectLeft();
        }
        else
        {
            objectInOther = PlayerManager.Instance.GetObjectRight();
        }

        if (objectInOther == null) { return; }

        if (objectInOther.name.Equals("mixingBowl") && objectInOther.TryGetComponent(out BowlLogic bowl))
        {
            _isMixing = true;
            _currentBowl = bowl;
        }
    }

    private void StopMixing()
    {
        _isMixing = false;
    }

    private void ShowHintLeft(bool show)
    {
        if (show) { _hud.CreateHint(HintConstants.RightMixHint); }
        else { _hud.ClearHint(HintConstants.RightMixHint); }
    }

    private void ShowHintRight(bool show)
    {
        if (show) { _hud.CreateHint(HintConstants.LeftMixHint); }
        else { _hud.ClearHint(HintConstants.LeftMixHint); }
    }

    protected override void AssignInputs()
    {
        base.AssignInputs();
        _player.Controls.Viva.InteractLeft.performed += context => Mix(true);
        _player.Controls.Viva.InteractRight.performed += context => Mix(false);
        _player.Controls.Viva.InteractLeft.canceled += context => StopMixing();
        _player.Controls.Viva.InteractRight.canceled += context => StopMixing();
    }

    private void OnDestroy()
    {
        OnGrabbedLeft -= ShowHintLeft;
        OnGrabbedRight -= ShowHintRight;
    }
}
