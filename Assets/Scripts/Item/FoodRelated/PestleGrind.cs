using UnityEngine;

public class PestleGrind : PlayerKB_GrabObject
{
    private bool _isGrinding = false;
    private MortarLogic _currentMortar;

    protected override void Start()
    {
        base.Start();
        AssignInputs();

        OnGrabbedLeft += ShowLeftHint;
        OnGrabbedRight += ShowRightHint;
    }

    private void Update()
    {
        if (_isGrinding && _currentMortar != null)
        {
            _currentMortar.GrindWheat();
        }
    }

    private void Grind(bool useLeft)
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

        if (objectInOther.name.Equals("mortar") && objectInOther.TryGetComponent(out MortarLogic mortar))
        {
            _isGrinding = true;
            _currentMortar = mortar;
        }
    }

    private void StopGrinding()
    {
        _isGrinding = false;
    }

    private void ShowLeftHint(bool show)
    {
        if (show) { _hud.CreateHint(HintConstants.RightGrindHint); }
        else { _hud.ClearHint(HintConstants.RightGrindHint); }
    }

    private void ShowRightHint(bool show)
    {
        if (show) { _hud.CreateHint(HintConstants.LeftGrindHint); }
        else { _hud.ClearHint(HintConstants.LeftGrindHint); }
    }

    protected override void AssignInputs()
    {
        base.AssignInputs();
        _player.Controls.Viva.InteractLeft.performed += context => Grind(true);
        _player.Controls.Viva.InteractRight.performed += context => Grind(false);
        _player.Controls.Viva.InteractLeft.canceled += context => StopGrinding();
        _player.Controls.Viva.InteractRight.canceled += context => StopGrinding();
    }

    private void OnDestroy()
    {
        OnGrabbedLeft -= ShowLeftHint;
        OnGrabbedRight -= ShowRightHint;
    }
}
