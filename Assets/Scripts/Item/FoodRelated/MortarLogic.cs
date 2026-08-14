using UnityEngine;

public class MortarLogic : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _wheatCrushedMeshRenderer;
    public float WheatQuantity { private set; get; } = 0;
    public float FlourQuantity { private set; get; } = 0;
    private float _toSpill = 0;

    [SerializeField] private RectTransform _displayBackground;
    [SerializeField] private RectTransform _displayForeground;

    public float GetToSpill()
    {
        float temp = _toSpill;
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

    private void UpdateDisplay()
    {
        float posX = -0.375f + ((WheatQuantity + FlourQuantity) / 300 * 0.375f);
        _displayBackground.localPosition = new(posX, 0, 0);

        posX = -0.375f + (FlourQuantity / 300 * 0.375f);
        _displayForeground.localPosition = new(posX, 0, 0);
    }

    public void AddWheat()
    {
        WheatQuantity += 100;
        WheatQuantity = Mathf.Clamp(WheatQuantity, 0, 300);
        UpdateBlendShapes();
        UpdateDisplay();
    }

    public void GrindWheat()
    {
        if (WheatQuantity <= 0 || FlourQuantity >= 300) { return; }
        WheatQuantity -= 0.25f;
        FlourQuantity += 0.25f;
        UpdateBlendShapes();
        UpdateDisplay();
    }

    public void SpillWheat()
    {
        _toSpill = FlourQuantity;
        FlourQuantity = 0;
        UpdateBlendShapes();
        UpdateDisplay();
    }
}
