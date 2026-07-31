using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static FruitCutMinigame;

public class PotLogic : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private HintManager _HUD;

    [SerializeField] private SkinnedMeshRenderer _potWater;
    private float _volume = 0f;
    [SerializeField] private const float MAX_CAPACITY = 5000f;
    private float _multiplier = 1;

    private float _strawberryPieces = 0;
    private float _peachPieces = 0;
    private float _cantaloupePieces = 0;
    [SerializeField] private const int PIECE_CAPACITY = 40;

    private bool _isInOven = false;
    private float _temperature = 25;

    private PlayerKB_GrabObject _grabScript;

    private float _strawberryJam = 0;
    private float _peachJam = 0;
    private float _cantaloupeJam = 0;
    [SerializeField] private const float JAM_CAPACITY = 20;

    [SerializeField] private GameObject _cookingPotUI;
    [SerializeField] private TextMeshProUGUI _waterQuantity;
    [SerializeField] private TextMeshProUGUI _strawberryText;
    [SerializeField] private TextMeshProUGUI _peachText;
    [SerializeField] private TextMeshProUGUI _cantaloupeText;
    [SerializeField] private RectTransform _temperatureBackground;
    [SerializeField] private GameObject _jamBackground;
    [SerializeField] private TextMeshProUGUI _jamAmount;

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
        if (_isInOven && _temperature < 85)  
        { 
            _temperature += ((_multiplier * 1.2f) + 0.3f) * Time.deltaTime;
            UpdateDisplay();
        }
        else if (!_isInOven && _temperature > 25)  
        { 
            _temperature -= ((_multiplier * 0.8f) + 0.2f) * Time.deltaTime;
            UpdateDisplay();
        }
    }

    private void OnParticleCollision(GameObject collider)
    {
        if (collider.gameObject.name.Equals("TapFX"))
        {
            if (_volume >= MAX_CAPACITY) { return; }
            _volume += 20f;
            if (_temperature > 25.5f) { _temperature -= 2f; }
            SetBlendShape();
            _multiplier = 1f - (_volume  / MAX_CAPACITY);
            UpdateDisplay();
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

        _cookingPotUI.SetActive(held);
    }

    private void OnPotGrabbedRight(bool held) 
    { 
        CuttingBoard.Instance.LastTouchedPot = this;

        if (held) { _HUD.CreateHint(HintConstants.LeftScoopJamHint); }
        else { _HUD.ClearHint(HintConstants.LeftScoopJamHint); }

        _cookingPotUI.SetActive(held);
    }

    private void UpdateDisplay()
    {
        _waterQuantity.SetText($"{_volume:0}");
        _strawberryText.SetText($"{_strawberryPieces:0.0}");
        _peachText.SetText($"{_peachPieces:0.0}");
        _cantaloupeText.SetText($"{_cantaloupePieces:0.0}");

        float width = _temperature / 85 * 950;
        _temperatureBackground.sizeDelta = new(width, _temperatureBackground.sizeDelta.y);
        float posX = -248.5f + (208.5f * (_temperature / 85));
        _temperatureBackground.anchoredPosition = new(posX, _temperatureBackground.anchoredPosition.y);

        width = GetSumOfJam() / JAM_CAPACITY * 900;
        RectTransform rectTransform = _jamBackground.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new(width, rectTransform.sizeDelta.y);
        posX = -232.7f + (164.2f * (GetSumOfJam() / JAM_CAPACITY));
        rectTransform.anchoredPosition = new(posX, rectTransform.anchoredPosition.y);

        _jamAmount.SetText(((int)GetSumOfJam()).ToString());

        Image image = _jamBackground.GetComponent<Image>();
        Color color = Color.white;
        int count = 0;
        if (_strawberryJam >= 1f) 
        {
            color = Color.firebrick;
            count++; 
        }
        if (_peachJam >= 1f) 
        {
            color = Color.sandyBrown;
            count++;
        }
        if (_cantaloupeJam >= 1f) 
        {
            color = Color.orangeRed;
            count++; 
        }

        if (count > 1) { image.color = Color.tomato; }
        else { image.color = color; }
    }

    public void MixIngredients()
    {
        if (_volume <= 0) { return; }
        if (_strawberryPieces <= 0.5f && _peachPieces <= 0.5f && _cantaloupePieces <= 0.5f) { return; }
        if (_temperature < 65) { return; }
        if (GetSumOfJam() >= JAM_CAPACITY) { return; }

        if (_strawberryPieces > 0) 
        { 
            _strawberryJam += 0.0025f;
            _strawberryPieces -= 0.01f;
        }
        if (_peachPieces > 0) 
        {
            _peachJam += 0.005f;
            _peachPieces -= 0.01f;
        }
        if (_cantaloupePieces > 0) 
        { 
            _cantaloupeJam += 0.01f;
            _cantaloupePieces -= 0.01f;
        }

        _volume -= 0.5f;
        UpdateDisplay();
    }

    public bool CollectFruitPieces(Fruit fruit)
    {
        switch (fruit)
        {
            case Fruit.Strawberry: 
                if (_strawberryPieces + _peachPieces + _cantaloupePieces > PIECE_CAPACITY - 2) { return false; }
                _strawberryPieces += 2; 
                break;
            case Fruit.Peach:
                if (_strawberryPieces + _peachPieces + _cantaloupePieces > PIECE_CAPACITY - 8) { return false; }
                _peachPieces += 8; 
                break;
            case Fruit.Cantaloupe:
                if (_strawberryPieces + _peachPieces + _cantaloupePieces > PIECE_CAPACITY - 20) { return false; }
                _cantaloupePieces += 20; 
                break;
        }
        UpdateDisplay();
        return true;
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

        UpdateDisplay();
    }

    private void SetBlendShape()
    {
        float percent = _volume / MAX_CAPACITY;
        _potWater.SetBlendShapeWeight(0, percent * 100);
    }

    private float GetSumOfJam() { return _strawberryJam + _peachJam + _cantaloupeJam; }

    private void OnDestroy()
    {
        _grabScript.OnGrabbedLeft -= OnPotGrabbedLeft;
        _grabScript.OnGrabbedRight -= OnPotGrabbedRight;
    }
}
