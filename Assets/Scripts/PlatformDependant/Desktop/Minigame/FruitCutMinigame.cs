using nTools.PrefabPainter;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FruitCutMinigame : MonoBehaviour
{
    private Camera _minigameCamera;
    private SkinnedMeshRenderer _minigameHands;
    private Animator _minigameAnimator;
    [SerializeField] private Animator _handAnimator;

    [SerializeField] private Prefab _strawberryPrefab;
    [SerializeField] private Prefab _peachPrefab;
    [SerializeField] private Prefab _cantaloupePrefab;
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
    private GameObject _objectInOtherHand;

    [SerializeField] private GameObject _knife;
    private Transform _knifeOrginalTransform;
    private Transform _knifeHoldTransform;

    [SerializeField] private Player _player;
    [SerializeField] private HintManager _HUD;
    private Camera _mainCamera;

    private GameObject _minigameHUD;
    private GameObject _movingPointImage;
    private GameObject _target;
    private TextMeshProUGUI _accuracyText;
    private TextMeshProUGUI _averageText;
    private TextMeshProUGUI _doneText;
    private Scrollbar _progressBar;
    private GameObject _progressBarHandle;

    [SerializeField] private Transform _boardAttachPoint;
    private GameObject _cuttingBoard;
    private Transform _boardOriginalTransform;

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
    private bool _noCutsLeft = true;

    public enum Fruit
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

        GameObject parent = transform.GetChild(3).gameObject;
        _minigameHands = parent.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();

        if (_minigameHands == null)
        {
            Debug.LogWarning("No minigame hand model found. Fruit cutting minigame disabled.");
            enabled = false;
        }
        _minigameHands.enabled = false;

        _minigameCamera = transform.GetChild(0).GetComponent<Camera>();
        _minigameAnimator = transform.GetChild(4).GetComponent<Animator>();

        _knifeOrginalTransform = transform.GetChild(1);
        _knifeHoldTransform = transform.GetChild(2);

        _minigameHUD = _HUD.transform.GetChild(2).gameObject;
        _movingPointImage = _minigameHUD.transform.GetChild(2).gameObject;
        parent = _minigameHUD.transform.GetChild(0).gameObject;
        _target = parent.transform.GetChild(0).gameObject;
        parent = _minigameHUD.transform.GetChild(3).gameObject;
        _accuracyText = parent.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _averageText = parent.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        parent = _minigameHUD.transform.GetChild(5).gameObject;
        _progressBar = parent.GetComponent<Scrollbar>();
        parent = parent.transform.GetChild(0).gameObject;
        _progressBarHandle = parent.transform.GetChild(0).gameObject;
        _doneText = _minigameHUD.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>();

        _cuttingBoard = transform.GetChild(4).gameObject;
        _boardOriginalTransform = _cuttingBoard.transform;
    }

    private void Start()
    {
        AssignInputs();
        InitAllLists();
    }

    private void Update()
    {
        if (_isPlaying) { MovePoint(); }
    }

    private void AssignInputs()
    {
        _player.Controls.Viva.Cancel.performed += CancelCutting;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _HUD.CreateHint(HintConstants.EnterCuttingMinigameHint);
            _player.Controls.Viva.UniversalInteract.performed += EnterMinigame;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _HUD.ClearHint(HintConstants.EnterCuttingMinigameHint);
            _player.Controls.Viva.UniversalInteract.performed -= EnterMinigame;
        }
    }

    private void Play()
    {
        SetRandomPoint();
    }

    private void MovePoint()
    {
        if (_goingUp) { _movingPoint += Time.deltaTime; }
        else { _movingPoint -= Time.deltaTime; }

        Vector3 vector = _movingPointImage.transform.localPosition;
        _movingPointImage.transform.localPosition = new Vector3(-150f + (_movingPoint * 300 / _range), vector.y, vector.z);

        if (_movingPoint >= _range) { _goingUp = false; }
        if (_movingPoint <= 0) { _goingUp = true; }

        _movingPoint = Mathf.Clamp(_movingPoint, 0, _range);
    }

    private void EnterMinigame(InputAction.CallbackContext context)
    {
        if (_isPlaying || !Globals.isDesktopMode || _minigameCamera == null) { return; }

        if (_cutsLeft == 0 && _selectedFruit == Fruit.None)
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
                return;
            }

            PlayerKB_GrabObject grabScript = null;
            int handedness = 0;

            _objectInOtherHand = PlayerManager.Instance.GetObjectRight();
            if (_objectInOtherHand != null)
            {
                grabScript = _objectInOtherHand.GetComponent<PlayerKB_GrabObject>();
                handedness = 2;
            }
            else
            {
                _objectInOtherHand = PlayerManager.Instance.GetObjectLeft();
                if (_objectInOtherHand != null)
                {
                    grabScript = _objectInOtherHand.GetComponent<PlayerKB_GrabObject>();
                    handedness = 1;
                }
            }

            _selectedFruit = (Fruit)itemInHand;
            if (grabScript != null)
            {
                grabScript.SetIsActive(false, grabScript.GetIsGrabbed());
            }
            
            SetFruitState();
        }
        else
        {
            // Display HUD message: 'There is already a fruit on the chopping board.'
        }

        _isPlaying = true;

        _mainCamera.enabled = false;
        _minigameCamera.enabled = true;
        _minigameHands.enabled = true;
        _minigameHUD.SetActive(true);

        Globals.handleMovement = false;
        Globals.handleKBLook = false;
        PlayerManager.Instance.HideHands(true);

        _HUD.ClearHint(HintConstants.EnterCuttingMinigameHint);
        _HUD.CreateHint(HintConstants.ExitCuttingMinigameHint);
        _HUD.CreateHint(HintConstants.CancelCuttingMinigameHint);

        _knife.transform.SetPositionAndRotation(_knifeHoldTransform.position, _knifeHoldTransform.rotation);

        _player.Controls.Viva.UniversalInteract.performed -= EnterMinigame;
        _player.Controls.Viva.Pause.performed += ExitMinigame;
        _player.Controls.Viva.LeftGrab.performed += TryCutFruit;

        Globals.allowMenuOpen = false;
        Play();
    }

    private void ExitMinigame(InputAction.CallbackContext context)
    {
        if (!_isPlaying) { return; }

        _isPlaying = false;

        _mainCamera.enabled = true;
        _minigameCamera.enabled = false;
        _minigameHands.enabled = false;
        _minigameHUD.SetActive(false);

        Globals.handleMovement = true;
        Globals.handleKBLook = true;
        PlayerManager.Instance.HideHands(false);

        _knife.transform.SetPositionAndRotation(_knifeOrginalTransform.position, _knifeOrginalTransform.rotation);
        _player.Controls.Viva.Pause.performed -= ExitMinigame;
        _player.Controls.Viva.UniversalInteract.performed += EnterMinigame;
        _player.Controls.Viva.LeftGrab.performed -= TryCutFruit;

        _HUD.ClearHint(HintConstants.ExitCuttingMinigameHint);
        _HUD.ClearHint(HintConstants.CancelCuttingMinigameHint);
        _HUD.CreateHint(HintConstants.EnterCuttingMinigameHint);

        if (_objectInOtherHand != null)
        {
            PlayerKB_GrabObject grabScript = _objectInOtherHand.GetComponent<PlayerKB_GrabObject>();
            grabScript.SetIsActive(true, grabScript.GetIsGrabbed());
        }

        Globals.allowMenuOpen = true;
    }

    private void TryCutFruit(InputAction.CallbackContext context)
    {
        if (_isCutting || _noCutsLeft) { return; }

        _handAnimator.Play("Cut");
        StartCoroutine(InitiateCut());
        _cutCount++;
        float diff = Mathf.Abs(_movingPoint - _randomPoint);
        float accuracy = Mathf.Clamp(100 - (diff / 2 * 200), 0, 100);
        _totalAccuracy += accuracy;
        float avgAccuracy = _totalAccuracy / _cutCount;

        _accuracyText.text = $"{accuracy:0}%";
        _averageText.text = $"{avgAccuracy:0}%";

        switch (avgAccuracy)
        {
            case float n when n < 85: _averageText.color = Color.red; break;
            case float n when n >= 85 && n < 95: _averageText.color = Color.white; break;
            case float n when n >= 95: _averageText.color = Color.green; break;
        }

        PlayAnimation();
        SetRandomPoint();
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

        _progressBar.size = 1 - (float)_cutsLeft / (_selectedFruit switch
        {
            Fruit.Peach => PEACH_MAX,
            Fruit.Cantaloupe => CANTALOUPE_MAX,
            Fruit.Strawberry => STRAWBERRY_MAX,
            _ => 0
        });

        if (_cutsLeft <= 0)
        {
            _noCutsLeft = true;
            CompleteCutting(avgAccuracy);
        }
    }

    private IEnumerator InitiateCut()
    {
        _isCutting = true;
        _movingPointImage.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        if (!_noCutsLeft) { _movingPointImage.SetActive(true); }
        _isCutting = false;
    }

    private void CompleteCutting(float accuracy)
    {
        _movingPointImage.SetActive(false);
        _progressBarHandle.SetActive(false);

        switch (accuracy)
        {
            case float n when n < 80:
                _doneText.text = "Poor. Price -10%";
                _doneText.color = Color.red;
                break;
            case float n when n >= 80 && n < 95:
                _doneText.text = "Decent.";
                _doneText.color = Color.white;
                break;
            case float n when n >= 95:
                _doneText.text = "Excellent! Price +10%";
                _doneText.color = Color.green;
                break;
        }

        _doneText.enabled = true;
        _player.Controls.Viva.UniversalInteractHold.performed += PickUpBoard;

        _cutCount = 0;
        _cutsLeft = 0;
        _totalAccuracy = 0;
        _accuracyText.text = "--%";
        _averageText.text = "--%";
        _selectedFruit = Fruit.None;
    }

    private void CancelCutting(InputAction.CallbackContext context)
    {
        if (_noCutsLeft) { return; }

        _currentPrefabShowing.SetActive(false);
        ExitMinigame(context);

        GameObject obj = null;
        switch (_selectedFruit)
        {
            case Fruit.Peach: 
                obj = Instantiate(_peachPrefab.gameObject);
                obj.name = _peachPrefab.gameObject.name;
                break;
            case Fruit.Cantaloupe:
                obj = Instantiate(_cantaloupePrefab.gameObject);
                obj.name = _cantaloupePrefab.gameObject.name;
                break;
            case Fruit.Strawberry: 
                obj = Instantiate(_strawberryPrefab.gameObject);
                obj.name = _strawberryPrefab.gameObject.name;
                break;
        }

        Crop cropScript = obj.GetComponent<Crop>();
        cropScript.ShouldGrow = false;
        obj.transform.localScale = Vector3.one;
        cropScript.SetIsActive(true, 1);

        _cutCount = 0;
        _cutsLeft = 0;
        _totalAccuracy = 0;
        _accuracyText.text = "--%";
        _averageText.text = "--%";
        _selectedFruit = Fruit.None;
    }

    private void PickUpBoard(InputAction.CallbackContext context)
    {
        ExitMinigame(context);

        _cuttingBoard.transform.SetParent(_boardAttachPoint);
        _cuttingBoard.transform.localPosition = Vector3.zero;
    }

    private void SetFruitState()
    {
        switch (_selectedFruit)
        {
            case Fruit.Peach:
                _cutsLeft = PEACH_MAX;
                _currentPrefabShowing = _peaches[0];
                _currentPrefabShowing.SetActive(true);
                break;
            case Fruit.Cantaloupe: 
                _cutsLeft = CANTALOUPE_MAX; 
                _currentPrefabShowing = _cantaloupes[0];
                _currentPrefabShowing.SetActive(true);
                break;
            case Fruit.Strawberry: 
                _cutsLeft = STRAWBERRY_MAX;
                _currentPrefabShowing = _strawberries[0];
                _currentPrefabShowing.SetActive(true);
                break;
            default: _cutsLeft = 0; break;
        }

        _noCutsLeft = false;
        _progressBarHandle.SetActive(true);
        _doneText.enabled = false;
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
                case Fruit.Strawberry: _strawberries = list ; break;
            }
        }
    }

    private void SetRandomPoint()
    {
        _randomPoint = UnityEngine.Random.Range(0, _range);
        Vector3 vector = _target.transform.localPosition;
        _target.transform.localPosition = new Vector3(-150f + (_randomPoint * 300 / _range), vector.y, vector.z);
    }

    private void PlayAnimation()
    {
        switch (_selectedFruit)
        {
            case Fruit.Peach: Peach(); break;
            case Fruit.Cantaloupe: Cantaloupe(); break;
            case Fruit.Strawberry: Strawberry(); break;
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
            switch (_cutsLeft)
            {
                case 19: _minigameAnimator.Play("cutCantaloupe18"); break;
                case 18: _minigameAnimator.Play("cutCantaloupe17"); break;
                case 17: _minigameAnimator.Play("cutCantaloupe16"); break;
                case 16: _minigameAnimator.Play("cutCantaloupe15"); break;
                case 15: _minigameAnimator.Play("cutCantaloupe14"); break;
                case 14: _minigameAnimator.Play("cutCantaloupe13"); break;
                case 13: _minigameAnimator.Play("cutCantaloupe12"); break;
                case 12: _minigameAnimator.Play("cutCantaloupe11"); break;
                case 11: _minigameAnimator.Play("cutCantaloupe10"); break;
                case 10: _minigameAnimator.Play("cutCantaloupe9"); break;
                case 9: _minigameAnimator.Play("cutCantaloupe8"); break;
                case 8: _minigameAnimator.Play("cutCantaloupe7"); break;
                case 7: _minigameAnimator.Play("cutCantaloupe6"); break;
                case 6: _minigameAnimator.Play("cutCantaloupe5"); break;
                case 5: _minigameAnimator.Play("cutCantaloupe4"); break;
                case 4: _minigameAnimator.Play("cutCantaloupe3"); break;
                case 3: _minigameAnimator.Play("cutCantaloupe2"); break;
                case 2: _minigameAnimator.Play("cutCantaloupe1"); break;
                case 1: _minigameAnimator.Play("cutCantaloupe0"); break;
            }
        }   
    }


}
