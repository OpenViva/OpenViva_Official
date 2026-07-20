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
            SetBlendShape();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out FruitPiece pieceScript))
        {
            switch (pieceScript.Fruit)
            {
                case FruitCutMinigame.Fruit.Strawberry: _strawberryPieces += 1; break;
                case FruitCutMinigame.Fruit.Peach: _peachPieces += 1; break;
                case FruitCutMinigame.Fruit.Cantaloupe: _cantaloupePieces += 1; break;
                default: Debug.LogWarning($"Fruit not recognized: {pieceScript.Fruit}"); return;
            }

            Destroy(pieceScript.gameObject);
        }

        if (other.gameObject.name.Contains("Oven")) { _isInOven = true; }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Contains("Oven")) { _isInOven = false; ; }
    }

    public void MixIngredients()
    {
        if (_volume <= 0 && (_strawberryPieces <= 0 || _peachPieces <= 0 || _cantaloupePieces <= 0 || _blueberryPieces <= 0)) { return; }

        if (_strawberryPieces > 0) { _strawberryJam += 0.1f; }
        if (_peachPieces > 0) { _peachJam += 0.1f; }
        if (_cantaloupePieces > 0) { _cantaloupeJam += 0.1f; }
        if (_blueberryPieces > 0) { _blueberryJam += 0.1f; }

        _volume -= 0.2f;
    }

    private void SetBlendShape()
    {
        float percent = _volume / _maxCapacity;
        _potWater.SetBlendShapeWeight(0, percent * 100);
    }
}
