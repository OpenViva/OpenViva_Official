using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    [Header("Pool Settings")]
    [Tooltip("Size of a pool for each initizalied pool")]
    [SerializeField] private int poolSize = 20;

    [Header("Pools Prefabs")]
    [SerializeField] private GameObject examplePool;
    // Add more pool types here (and initialize on Start below)

    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new();

    private void Start()
    {
        InitializeNewPool(examplePool);

        // Add more pools to initialize here when needed...
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Get the given type of GameObject by its prefab.
    /// </summary>
    /// <param name="prefab">Prefab of the required GameObject (must have PooledObject script)</param>
    /// <returns></returns>
    public GameObject GetObject(GameObject prefab)
    {
        if (poolDictionary.ContainsKey(prefab) == false)
        {
            InitializeNewPool(prefab);
        }

        if (poolDictionary[prefab].Count == 0)
        {
            CreateNewObject(prefab); // If all objects of this type are in use, create a new one
        }

        GameObject objectToGet = poolDictionary[prefab].Dequeue();
        objectToGet.SetActive(true);
        objectToGet.transform.parent = null;

        return objectToGet;
    }

    /// <summary>
    /// Return the given GameObject to its pool and SetActive = false after a set delay.
    /// </summary>
    /// <param name="objectToReturn">GameObject to return (must have PooledObject script)</param>
    /// <param name="delay">Wait for [seconds] before returning (optional)</param>
    public void ReturnObject(GameObject objectToReturn, float delay = 0.001f)
    {
        StartCoroutine(DelayReturn(delay, objectToReturn));
    }

    private IEnumerator DelayReturn(float delay, GameObject objectToReturn)
    {
        yield return new WaitForSeconds(delay);

        ReturnToPool(objectToReturn);
    }

    private void ReturnToPool(GameObject objectToReturn)
    {
        GameObject originalPrefab = objectToReturn.GetComponent<PooledObject>().originalPrefab;

        objectToReturn.SetActive(false);
        objectToReturn.transform.parent = transform;

        poolDictionary[originalPrefab].Enqueue(objectToReturn);
    }

    private void InitializeNewPool(GameObject prefab)
    {
        poolDictionary[prefab] = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObject(prefab);
        }
    }

    private void CreateNewObject(GameObject prefab)
    {
        GameObject newObject = Instantiate(prefab, transform);
        newObject.AddComponent<PooledObject>().originalPrefab = prefab;
        newObject.SetActive(false);

        poolDictionary[prefab].Enqueue(newObject);
    }
}
