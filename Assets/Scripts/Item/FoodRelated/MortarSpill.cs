using UnityEngine;

public class MortarSpill : PlayerKB_GrabObject
{
    [SerializeField] private ParticleSystem _flourParticle;
    private MortarLogic _logic;

    protected override void Start()
    {
        base.Start();
        AssignInputs();

        OnGrabbedLeft += ShowLeftHint;
        OnGrabbedRight += ShowRightHint;

        _logic = GetComponent<MortarLogic>();
    }

    private void Spill(bool useLeft)
    {
        if (useLeft && !_isGrabbedInLeft) { return; }
        else if (!useLeft && !_isGrabbedInRight) { return; }

        int flourToSpill = (int)_logic.FlourQuantity / 100;
        for (int i = 0; i < flourToSpill; i++)
        {
            _flourParticle.Play();
        }

        _logic.SpillWheat();
    }

    private void ShowRightHint(bool show)
    {
        if (show) { _hud.CreateHint(HintConstants.LeftMortarSpillHint); }
        else { _hud.ClearHint(HintConstants.LeftMortarSpillHint); }
    }

    private void ShowLeftHint(bool show)
    {
        if (show) { _hud.CreateHint(HintConstants.RightMortarSpillHint); }
        else { _hud.ClearHint(HintConstants.RightMortarSpillHint); }
    }
            

    protected override void AssignInputs()
    {
        base.AssignInputs();
        _player.Controls.Viva.InteractRightHold.performed += context => Spill(true);
        _player.Controls.Viva.InteractLeftHold.performed += context => Spill(false);
    }

    private void OnDestroy()
    {
        OnGrabbedLeft -= ShowLeftHint;
        OnGrabbedRight -= ShowRightHint;
    }
}
