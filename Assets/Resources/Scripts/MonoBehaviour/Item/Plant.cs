using UnityEngine;

public class Plant : MonoBehaviour
{

    [SerializeField]
    private Transform[] fruitSpawnTransforms;

    [SerializeField]
    private GameObject fruitPrefab;

    private int fruitsActive = 0;
    private float growthPercent = 0.0f;

    public void Grow(float percent)
    {

        growthPercent = Mathf.Min(1.0f, growthPercent + percent);
        if (growthPercent < 1.0f)
        {
            return;
        }

        if (fruitsActive == fruitSpawnTransforms.Length)
        {
            return;
        }
        int randomGrowths = Random.Range(0, fruitSpawnTransforms.Length - 1 - fruitsActive);

        for (int i = 0; i < randomGrowths; i++)
        {
            Transform targetFruitTransform = fruitSpawnTransforms[fruitsActive + i];

            GameObject plant = GameObject.Instantiate(fruitPrefab, targetFruitTransform.position, targetFruitTransform.rotation);
        }

        fruitsActive += randomGrowths;
    }
}