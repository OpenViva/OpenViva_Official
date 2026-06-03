using UnityEngine;

public class CupCollectWater : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _cupWater;
    private float _volume = 0f;
    [SerializeField] private float _maxCapacity = 250f;

    private void OnParticleCollision(GameObject other)
    {
        if (other.gameObject.name.Equals("TapFX"))
        {
            if (_volume >= _maxCapacity) { return; }
            _volume += 50f;
            SetBlendShape();
        }
    }

    private void SetBlendShape()
    {
        float percent = _volume / _maxCapacity;
        _cupWater.SetBlendShapeWeight(0, percent * 100);
    }
}
