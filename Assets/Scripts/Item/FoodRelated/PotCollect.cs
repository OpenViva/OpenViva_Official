using UnityEngine;

public class PotCollect : MonoBehaviour
{
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

        _grabScript.OnGrabbedLeft += OnPotGrabbedLeft;
        _grabScript.OnGrabbedRight += OnPotGrabbedRight;
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

    private void OnPotGrabbedLeft(bool held) { CuttingBoard.Instance.LastTouchedPot = this; }

    private void OnPotGrabbedRight(bool held) { CuttingBoard.Instance.LastTouchedPot = this; }

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

    public void CollectFruitPieces(FruitCutMinigame.Fruit fruit)
    {
        switch (fruit)
        {
            case FruitCutMinigame.Fruit.Strawberry: _strawberryPieces += 2; break;
            case FruitCutMinigame.Fruit.Peach: _peachPieces += 8; break;
            case FruitCutMinigame.Fruit.Cantaloupe: _cantaloupePieces += 20; break;
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
