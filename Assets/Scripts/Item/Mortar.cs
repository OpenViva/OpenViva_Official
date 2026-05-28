using UnityEngine;

public class Mortar : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _wheatCrushedMeshRenderer;
    public float WheatQuantity = 0;
    private float _flourQuantity = 0;

    private void UpdateBlendShapes()
    {
        if (_wheatCrushedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer component not found on the Mortar object.");
            return;
        }

        int devided = (int)(WheatQuantity / 100);
        for (int i = 0; i <= devided; i++)
        {
            _wheatCrushedMeshRenderer.SetBlendShapeWeight(i, Mathf.Clamp(WheatQuantity - (i * 100), 0, 100));
        }

        _wheatCrushedMeshRenderer.SetBlendShapeWeight(3, _flourQuantity / 3);
    }

    public void AddWheat()
    {
        WheatQuantity += 100;
        WheatQuantity = Mathf.Clamp(WheatQuantity, 0, 300);
        UpdateBlendShapes();
    }

    public void GrindWheat()
    {
        if (_flourQuantity >= 300) { return; }
        WheatQuantity -= 1;
        _flourQuantity += 1;
        UpdateBlendShapes();
    }
}
