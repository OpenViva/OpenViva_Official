using UnityEngine;

public class BowlCollectFlour : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _flourBlendShape;
    private int _wheatVolume = 0;
    private int _maxWheatVolume = 16; // Note: Only 12 is needed for recipe.

    private void OnParticleCollision(GameObject other)
    {
        if (other.gameObject.name.Equals("FlourParticle"))
        {
            if (_wheatVolume >=  _maxWheatVolume) { return; }
            _wheatVolume += 1;
            SetBlendShapes();   
        }
    }

    private void SetBlendShapes()
    {
        if (_wheatVolume <= 0)
        {
            _flourBlendShape.SetBlendShapeWeight(0, 0);
            _flourBlendShape.enabled = false;
            return;
        }
        else { _flourBlendShape.enabled = true; }

        float multiplied = (_wheatVolume * 12.5f)/2;
        multiplied = Mathf.Clamp(multiplied, 0, 100);
        _flourBlendShape.SetBlendShapeWeight(0, multiplied);
    }
}
