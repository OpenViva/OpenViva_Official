using nTools.PrefabPainter;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
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

    [SerializeField] GameObject _minigameGameObject;
    [SerializeField] GameObject _playerHandR;
    [SerializeField] GameObject _playerHandL;
    private Animator _playerAnimatorL;
    private Animator _playerAnimatorR;
    private Vector3 _initialBoardPosition;
    private Quaternion _initialBoardRotation;

    [SerializeField] private Player _player;
    private bool _isBoardCarried;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _playerAnimatorL = _playerHandL.transform.GetComponentInParent<Animator>();
        _playerAnimatorR = _playerHandR.transform.GetComponentInParent<Animator>();

        _strawberryPrefabs = transform.GetChild(0).gameObject;
        _peachPrefabs = transform.GetChild(1).gameObject;
        _cantaloupePrefabs = transform.GetChild(2).gameObject;

        _peachPrefabs.GetChildGameObjects(_peaches);
        _strawberryPrefabs.GetChildGameObjects(_strawberries);
        _cantaloupePrefabs.GetChildGameObjects(_cantaloupes);

        _initialBoardPosition = transform.localPosition;
        _initialBoardRotation = transform.localRotation;
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

    public void PickBoardUp(Fruit fruit)
    {
        transform.SetParent(_playerHandR.transform);
        transform.SetLocalPositionAndRotation(
            ObjectHoldPositions.Instance.GetCuttingBoardPosition(),
            ObjectHoldPositions.Instance.GetCuttingBoardRotation()
        );

        _playerAnimatorL.Play("holdCuttingBoardL");
        _playerAnimatorR.Play("holdCuttingBoardR");

        _player.Controls.Viva.InteractLeft.performed += context => TiltAnticlockwise(context, fruit);
        _player.Controls.Viva.InteractRight.performed += context => TiltClockwise(context, fruit);
        _player.Controls.Viva.InteractLeft.canceled += context => AnticlockwiseStraigten(context, fruit);
        _player.Controls.Viva.InteractRight.canceled += context => ClockwiseStraighten(context, fruit);

        _isBoardCarried = true;
    }

    public void PutBoardDown()
    {
        transform.SetParent(_minigameGameObject.transform);
        transform.SetLocalPositionAndRotation(_initialBoardPosition, _initialBoardRotation);

        _playerAnimatorL.Play("handIdle");
        _playerAnimatorR.Play("handIdle");
    }

    private void TiltClockwise(InputAction.CallbackContext context, Fruit fruit)
    {
        _playerAnimatorL.Play("clockwiseCuttingBoardL");
        _playerAnimatorR.Play("clockwiseCuttingBoardR");

        EnableFruitPhysics(true, fruit);
    }

    private void TiltAnticlockwise(InputAction.CallbackContext context, Fruit fruit)
    {
        _playerAnimatorL.Play("anticlockwiseCuttingBoardL");
        _playerAnimatorR.Play("anticlockwiseCuttingBoardR");

        EnableFruitPhysics(true, fruit);
    }

    private void ClockwiseStraighten(InputAction.CallbackContext context, Fruit fruit)
    {
        _playerAnimatorL.Play("clockwiseStraightenL");
        _playerAnimatorR.Play("clockwiseStraightenR");

        EnableFruitPhysics(false, fruit);
    }

    private void AnticlockwiseStraigten(InputAction.CallbackContext context, Fruit fruit)
    { 
        _playerAnimatorR.Play("anticlockwiseStraightenR");
        _playerAnimatorL.Play("anticlockwiseStraightenL");

        EnableFruitPhysics(false, fruit);
    }

    private void EnableFruitPhysics(bool enable, Fruit fruit)
    {
        List<Rigidbody> rbList = new();
        switch (fruit)
        {
            case Fruit.Peach:
                rbList = _peaches[_peaches.Count - 1].GetComponentsInChildren<Rigidbody>(true).ToList();
                break;

            case Fruit.Cantaloupe:
                rbList = _cantaloupes[_cantaloupes.Count - 1].GetComponentsInChildren<Rigidbody>(true).ToList();
                break;

            case Fruit.Strawberry:
                rbList = _strawberries[_strawberries.Count - 1].GetComponentsInChildren<Rigidbody>(true).ToList();
                break;

            default: Debug.LogWarning($"Fruit not recognized: {fruit}"); break;
        }

        string s = null;
        for (int i = 0;  i < rbList.Count; i++)
        {
            s += $"\n{rbList[i]}";
        }
        Debug.Log(s);

        foreach (Rigidbody rb in rbList)
        {
            rb.isKinematic = !enable;
            rb.useGravity = enable;
        }

        rbList[0].transform.parent.transform.SetParent(gameObject.transform);
    }
}
