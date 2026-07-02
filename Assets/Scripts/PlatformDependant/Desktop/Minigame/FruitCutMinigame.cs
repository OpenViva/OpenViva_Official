using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FruitCutMinigame : MonoBehaviour
{
    [SerializeField] private Camera _minigameCamera;
    [SerializeField] private SkinnedMeshRenderer _minigameHands;
    private Animator _minigameAnimator;
    [SerializeField] private Animator _handAnimator;

    [SerializeField] private GameObject _strawberryPrefabs;
    [SerializeField] private GameObject _peachPrefabs;
    [SerializeField] private GameObject _cantaloupePrefabs;
    private List<GameObject> _strawberries = new();
    private List<GameObject> _peaches = new();
    private List<GameObject> _cantaloupes = new();
    private const int STRAWBERRY_MAX = 2;
    private const int PEACH_MAX = 7;
    private const int CANTALOUPE_MAX = 19;
    private GameObject _currentPrefabShowing;

    [SerializeField]private GameObject _knife;
    private Transform _knifeOrginalTransform;
    private Transform _knifeHoldTransform;

    [SerializeField] private Player _player;
    [SerializeField] private HintManager _HUD;
    private Camera _mainCamera;

    // Minigame fields
    [SerializeField] private float _range = 2f;
    private float _randomPoint;
    private float _movingPoint;
    private bool _goingUp = true;
    private float _totalAccuracy = 0;
    private int _cutCount = 0;
    private bool _isCutting = false;
    private bool _isPlaying = false;
    private int _cutsLeft = 0;

    private enum Fruit
    {
        None = 0,
        Peach = 2,
        Strawberry = 3,
        Cantaloupe = 4
    }
    private Fruit _selectedFruit = 0;

    private void Awake()
    {
        _mainCamera = Camera.main;

        if (_minigameHands == null)
        {
            Debug.LogWarning("No minigame hand model found. Fruit cutting minigame disabled.");
            enabled = false;
        }
        _minigameHands.enabled = false;

        _minigameAnimator = GetComponent<Animator>();

        _knifeOrginalTransform = transform.GetChild(1);
        _knifeHoldTransform = transform.GetChild(2);

        AssignInputs();
        InitAllLists();
    }

    private void Update()
    {
        if (_isPlaying)
        {
            MovePoint();
        }
    }

    private void AssignInputs()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _HUD.CreateHint(HintConstants.CuttingMinigameHint);
            _player.Controls.Viva.UniversalInteract.performed += EnterMinigame;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _HUD.ClearHint(HintConstants.CuttingMinigameHint);
            _player.Controls.Viva.UniversalInteract.performed -= EnterMinigame;
        }
    }

    private void Play()
    {
        _randomPoint = UnityEngine.Random.Range(0, _range);
    }

    private void MovePoint()
    {
        if (_goingUp) { _movingPoint += Time.deltaTime; }
        else { _movingPoint -= Time.deltaTime; }

        if (_movingPoint >= _range) { _goingUp = false; }
        if (_movingPoint <= 0) { _goingUp = true; }

        _movingPoint = Mathf.Clamp(_movingPoint, 0, _range);
    }

    private void EnterMinigame(InputAction.CallbackContext context)
    {
        if (_isPlaying || !Globals.isDesktopMode || _minigameCamera == null) { return; }

        if (_cutsLeft == 0)
        {
            int itemInHand = PlayerManager.Instance.GetItemLeft(true);
            bool isDefined = Enum.IsDefined(typeof(Fruit), itemInHand);
            if (!isDefined)
            {
                itemInHand = PlayerManager.Instance.GetItemRight(true);
                isDefined = Enum.IsDefined(typeof(Fruit), itemInHand);
            }
            if (!isDefined)
            {
                // Display HUD message: 'You need a fruit!'
                Debug.Log("No fruit detected.");
                return;
            }
            _selectedFruit = (Fruit)itemInHand;
            Debug.Log($"Fruit detected: {_selectedFruit}");

            SetFruitState();
        }
        else
        {
            Debug.Log("There is already a fruit on the chopping board."); // Later: Add a way to remove the fruit.
        }

        _isPlaying = true;

        _mainCamera.enabled = false;
        _minigameCamera.enabled = true;
        _minigameHands.enabled = true;

        Globals.handleMovement = false;
        Globals.handleKBLook = false;
        PlayerManager.Instance.HideHands(true);

        _knife.transform.SetPositionAndRotation(_knifeHoldTransform.position, _knifeHoldTransform.rotation);

        _player.Controls.Viva.UniversalInteract.performed -= EnterMinigame;
        _player.Controls.Viva.UniversalInteract.performed += ExitMinigame;
        _player.Controls.Viva.LeftGrab.performed += TryCutFruit;

        Play();
    }

    private void ExitMinigame(InputAction.CallbackContext context)
    {
        if (!_isPlaying) { return; }

        _isPlaying = false;

        _mainCamera.enabled = true;
        _minigameCamera.enabled = false;
        _minigameHands.enabled = false;

        Globals.handleMovement = true;
        Globals.handleKBLook = true;
        PlayerManager.Instance.HideHands(false);

        _knife.transform.SetPositionAndRotation(_knifeOrginalTransform.position, _knifeOrginalTransform.rotation);
        _player.Controls.Viva.UniversalInteract.performed -= ExitMinigame;
        _player.Controls.Viva.UniversalInteract.performed += EnterMinigame;
        _player.Controls.Viva.InteractLeft.performed -= TryCutFruit;
    }

    private void TryCutFruit(InputAction.CallbackContext context)
    {
        if (_isCutting) { return; }

        if (_cutsLeft <= 0)
        {
            // Call end method
            return;
        }

        _handAnimator.Play("Cut");
        StartCoroutine(InitiateCut());
        _cutCount++;
        float diff = Mathf.Abs(_movingPoint - _randomPoint);
        float accuracy = Mathf.Clamp(100 - (diff / 2 * 200), 0, 100);
        _totalAccuracy += accuracy;
        float avgAccuracy = _totalAccuracy / _cutCount;
        Debug.Log($"Fruit cut. Accuracy: {accuracy}%; Average Acc.: {avgAccuracy}%");

        PlayAnimation();
        _randomPoint = UnityEngine.Random.Range(0, _range);
        _cutsLeft--;

        _currentPrefabShowing.SetActive(false);
        switch (_selectedFruit)
        {
            case Fruit.Peach: _currentPrefabShowing = _peaches[PEACH_MAX - _cutsLeft]; break;
            case Fruit.Cantaloupe: _currentPrefabShowing = _cantaloupes[CANTALOUPE_MAX - _cutsLeft]; break;
            case Fruit.Strawberry: _currentPrefabShowing = _strawberries[STRAWBERRY_MAX - _cutsLeft]; break;
            default: Debug.LogWarning($"Fruit not recognized: {_selectedFruit}"); break;
        }
        _currentPrefabShowing.SetActive(true);
    }

    private IEnumerator InitiateCut()
    {
        _isCutting = true;
        yield return new WaitForSeconds(1.5f);
        _isCutting = false;
    }

    private void SetFruitState()
    {
        switch (_selectedFruit)
        {
            case Fruit.Peach:
                _cutsLeft = PEACH_MAX;
                _currentPrefabShowing = _peaches[0].gameObject;
                _currentPrefabShowing.SetActive(true);

                break;
            case Fruit.Cantaloupe: 
                _cutsLeft = CANTALOUPE_MAX; 
                _currentPrefabShowing = _cantaloupes[0].gameObject;
                _currentPrefabShowing.SetActive(true);
                break;
            case Fruit.Strawberry: 
                _cutsLeft = STRAWBERRY_MAX;
                _currentPrefabShowing = _strawberries[0].gameObject;
                _currentPrefabShowing.SetActive(true);
                break;
            default: _cutsLeft = 0; break;
        }
    }

    private void InitAllLists()
    {
        InitList(Fruit.Peach);
        InitList(Fruit.Cantaloupe);
        InitList(Fruit.Strawberry);

        void InitList(Fruit fruit)
        {
            GameObject parent = null;
            switch (fruit)
            {
                case Fruit.Peach: parent = _peachPrefabs; break;
                case Fruit.Cantaloupe: parent = _cantaloupePrefabs; break;
                case Fruit.Strawberry: parent = _strawberryPrefabs; break;
                default: Debug.LogWarning($"Fruit not recognized: {fruit}"); return;
            }

            int count = parent.transform.childCount;

            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                list.Add(parent.transform.GetChild(i).gameObject); 
            }

            switch (fruit)
            {
                case Fruit.Peach: _peaches = list; ; break;
                case Fruit.Cantaloupe: _cantaloupes = list; break;
                case Fruit.Strawberry: _strawberries = list ; break;
            }
        }
    }

    private void PlayAnimation()
    {
        switch (_selectedFruit)
        {
            case (Fruit.Peach): Peach(); break;
            case (Fruit.Cantaloupe): Cantaloupe(); break;
            case (Fruit.Strawberry): Strawberry(); break;
        }

        void Strawberry()
        {
            switch (_cutsLeft)
            {
                case 2: _minigameAnimator.Play("cutStrawberry1"); break;
                case 1: _minigameAnimator.Play("cutStrawberry0"); break;
            }
        }

        void Peach()
        {
            switch (_cutsLeft)
            {
                case 7: _minigameAnimator.Play("cutPeach6"); break;
                case 6: _minigameAnimator.Play("cutPeach5"); break;
                case 5: _minigameAnimator.Play("cutPeach4"); break;
                case 4: _minigameAnimator.Play("cutPeach3"); break;
                case 3: _minigameAnimator.Play("cutPeach2"); break;
                case 2: _minigameAnimator.Play("cutPeach1"); break;
                case 1: _minigameAnimator.Play("cutPeach0"); break;
            }
        }

        void Cantaloupe()
        {
            // to add
        }   
    }
}
