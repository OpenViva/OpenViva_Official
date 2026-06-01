using UnityEngine;

public class PestleGrind : PlayerKB_GrabObject
{
    public bool IsGrinding = false;
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
        if (IsGrinding && _currentMortar != null)
        {
            _currentMortar.GrindWheat();
        }
    }

    private void GrindLeft()
    {
        if (!_isGrabbedInRight) { return; }

        GameObject objectInLeft = PlayerManager.Instance.GetObjectLeft();
        if (objectInLeft.name.Equals("mortar") && objectInLeft.TryGetComponent(out MortarLogic mortar))
        {
            IsGrinding = true;
            _currentMortar = mortar;
        }
    }

    private void GrindRight()
    {
        if (!_isGrabbedInLeft) { return; }

        if (PlayerManager.Instance.GetItemRight() == 15)
        {
            IsGrinding = true;
        }
    }

    private void StopGrinding()
    {
        IsGrinding = false;
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
        _player.Controls.Viva.InteractLeft.performed += context => GrindLeft();
        _player.Controls.Viva.InteractRight.performed += context => GrindRight();
        _player.Controls.Viva.InteractLeft.canceled += context => StopGrinding();
        _player.Controls.Viva.InteractRight.canceled += context => StopGrinding();
    }

    private void OnDestroy()
    {
        OnGrabbedLeft -= ShowLeftHint;
        OnGrabbedRight -= ShowRightHint;
    }
}
