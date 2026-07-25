using UnityEngine;

public class SpoonMix : PlayerKB_GrabObject
{
    private bool _isMixing = false;
    private BowlLogic _currentBowl;
    private PotCollect _currentPot;

    [SerializeField] private GameObject _filling;
    bool _fillingVisible = false;

    protected override void Start()
    {
        base.Start();
        AssignInputs();

        OnGrabbedLeft += ShowHintLeft;
        OnGrabbedRight += ShowHintRight;
    }

    protected override void AssignInputs()
    {
        base.AssignInputs();
        _player.Controls.Viva.InteractLeft.performed += context => Mix(true);
        _player.Controls.Viva.InteractRight.performed += context => Mix(false);
        _player.Controls.Viva.InteractLeft.canceled += context => StopMixing();
        _player.Controls.Viva.InteractRight.canceled += context => StopMixing();
    }

    private void Update()
    {
        if (!_isMixing) { return; }
        if (_currentBowl != null) { _currentBowl.MixBatter(); }
        if (_currentPot != null) { _currentPot.MixIngredients(); }
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

        if (objectInOther.name.Equals("pot") && objectInOther.TryGetComponent(out PotCollect pot))
        {
            _isMixing = true;
            _currentPot = pot;
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

    public bool TrySetFillingVisible(bool visible, FruitCutMinigame.Fruit fruit) 
    { 
        if (visible == _fillingVisible) {  return false; }

        if (visible && _filling.TryGetComponent(out Renderer renderer))
        {
            switch (fruit)
            {
                case FruitCutMinigame.Fruit.Strawberry: renderer.material.color = Color.firebrick; break;
                case FruitCutMinigame.Fruit.Peach: renderer.material.color = Color.sandyBrown; break;
                case FruitCutMinigame.Fruit.Cantaloupe: renderer.material.color = Color.orangeRed; break;
                default: renderer.material.color = Color.tomato; break;
            }
        }
        else { return false; }

        _filling.SetActive(visible);
        _fillingVisible = visible;
        return true;
    }

    private void OnDestroy()
    {
        OnGrabbedLeft -= ShowHintLeft;
        OnGrabbedRight -= ShowHintRight;
    }
}
