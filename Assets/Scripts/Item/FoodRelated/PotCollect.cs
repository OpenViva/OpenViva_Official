using UnityEngine;

public class PotCollect : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _potWater;
    private float _volume = 0f;
    [SerializeField] private float _maxCapacity = 5000f;

    private float _strawberryPieces = 0;
    private float _peachPieces = 0;
    private float _cantaloupePieces = 0;
    private float _blueberryPieces = 0;

    private bool _isInOven = false;
    private float _temperature = 25;

    private float _strawberryJam = 0;
    private float _peachJam = 0;
    private float _cantaloupeJam = 0;
    private float _blueberryJam = 0;

    private void Update()
    {
        if (_isInOven && _temperature < 85) { _temperature += 0.5f * Time.deltaTime; }
        else if (!_isInOven && _temperature > 25) { _temperature -= 0.5f * Time.deltaTime; }
    }

    private void OnParticleCollision(GameObject collider)
    {
        if (collider.gameObject.name.Equals("TapFX"))
        {
            if (_volume >= _maxCapacity) { return; }
            _volume += 50f;
            if (_temperature > 27.5f) { _temperature -= 2.5f; }
            SetBlendShape();
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

    public void MixIngredients()
    {
        if (_volume <= 0 && (_strawberryPieces <= 0.5f || _peachPieces <= 0.5f || _cantaloupePieces <= 0.5f || _blueberryPieces <= 0.5f) || _temperature < 65) { return; }

        if (_strawberryPieces > 0) 
        { 
            _strawberryJam += 0.01f;
            _strawberryPieces -= 0.01f;
        }
        if (_peachPieces > 0) 
        {
            _peachJam += 0.01f;
            _peachPieces -= 0.01f;
        }
        if (_cantaloupePieces > 0) 
        { 
            _cantaloupeJam += 0.01f;
            _cantaloupePieces -= 0.01f;
        }
        if (_blueberryPieces > 0) 
        { 
            _blueberryJam += 0.01f;
            _blueberryPieces -= 0.01f;
        }

        _volume -= 0.5f;
    }

    private void SetBlendShape()
    {
        float percent = _volume / _maxCapacity;
        _potWater.SetBlendShapeWeight(0, percent * 100);
    }
}
