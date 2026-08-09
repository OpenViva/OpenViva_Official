using UnityEngine;
using static FruitCutMinigame;

public class SpoonMix : PlayerKB_GrabObject
{
    private bool _isMixing = false;
    private BowlLogic _currentBowl;
    private PotLogic _currentPot;
    private AudioSource _audioSource;

    [SerializeField] private GameObject _filling;
    bool _fillingVisible = false;
    Fruit _fruitOnSpoon = Fruit.None;

    private void Awake()
    {
        _audioSource = GetComponentInChildren<AudioSource>();
    }

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
        _player.Controls.Viva.InteractLeft.performed += context => UseSpoon(true);
        _player.Controls.Viva.InteractRight.performed += context => UseSpoon(false);
        _player.Controls.Viva.InteractLeft.canceled += context => StopMixing();
        _player.Controls.Viva.InteractRight.canceled += context => StopMixing();
    }

    private void Update()
    {
        if (!_isMixing) { return; }
        if (_currentBowl != null) { _currentBowl.MixBatter(); }
        if (_currentPot != null) { _currentPot.MixIngredients(); }
    }

    private void UseSpoon(bool useLeft)
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

        if (objectInOther.name.Equals("pot") && objectInOther.TryGetComponent(out PotLogic pot))
        {
            _isMixing = true;
            _currentPot = pot;
        }

        if (objectInOther.name.Contains("toast") && objectInOther.TryGetComponent(out JamOnToast toast))
        {
            if (!_fillingVisible) { return; }

            bool itWorked = toast.TrySpreadJam(_fruitOnSpoon);
            if (itWorked) 
            { 
                _filling.SetActive(false);
                _fillingVisible = false; 
            }
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

    public bool TrySetFillingVisible(bool visible, Fruit fruit) 
    { 
        if (visible == _fillingVisible) {  return false; }

        if (visible && _filling.TryGetComponent(out Renderer renderer))
        {
            switch (fruit)
            {
                case Fruit.Strawberry: 
                    renderer.material.color = Color.firebrick;
                    _fruitOnSpoon = Fruit.Strawberry;
                    break;

                case Fruit.Peach: 
                    renderer.material.color = Color.sandyBrown;
                    _fruitOnSpoon =  Fruit.Peach;
                    break;

                case Fruit.Cantaloupe: 
                    renderer.material.color = Color.orangeRed; 
                    _fruitOnSpoon = Fruit.Cantaloupe;
                    break;

                default: 
                    renderer.material.color = Color.tomato;
                    _fruitOnSpoon = Fruit.None;
                    break;
            }
        }
        else { return false; }

        _filling.SetActive(visible);
        _fillingVisible = visible;
        _audioSource.Play();
        return true;
    }

    private void OnDestroy()
    {
        OnGrabbedLeft -= ShowHintLeft;
        OnGrabbedRight -= ShowHintRight;
    }
}
