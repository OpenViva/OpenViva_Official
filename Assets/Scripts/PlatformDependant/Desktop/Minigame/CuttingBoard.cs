using nTools.PrefabPainter;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using static FruitCutMinigame;

public class CuttingBoard : MonoBehaviour
{
    public static CuttingBoard Instance { get; private set; }

    private List<GameObject> _strawberryObjects = new();
    private List<GameObject> _peachObjects = new();
    private List<GameObject> _cantaloupeObjects = new();

    [SerializeField] private Prefab _strawberryCrop;
    [SerializeField] private Prefab _peachCrop;
    [SerializeField] private Prefab _cantaloupeCrop;
    [SerializeField] private Prefab _finalStrawberryPrefab;
    [SerializeField] private Prefab _finalPeachPrefab;
    [SerializeField] private Prefab _finalCantaloupePrefab;

    private GameObject _currentlyShowing;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(this); }

        transform.GetChild(0).gameObject.GetChildGameObjects(_strawberryObjects);
        transform.GetChild(1).gameObject.GetChildGameObjects(_peachObjects);
        transform.GetChild(2).gameObject.GetChildGameObjects(_cantaloupeObjects);
    }

    public void EnableFirstObject(Fruit selectedFruit)
    {
        _currentlyShowing?.SetActive(false);
        switch (selectedFruit)
        {
            case Fruit.Strawberry:  _currentlyShowing = _strawberryObjects[0]; break;
            case Fruit.Peach: _currentlyShowing = _peachObjects[0]; break;
            case Fruit.Cantaloupe: _currentlyShowing = _cantaloupeObjects[0]; break;
            default: return;
        }
        _currentlyShowing.SetActive(true);
    }
}
