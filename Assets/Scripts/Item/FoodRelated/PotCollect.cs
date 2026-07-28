using UnityEngine;
using static FruitCutMinigame;

public class PotCollect : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private HintManager _HUD;

    [SerializeField] private SkinnedMeshRenderer _potWater;
    private float _volume = 0f;
    [SerializeField] private float _maxCapacity = 5000f;
    private float _multiplier = 1;

    private float _strawberryPieces = 0;
    private float _peachPieces = 0;
    private float _cantaloupePieces = 0;

    private bool _isInOven = false;
    private float _temperature = 25;

    private PlayerKB_GrabObject _grabScript;

    private float _strawberryJam = 0;
    private float _peachJam = 0;
    private float _cantaloupeJam = 0;

    private void Awake()
    {
        _grabScript = GetComponent<PlayerKB_GrabObject>();
    }

    private void Start()
    {
        AssignInputs();
    }

    private void AssignInputs()
    {
        _grabScript.OnGrabbedLeft += OnPotGrabbedLeft;
        _grabScript.OnGrabbedRight += OnPotGrabbedRight;

        _player.Controls.Viva.InteractLeft.performed += context => ScoopJam(true);
        _player.Controls.Viva.InteractRight.performed += context => ScoopJam(false);
    }

    private void Update()
    {
        if (_isInOven && _temperature < 85)  { _temperature += ((_multiplier * 1.2f) + 0.3f) * Time.deltaTime; }
        else if (!_isInOven && _temperature > 25)  { _temperature -= ((_multiplier * 0.8f) + 0.2f) * Time.deltaTime; }
    }

    private void OnParticleCollision(GameObject collider)
    {
        if (collider.gameObject.name.Equals("TapFX"))
        {
            if (_volume >= _maxCapacity) { return; }
            _volume += 20f;
            if (_temperature > 25.5f) { _temperature -= 2f; }
            SetBlendShape();
            _multiplier = 1f - (_volume  / _maxCapacity);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Oven")) { _isInOven = true; }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Contains("Oven")) { _isInOven = false; ; }
    }

    private void OnPotGrabbedLeft(bool held) 
    { 
        CuttingBoard.Instance.LastTouchedPot = this;

        if (held) { _HUD.CreateHint(HintConstants.RightScoopJamHint); }
        else { _HUD.ClearHint(HintConstants.RightScoopJamHint); }
    }

    private void OnPotGrabbedRight(bool held) 
    { 
        CuttingBoard.Instance.LastTouchedPot = this;

        if (held) { _HUD.CreateHint(HintConstants.LeftScoopJamHint); }
        else { _HUD.ClearHint(HintConstants.LeftScoopJamHint); }
    }

    public void MixIngredients()
    {
        if (_volume <= 0 || (_strawberryPieces <= 0.5f && _peachPieces <= 0.5f && _cantaloupePieces <= 0.5f) || _temperature < 65) { return; }

        if (_strawberryPieces > 0) 
        { 
            _strawberryJam += 0.01f;
            _strawberryPieces -= 0.02f;
        }
        if (_peachPieces > 0) 
        {
            _peachJam += 0.01f;
            _peachPieces -= 0.01f;
        }
        if (_cantaloupePieces > 0) 
        { 
            _cantaloupeJam += 0.01f;
            _cantaloupePieces -= 0.005f;
        }

        _volume -= 0.5f;
    }

    public void CollectFruitPieces(Fruit fruit)
    {
        switch (fruit)
        {
            case Fruit.Strawberry: _strawberryPieces += 2; break;
            case Fruit.Peach: _peachPieces += 8; break;
            case Fruit.Cantaloupe: _cantaloupePieces += 20; break;
        }
    }

    private void ScoopJam(bool leftTriggered)
    {
        if (_strawberryJam < 1f && _peachJam < 1f && _cantaloupeJam < 1f) { return; }
        else if (leftTriggered && (_grabScript.IsGrabbedLeft || !_grabScript.IsGrabbedRight)) { return; }
        else if (!leftTriggered && (_grabScript.IsGrabbedRight || !_grabScript.IsGrabbedLeft)) { return; }

        GameObject other = null;
        if (leftTriggered) { other = PlayerManager.Instance.GetObjectLeft(); }
        else { other = PlayerManager.Instance.GetObjectRight(); }
        SpoonMix spoonScript = other.GetComponent<SpoonMix>();
        if (spoonScript == null) { return; }

        string combination = "";
        if (_strawberryJam >= 1f) { combination += "s"; }
        if (_peachJam >= 1f) { combination += "p"; }
        if (_cantaloupeJam >= 1f) { combination += "c"; }

        switch (combination)
        {
            case "spc":
                if (spoonScript.TrySetFillingVisible(true, Fruit.None))
                {
                    _strawberryJam -= 1 / 3f;
                    _peachJam -= 1 / 3f;
                    _cantaloupeJam -= 1 / 3f;
                }
                break;

            case "sp":
                if (spoonScript.TrySetFillingVisible(true, Fruit.None))
                {
                    _strawberryJam -= 0.5f;
                    _peachJam -= 0.5f;
                }
                break;

            case "sc":
                if (spoonScript.TrySetFillingVisible(true, Fruit.None))
                {
                    _strawberryJam -= 0.5f;
                    _cantaloupeJam -= 0.5f;
                }
                break;

            case "pc":
                if (spoonScript.TrySetFillingVisible(true, Fruit.None))
                {
                    _peachJam -= 0.5f;
                    _cantaloupeJam -= 0.5f;
                }
                break;

            case "s":
                if (spoonScript.TrySetFillingVisible(true, Fruit.Strawberry))
                {
                    _strawberryJam -= 1f;
                }
                break;

            case "p":
                if (spoonScript.TrySetFillingVisible(true, Fruit.Peach))
                {
                    _peachJam -= 1f;
                }
                break;

            case "c":
                if (spoonScript.TrySetFillingVisible(true, Fruit.Cantaloupe))
                {
                    _cantaloupeJam -= 1f;
                }
                break;

            default: break;
        }
    }

    private void SetBlendShape()
    {
        float percent = _volume / _maxCapacity;
        _potWater.SetBlendShapeWeight(0, percent * 100);
    }

    private void OnDestroy()
    {
        _grabScript.OnGrabbedLeft -= OnPotGrabbedLeft;
        _grabScript.OnGrabbedRight -= OnPotGrabbedRight;
    }
}
