using UnityEngine;

public class EggCrack : Ingredient
{
    private float _speed = 0;
    private float t = 0;

    private Player _player;
    private PlayerKB_GrabObject _grabScript;

    private void Awake()
    {
        _player = FindFirstObjectByType<Player>();
        _grabScript = GetComponent<PlayerKB_GrabObject>();
    }

    protected override void Start()
    {
        base.Start();
        _player.Controls.Viva.InteractLeft.performed += context => TryCrackLeft();
        _player.Controls.Viva.InteractRight.performed += context => TryCrackRight();
        _grabScript.OnGrabbedLeft += ShowLeftHint;
        _grabScript.OnGrabbedRight += ShowRightHint;
    }

    protected override void Update()
    {
        CalcDeltaMomentum();
    }

    private void CalcDeltaMomentum()
    {
        t += Time.deltaTime;
        if (t > (3 / 60f))
        {
            t = 0;
            if ((rb.linearVelocity.magnitude + 2) < _speed)
            {
                SpawnConversion();
            }
            else
            {
                _speed = rb.linearVelocity.magnitude;
            }
        }
    }

    private void TryCrackLeft()
    {
        if (_grabScript.GetIsGrabbed() == 1)
        {
            _grabScript.SetIsActive(false, 1, 8);
            SpawnConversion();
        }
    }

    private void TryCrackRight()
    {
        if (_grabScript.GetIsGrabbed() == 2)
        {
            _grabScript.SetIsActive(false, 2, 8);
            SpawnConversion();
        }
    }

    private void ShowLeftHint(bool show)
    {
        if (show) { _hud.CreateHint(HintConstants.LeftCrackEggHint); }
        else { _hud.ClearHint(HintConstants.LeftCrackEggHint); }
    }

    private void ShowRightHint(bool show)
    {
        if (show) { _hud.CreateHint(HintConstants.RightCrackEggHint); }
        else { _hud.ClearHint(HintConstants.RightCrackEggHint); }
    }
}
