using System.Collections;
using UnityEngine;

public class SpawnSelectorVisuals : MonoBehaviour
{
    public GameObject SpawnPrefab;
    public Vector3 SpawnOffset = new(0, 0.5f, 0);

    public GameObject SpawnedSelector;

    private WaitForSeconds SpawnTime;

    void Awake()
    {
        SpawnTime = new WaitForSeconds(0.1f);

        StartCoroutine(SpawnSelectorVisual());
    }

    IEnumerator SpawnSelectorVisual()
    {
        yield return SpawnTime;

        SpawnedSelector = Instantiate(SpawnPrefab, SpawnOffset, Quaternion.identity);

        yield return null;
    }
}
