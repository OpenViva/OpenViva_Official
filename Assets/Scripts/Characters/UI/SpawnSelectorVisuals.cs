using UnityEngine;

public class SpawnSelectorVisuals : MonoBehaviour
{
    public GameObject SpawnPrefab;
    public Vector3 SpawnOffset = new(0, 1.3f, 0);
    public GameObject SpawnedSelector;

    public void SpawnSelectorVisual(Transform parent)
    {
        SpawnedSelector = Instantiate(SpawnPrefab, parent);
        SpawnedSelector.transform.localPosition = Vector3.up + SpawnOffset;
        SpawnedSelector.transform.localRotation = Quaternion.identity;
    }
}
