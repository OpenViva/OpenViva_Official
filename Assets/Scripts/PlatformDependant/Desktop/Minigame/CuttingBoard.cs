using nTools.PrefabPainter;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using static FruitCutMinigame;

public class CuttingBoard : MonoBehaviour
{
    public static CuttingBoard Instance { get; private set; }

    // Objects, prefabs and animations
    private Animator _animator;
    private List<GameObject> _strawberryObjects = new();
    private List<GameObject> _peachObjects = new();
    private List<GameObject> _cantaloupeObjects = new();

    [SerializeField] private Prefab _finalStrawberryPrefab;
    [SerializeField] private Prefab _finalPeachPrefab;
    [SerializeField] private Prefab _finalCantaloupePrefab;

    public const int STRAWBERRY_MAX_CUTS = 2;
    public const int PEACH_MAX_CUTS = 7;
    public const int CANTALOUPE_MAX_CUTS = 19;

    private Fruit _selectedFruit = Fruit.None;
    private GameObject _currentlyShowing;

    // Picking up the board
    [SerializeField] private Transform _minigameTransform;
    [SerializeField] private Transform _playerRightHand;
    [SerializeField] private Animator _leftHandAnimator;
    [SerializeField] private Animator _rightHandAnimator;
    private Vector3 _boardHoldPosition;
    private Quaternion _boardHoldRotation;
    private Vector3 _boardOriginalPosition;
    private Quaternion _boardOriginalRotation;
    private bool _boardIsHeld = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(this); }

        transform.GetChild(0).gameObject.GetChildGameObjects(_strawberryObjects);
        transform.GetChild(1).gameObject.GetChildGameObjects(_peachObjects);
        transform.GetChild(2).gameObject.GetChildGameObjects(_cantaloupeObjects);

        _animator = GetComponent<Animator>();

        _boardHoldPosition = ObjectHoldPositions.Instance.GetCuttingBoardPosition();
        _boardHoldRotation = ObjectHoldPositions.Instance.GetCuttingBoardRotation();
        _boardOriginalPosition = transform.position;
        _boardOriginalRotation = transform.rotation;
    }

    private void Start()
    {
        AssignInputs();
    }

    private void AssignInputs()
    {

    }

    public void EnableFirstObject(Fruit selectedFruit)
    {
        _selectedFruit = selectedFruit;

        if (_currentlyShowing != null) { _currentlyShowing.SetActive(false); }
        switch (_selectedFruit)
        {
            case Fruit.Strawberry:  _currentlyShowing = _strawberryObjects[0]; break;
            case Fruit.Peach: _currentlyShowing = _peachObjects[0]; break;
            case Fruit.Cantaloupe: _currentlyShowing = _cantaloupeObjects[0]; break;
            default: return;
        }
        _currentlyShowing.SetActive(true);
    }

    public void EnableNextObject(int cutsMade)
    {
        _currentlyShowing.SetActive(false);
        switch (_selectedFruit)
        {
            case Fruit.Strawberry: _currentlyShowing = _strawberryObjects[cutsMade];  break;
            case Fruit.Peach:  _currentlyShowing = _peachObjects[cutsMade];  break;
            case Fruit.Cantaloupe:  _currentlyShowing = _cantaloupeObjects[cutsMade];  break;
            default: return;
        }
        _currentlyShowing.SetActive(true);

        PlayObjectAnimation(cutsMade);
    }

    public void EnableLastObject()
    {
        _currentlyShowing.SetActive(false);
        switch (_selectedFruit)
        {
            case Fruit.Strawberry:
                _currentlyShowing = Instantiate(_finalStrawberryPrefab.gameObject);
                _currentlyShowing.transform.SetParent(transform.GetChild(0).transform, true);
                _currentlyShowing.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                _currentlyShowing.SetActive(true);
                _currentlyShowing.name = _finalStrawberryPrefab.gameObject.name;
                PlayObjectAnimation(STRAWBERRY_MAX_CUTS);
                break;

            case Fruit.Peach: 
                _currentlyShowing = Instantiate(_finalPeachPrefab.gameObject);
                _currentlyShowing.transform.SetParent(transform.GetChild(1).transform, true);
                _currentlyShowing.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                _currentlyShowing.SetActive(true);
                _currentlyShowing.name = _finalPeachPrefab.gameObject.name;
                PlayObjectAnimation(PEACH_MAX_CUTS);
                break;

            case Fruit.Cantaloupe:
                _currentlyShowing = Instantiate(_finalCantaloupePrefab.gameObject);
                _currentlyShowing.transform.SetParent(transform.GetChild(2).transform, true);
                _currentlyShowing.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                _currentlyShowing.SetActive(true);
                _currentlyShowing.name = _finalStrawberryPrefab.gameObject.name;
                PlayObjectAnimation(CANTALOUPE_MAX_CUTS);
                break;

            default: return;
        }
    }

    private void PlayObjectAnimation(int cutsMade)
    {
        switch (_selectedFruit)
        {
            case Fruit.Strawberry: Strawberry(); break;
            case Fruit.Peach: Peach(); break;
            case Fruit.Cantaloupe: Cantaloupe(); break;
        }

        void Strawberry()
        {
            if (cutsMade == 1) { _animator.Play("cutStrawberry1"); }
        }

        void Peach()
        {
            switch (cutsMade)
            {
                case 1: _animator.Play("cutPeach6"); break;
                case 2: _animator.Play("cutPeach5"); break;
                case 3: _animator.Play("cutPeach4"); break;
                case 4: _animator.Play("cutPeach3"); break;
                case 5: _animator.Play("cutPeach2"); break;
                case 6: _animator.Play("cutPeach1"); break;
            }
        }

        void Cantaloupe()
        {
            switch (cutsMade)
            {
                case 1: _animator.Play("cutCantaloupe18"); break;
                case 2: _animator.Play("cutCantaloupe17"); break;
                case 3: _animator.Play("cutCantaloupe16"); break;
                case 4: _animator.Play("cutCantaloupe15"); break;
                case 5: _animator.Play("cutCantaloupe14"); break;
                case 6: _animator.Play("cutCantaloupe13"); break;
                case 7: _animator.Play("cutCantaloupe12"); break;
                case 8: _animator.Play("cutCantaloupe11"); break;
                case 9: _animator.Play("cutCantaloupe10"); break;
                case 10: _animator.Play("cutCantaloupe9"); break;
                case 11: _animator.Play("cutCantaloupe8"); break;
                case 12: _animator.Play("cutCantaloupe7"); break;
                case 13: _animator.Play("cutCantaloupe6"); break;
                case 14: _animator.Play("cutCantaloupe5"); break;
                case 15: _animator.Play("cutCantaloupe4"); break;
                case 16: _animator.Play("cutCantaloupe3"); break;
                case 17: _animator.Play("cutCantaloupe2"); break;
                case 18: _animator.Play("cutCantaloupe1"); break;
            }
        }
    }

    public void PickBoardUp()
    {
        transform.SetParent(_playerRightHand, true);
        transform.SetLocalPositionAndRotation(_boardHoldPosition, _boardHoldRotation);

        _leftHandAnimator.Play("holdCuttingBoardL");
        _rightHandAnimator.Play("holdCuttingBoardR");

        _boardIsHeld = true;
        PlayerManager.Instance.LeftHandOccupied = true;
        PlayerManager.Instance.RightHandOccupied = true;
    }
}
