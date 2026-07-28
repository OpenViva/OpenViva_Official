using UnityEngine;

public class MortarLogic : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _wheatCrushedMeshRenderer;
    public float WheatQuantity { private set; get; } = 0;
    public float FlourQuantity { private set; get; } = 0;

    private int _toSpill = 0;
    public int GetToSpill()
    {
        int temp = _toSpill;
        _toSpill = 0;
        return temp;
    }

    private void UpdateBlendShapes()
    {
        WheatQuantity = Mathf.Clamp(WheatQuantity, 0, 300 - FlourQuantity);
        if (_wheatCrushedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer component not found on the MortarLogic object.");
            return;
        }

        int devided = (int)(WheatQuantity / 100);
        for (int i = 0; i <= devided; i++)
        {
            _wheatCrushedMeshRenderer.SetBlendShapeWeight(i, Mathf.Clamp(WheatQuantity - (i * 100), 0, 100));
        }

        _wheatCrushedMeshRenderer.SetBlendShapeWeight(3, FlourQuantity / 3);

        // Debug.Log($"Wheat Quantity: {WheatQuantity}, Flour Quantity: {FlourQuantity}");
    }

    public void AddWheat()
    {
        WheatQuantity += 100;
        WheatQuantity = Mathf.Clamp(WheatQuantity, 0, 300);
        UpdateBlendShapes();
    }

    public void GrindWheat()
    {
        if (WheatQuantity <= 0 || FlourQuantity >= 300) { return; }
        WheatQuantity -= 0.25f;
        FlourQuantity += 0.25f;
        UpdateBlendShapes();
    }

    public void SpillWheat()
    {
        _toSpill = (int)FlourQuantity / 100;
        FlourQuantity = 0;
        UpdateBlendShapes();
    }
}
