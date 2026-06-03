using UnityEngine;

public class PotCollectWater : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _potWater;
    private float _volume = 0f;
    [SerializeField] private float _maxCapacity = 5000f;

    private void OnParticleCollision(GameObject collider)
    {
        if (collider.gameObject.name.Equals("TapFX"))
        {
            if (_volume >= _maxCapacity) { return; }
            _volume += 50f;
            SetBlendShape();
        }
    }

    private void SetBlendShape()
    {
        float percent = _volume / _maxCapacity;
        _potWater.SetBlendShapeWeight(0, percent * 100);
    }
}
