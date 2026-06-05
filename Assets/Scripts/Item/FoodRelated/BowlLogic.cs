using UnityEngine;

public class BowlLogic : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _flourBlendShape;
    private float _flourVolume = 0;
    private float _maxFlourVolume = 2100; // recipe needs 300

    [SerializeField] private SkinnedMeshRenderer _waterBlendShape;
    private float _waterVolume = 0;
    private float _maxWaterVolume = 1400; // recipe needs 200

    [SerializeField] private SkinnedMeshRenderer _batterBlendShape;
    private float _batterVolume = 0;
    private float _maxBatterVolume = 3500;

    private void OnParticleCollision(GameObject other)
    {
        if (other.gameObject.name.Equals("FlourParticle"))
        {
            if (_flourVolume >= _maxFlourVolume) { return; }
            _flourVolume += other.GetComponentInParent<MortarLogic>().GetToSpill() * 100;
            SetFlourBlend();
        }
        else if (other.gameObject.name.Equals("TapFX"))
        {
            if (_waterVolume >= _maxWaterVolume) { return; }
            _waterVolume += 50f;
            SetWaterBlend();
        }
    }

    private void SetFlourBlend()
    {
        if (_flourVolume <= 0)
        {
            _flourBlendShape.SetBlendShapeWeight(0, 0);
            _flourBlendShape.enabled = false;
            return;
        }
        else { _flourBlendShape.enabled = true; }
        
        float percent = _flourVolume / 3500;
        _flourBlendShape.SetBlendShapeWeight(0, percent * 100);
    }

    private void SetWaterBlend()
    {
        if (_waterVolume <= 0)
        {
            _waterBlendShape.SetBlendShapeWeight(0, 0);
            _waterBlendShape.enabled = false;
            return;
        }
        else { _waterBlendShape.enabled = true; }

        float percent = _waterVolume / 3500;
        _waterBlendShape.SetBlendShapeWeight(0, percent * 100);
    }

    private void SetBatterBlend()
    {
        if (_batterVolume <= 0)
        {
            _batterBlendShape.SetBlendShapeWeight(0, 0);
            _batterBlendShape.enabled = false;
            return;
        }
        else { _batterBlendShape.enabled = true; }

        float percent = _batterVolume / 3500;
        _batterBlendShape.SetBlendShapeWeight(0, percent * 100);
    }

    private void SetAllBlends()
    {
        SetFlourBlend();
        SetWaterBlend();
        SetBatterBlend();
    }

    public void MixBatter()
    {
        if (_flourVolume <= 0 || _waterVolume <= 0 || _batterVolume >= _maxBatterVolume) { return; }

        _flourVolume -= 0.6f * 0.5f;
        _waterVolume -= 0.4f * 0.5f;
        _batterVolume += 0.5f;

        SetAllBlends();
    }
}
