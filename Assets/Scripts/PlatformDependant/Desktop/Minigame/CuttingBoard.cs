using nTools.PrefabPainter;
using System.Collections.Generic;
using UnityEngine;
using static FruitCutMinigame;

public class CuttingBoard : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private Prefab _strawberryPrefab;
    [SerializeField] private Prefab _peachPrefab;
    [SerializeField] private Prefab _cantaloupePrefab;
    private GameObject _strawberryPrefabs;
    private GameObject _peachPrefabs;
    private GameObject _cantaloupePrefabs;
    private List<GameObject> _strawberries = new();
    private List<GameObject> _peaches = new();
    private List<GameObject> _cantaloupes = new();

    private GameObject _currentActivePrefab;

    public const int STRAWBERRY_MAX = 2;
    public const int PEACH_MAX = 7;
    public const int CANTALOUPE_MAX = 19;

    private void Start()
    {
        _animator = GetComponent<Animator>();

        _strawberryPrefabs = transform.GetChild(0).gameObject;
        _peachPrefabs = transform.GetChild(1).gameObject;
        _cantaloupePrefabs = transform.GetChild(2).gameObject;

        InitAllLists();
    }

    private void InitAllLists()
    {
        InitList(Fruit.Peach);
        InitList(Fruit.Cantaloupe);
        InitList(Fruit.Strawberry);

        void InitList(Fruit fruit)
        {
            GameObject parent;
            switch (fruit)
            {
                case Fruit.Peach: parent = _peachPrefabs; break;
                case Fruit.Cantaloupe: parent = _cantaloupePrefabs; break;
                case Fruit.Strawberry: parent = _strawberryPrefabs; break;
                default: Debug.LogWarning($"Fruit not recognized: {fruit}"); return;
            }

            int count = parent.transform.childCount;

            List<GameObject> list = new();
            for (int i = 0; i < count; i++)
            {
                list.Add(parent.transform.GetChild(i).gameObject);
            }

            switch (fruit)
            {
                case Fruit.Peach: _peaches = list; ; break;
                case Fruit.Cantaloupe: _cantaloupes = list; break;
                case Fruit.Strawberry: _strawberries = list; break;
            }
        }
    }

    public int ActivateNewFruit(Fruit fruit)
    {
        switch (fruit)
        {
            case Fruit.Peach:
                _currentActivePrefab = _peaches[0];
                _currentActivePrefab.SetActive(true);
                return PEACH_MAX;

            case Fruit.Cantaloupe:
                _currentActivePrefab = _cantaloupes[0];
                _currentActivePrefab.SetActive(true);
                return CANTALOUPE_MAX;

            case Fruit.Strawberry:
                _currentActivePrefab = _strawberries[0];
                _currentActivePrefab.SetActive(true);
                return STRAWBERRY_MAX;

            default: return 0;
        }
    }

    public void SetActivePrefab(Fruit fruit, int cutsLeft)
    {
        _currentActivePrefab.SetActive(false);
        switch (fruit)
        {
            case Fruit.Peach: 
                _currentActivePrefab = _peaches[PEACH_MAX - cutsLeft];
                Peach();
                break;

            case Fruit.Cantaloupe: 
                _currentActivePrefab = _cantaloupes[CANTALOUPE_MAX - cutsLeft];
                Cantaloupe();
                break;

            case Fruit.Strawberry: 
                _currentActivePrefab = _strawberries[STRAWBERRY_MAX - cutsLeft]; 
                Strawberry();
                break;

            default: Debug.LogWarning($"Fruit not recognized: {fruit}"); break;
        }

        void Peach()
        {
            _currentActivePrefab.SetActive(true);
            switch (cutsLeft)
            {
                case 6: _animator.Play("cutPeach6"); break;
                case 5: _animator.Play("cutPeach5"); break;
                case 4: _animator.Play("cutPeach4"); break;
                case 3: _animator.Play("cutPeach3"); break;
                case 2: _animator.Play("cutPeach2"); break;
                case 1: _animator.Play("cutPeach1"); break;
                case 0: _animator.Play("cutPeach0"); break;
            }
        }

        void Cantaloupe()
        {
            _currentActivePrefab.SetActive(true);
            switch (cutsLeft)
            {
                case 18: _animator.Play("cutCantaloupe18"); break;
                case 17: _animator.Play("cutCantaloupe17"); break;
                case 16: _animator.Play("cutCantaloupe16"); break;
                case 15: _animator.Play("cutCantaloupe15"); break;
                case 14: _animator.Play("cutCantaloupe14"); break;
                case 13: _animator.Play("cutCantaloupe13"); break;
                case 12: _animator.Play("cutCantaloupe12"); break;
                case 11: _animator.Play("cutCantaloupe11"); break;
                case 10: _animator.Play("cutCantaloupe10"); break;
                case 9: _animator.Play("cutCantaloupe9"); break;
                case 8: _animator.Play("cutCantaloupe8"); break;
                case 7: _animator.Play("cutCantaloupe7"); break;
                case 6: _animator.Play("cutCantaloupe6"); break;
                case 5: _animator.Play("cutCantaloupe5"); break;
                case 4: _animator.Play("cutCantaloupe4"); break;
                case 3: _animator.Play("cutCantaloupe3"); break;
                case 2: _animator.Play("cutCantaloupe2"); break;
                case 1: _animator.Play("cutCantaloupe1"); break;
                case 0: _animator.Play("cutCantaloupe0"); break;
            }
        }

        void Strawberry()
        {
            _currentActivePrefab.SetActive(true);
            switch (cutsLeft)
            {
                case 1: _animator.Play("cutStrawberry1"); break;
                case 0: _animator.Play("cutStrawberry0"); break;
            }
        }
    }

    public void OnMinigameCancelled(Fruit fruit)
    {
        _currentActivePrefab.SetActive(false);

        GameObject crop = null;
        switch (fruit)
        {
            case Fruit.Peach:
                crop = Instantiate(_peachPrefab.gameObject);
                crop.name = _peachPrefab.gameObject.name;
                break;

            case Fruit.Cantaloupe:
                crop = Instantiate(_cantaloupePrefab.gameObject);
                crop.name = _cantaloupePrefab.gameObject.name;
                break;

            case Fruit.Strawberry:
                crop = Instantiate(_strawberryPrefab.gameObject);
                crop.name = _strawberryPrefab.gameObject.name;
                break;
        }

        Crop cropScript = crop.GetComponent<Crop>();
        cropScript.ShouldGrow = false;
        crop.transform.localScale = Vector3.one;
        cropScript.SetIsActive(true, 1);
    }

    public void PickBoardUp()
    {

    }
}
