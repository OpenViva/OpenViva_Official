using MinigameUIController;
using nTools.PrefabPainter;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FruitCutMinigame : MonoBehaviour
{
    // Setup Fields
    private Camera _mainCamera;
    [SerializeField] private Player _player;
    private AnimationIndexes _animationIndexes;

    [SerializeField] private Camera _minigameCamera;
    [SerializeField] private SkinnedMeshRenderer _minigameHands;
    [SerializeField] private Animator _minigameHandAnimator;
    [SerializeField] private GameObject _knife;
    [SerializeField] private Transform _knifeOriginalTransform;
    [SerializeField] private Transform _knifeHoldTransform;

    private bool _playerInRange = false;
    private bool _isPlaying = false;
    private PlayerKB_GrabObject _otherObjectGrabScriptL;
    private PlayerKB_GrabObject _otherObjectGrabScriptR;

    // Gameplay Fields
    [SerializeField] private float _range = 1.5f;
    private float _randomizedTarget;
    private float _timingLine;
    private bool _goingUp = true;

    // Cutting
    private bool _isCutting = false;
    private int _cutsMadeThisAttempt = 0;
    private int _totalCutsNeeded = 0;
    private float _accuracy;

    // Cancelling
    [SerializeField] private Prefab _strawberryCrop;
    [SerializeField] private Prefab _peachCrop;
    [SerializeField] private Prefab _cantaloupeCrop;

    public enum Fruit
    {
        None = 0,
        Peach = 2,
        Strawberry = 3,
        Cantaloupe = 4,
        Blueberry = 5
    }
    private Fruit _selectedFruit = Fruit.None;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        _animationIndexes = PlayerManager.Instance.AnimationKB.GetComponent<AnimationIndexes>();
        AssignInputs();
    }

    private void Update()
    {
        if (_isPlaying) { MoveTimingLine(); }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) { _playerInRange = true; }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) { _playerInRange = false; }
    }

    private void AssignInputs()
    {
        _player.Controls.Viva.UniversalInteract.performed += EnterMinigame;
        _player.Controls.Viva.LeftGrab.performed += PerformCut;
        _player.Controls.Viva.Pause.performed += context => LeaveMinigame(context, false);
        _player.Controls.Viva.Cancel.performed += QuitMinigame;
        _player.Controls.Viva.InteractLeftHold.performed += SkipMinigame;
    }

    private void EnterMinigame(InputAction.CallbackContext context)
    {
        if (_isPlaying || !Globals.isDesktopMode || !_playerInRange) { return; }

        bool fruitChanged = false;

        if (PlayerManager.Instance.LeftHandOccupied)
        {
            int itemIndexLeft = PlayerManager.Instance.GetItemLeft();
            PlayerKB_GrabObject grabScript = PlayerManager.Instance.GetObjectLeft().GetComponent<PlayerKB_GrabObject>();

            if (Enum.IsDefined(typeof(Fruit), itemIndexLeft) && _selectedFruit == Fruit.None)
            {
                _selectedFruit = (Fruit)itemIndexLeft;
                grabScript.SetIsActive(false, 1);
                Destroy(grabScript.gameObject);
                fruitChanged = true;
            }
            else { _otherObjectGrabScriptL = grabScript; }
        }

        if (PlayerManager.Instance.RightHandOccupied)
        {
            int itemIndexRight = PlayerManager.Instance.GetItemRight();
            PlayerKB_GrabObject grabScript = PlayerManager.Instance.GetObjectRight().GetComponent<PlayerKB_GrabObject>();

            if (Enum.IsDefined(typeof(Fruit), itemIndexRight) && _selectedFruit == Fruit.None)
            {
                _selectedFruit = (Fruit)itemIndexRight;
                grabScript.SetIsActive(false, 2);
                Destroy(grabScript.gameObject);
                fruitChanged = true;
            }
            else { _otherObjectGrabScriptR = grabScript; }
        }

        if (fruitChanged)
        {
            CuttingBoard.Instance.EnableFirstObject(_selectedFruit);
            SetNewTarget();

            switch (_selectedFruit)
            {
                case Fruit.Strawberry: _totalCutsNeeded = CuttingBoard.STRAWBERRY_MAX_CUTS; break;
                case Fruit.Peach: _totalCutsNeeded = CuttingBoard.PEACH_MAX_CUTS; break;
                case Fruit.Cantaloupe: _totalCutsNeeded = CuttingBoard.CANTALOUPE_MAX_CUTS; break;
            }

            CuttingMinigame.Instance.DoneTextEnabled(false, _accuracy);
            SetNewTarget();
            CuttingMinigame.Instance.MovingPointEnabled(true);
        }

        if (_selectedFruit != Fruit.None)
        {
            _isPlaying = true;
            _mainCamera.enabled = false;
            _minigameCamera.enabled = true;
            _minigameHands.enabled = true;
            PlayerManager.Instance.HideHands(true);
            PlayerManager.Instance.LeftHandOccupied = true;
            PlayerManager.Instance.RightHandOccupied = true;
            Globals.handleKBLook = false;
            Globals.handleMovement = false;
            Globals.allowMenuOpen = false;
            _knife.transform.SetPositionAndRotation(_knifeHoldTransform.position, _knifeHoldTransform.rotation);

            if (_otherObjectGrabScriptL != null) { _otherObjectGrabScriptL.SetIsActive(false, 1); }
            if (_otherObjectGrabScriptR != null) { _otherObjectGrabScriptR.SetIsActive(false, 2); }

            CuttingMinigame.Instance.CuttingMinigameUIEnabled(true);
        }
        else
        {
            _otherObjectGrabScriptL = null;
            _otherObjectGrabScriptR = null;
        }
    }

    private void SetNewTarget()
    {
        _randomizedTarget = UnityEngine.Random.Range(0, _range);
        CuttingMinigame.Instance.SetBackgroundPosition(_randomizedTarget, _range);
    }

    private void MoveTimingLine()
    {
        if (_goingUp) { _timingLine += Time.deltaTime; }
        else { _timingLine -= Time.deltaTime; }

        _timingLine = Mathf.Clamp(_timingLine, 0, _range);
        CuttingMinigame.Instance.SetMovingPointPosition(_timingLine, _range);

        if (_timingLine >= _range) { _goingUp = false; }
        else if (_timingLine <= 0) { _goingUp = true; }
    }

    private void PerformCut(InputAction.CallbackContext context)
    {
        if (!_isPlaying || _isCutting || _cutsMadeThisAttempt >= _totalCutsNeeded) { return; }

        _minigameHandAnimator.Play("Cut");
        StartCoroutine(DeclareCut());
        _cutsMadeThisAttempt++;

        float diff = Mathf.Abs(_timingLine - _randomizedTarget);
        float cutAccuracy = Mathf.Clamp(100 - (diff / 2 * 200), 0, 100);
        _accuracy = (_accuracy * ((_cutsMadeThisAttempt - 1f) / _cutsMadeThisAttempt)) + (cutAccuracy * (1f / _cutsMadeThisAttempt));

        CuttingMinigame.Instance.SetAccuracy(cutAccuracy, _accuracy);
        CuttingBoard.Instance.EnableNextObject(_cutsMadeThisAttempt);
        CuttingMinigame.Instance.SetProgressBarSize((float)_cutsMadeThisAttempt / _totalCutsNeeded);

        if (_cutsMadeThisAttempt == _totalCutsNeeded) { CuttingMinigame.Instance.DoneTextEnabled(true, _accuracy); }

        SetNewTarget();

        IEnumerator DeclareCut()
        {
            _isCutting = true;
            CuttingMinigame.Instance.MovingPointEnabled(false);
            yield return new WaitForSeconds(1.5f);
            if (_cutsMadeThisAttempt < _totalCutsNeeded) { CuttingMinigame.Instance.MovingPointEnabled(true); }
            _isCutting = false;
        }
    }

    private void LeaveMinigame(InputAction.CallbackContext context, bool dropOtherItems)
    {
        if (!_isPlaying) { return; }

        CuttingMinigame.Instance.CuttingMinigameUIEnabled(false);
        _knife.transform.SetPositionAndRotation(_knifeOriginalTransform.position, _knifeOriginalTransform.rotation);
        Globals.allowMenuOpen = true;
        Globals.handleMovement = true;
        Globals.handleKBLook = true;
        PlayerManager.Instance.RightHandOccupied = false;
        PlayerManager.Instance.LeftHandOccupied = false;
        PlayerManager.Instance.HideHands(false);
        _minigameHands.enabled = false;
        _minigameCamera.enabled = false;
        _mainCamera.enabled = true;
        _isPlaying = false;

        if (_otherObjectGrabScriptL != null)
        {
            if (dropOtherItems)
            {
                _otherObjectGrabScriptL.SetIsActive(false, 1);
                _otherObjectGrabScriptL.gameObject.SetActive(true);
                _otherObjectGrabScriptL.IsActive = true;
            }
            else
            {
                _otherObjectGrabScriptL.SetIsActive(true, 1);
            }

            _otherObjectGrabScriptL = null;
        }

        if (_otherObjectGrabScriptR != null)
        {
            if (dropOtherItems)
            {
                _otherObjectGrabScriptR.SetIsActive(false, 2);
                _otherObjectGrabScriptR.gameObject.SetActive(true);
                _otherObjectGrabScriptR.IsActive = true;
            }
            else
            {
                _otherObjectGrabScriptR.SetIsActive(true, 2);
            }

            _otherObjectGrabScriptR = null;
        }
    }

    private void QuitMinigame(InputAction.CallbackContext context)
    {
        if (!_isPlaying || _cutsMadeThisAttempt >= _totalCutsNeeded) { return; }

        // At least one hand must be free.
        int freeHand = 0;
        if (_otherObjectGrabScriptL != null && _otherObjectGrabScriptR != null)
        {
            _otherObjectGrabScriptR.SetIsActive(true, 2);
            _otherObjectGrabScriptR.gameObject.transform.SetParent(null, true);
            _otherObjectGrabScriptR = null;
            freeHand = 2;
        }
        else if (_otherObjectGrabScriptL != null)
        {
            _otherObjectGrabScriptL.SetIsActive(true, 1);
            _otherObjectGrabScriptL = null;
            freeHand = 2;
        }
        else if (_otherObjectGrabScriptR != null)
        {
            _otherObjectGrabScriptR.SetIsActive(true, 2);
            _otherObjectGrabScriptR = null;
            freeHand = 1;
        }

        Crop cropScript = null;
        switch (_selectedFruit)
        {
            case Fruit.Strawberry:
                cropScript = Instantiate(_strawberryCrop.gameObject).GetComponent<Crop>();
                cropScript.gameObject.name = _strawberryCrop.gameObject.name;
                break;

            case Fruit.Peach:
                cropScript = Instantiate(_peachCrop.gameObject).GetComponent<Crop>();
                cropScript.gameObject.name = _peachCrop.gameObject.name;
                break;

            case Fruit.Cantaloupe:
                cropScript = Instantiate(_cantaloupeCrop.gameObject).GetComponent<Crop>();
                cropScript.gameObject.name = _cantaloupeCrop.gameObject.name;
                break;
        }
        cropScript.ShouldGrow = false;
        cropScript.gameObject.transform.localScale = Vector3.one;
        cropScript.SetIsActive(true, freeHand);

        _selectedFruit = Fruit.None;
        CuttingBoard.Instance.EnableFirstObject(_selectedFruit);
        LeaveMinigame(context, false);
    }

    private void SkipMinigame(InputAction.CallbackContext context)
    {
        if (!_isPlaying || _cutsMadeThisAttempt >= _totalCutsNeeded) { return; }

        _minigameHandAnimator.Play("Cut");
        _cutsMadeThisAttempt = _totalCutsNeeded;
        _accuracy = 85;
        CuttingBoard.Instance.EnableNextObject(_cutsMadeThisAttempt);
        CuttingMinigame.Instance.SetAccuracy(_accuracy, _accuracy);
        CuttingMinigame.Instance.DoneTextEnabled(true, _accuracy);
        CuttingMinigame.Instance.SetProgressBarSize((float)_cutsMadeThisAttempt / _totalCutsNeeded);
        CuttingMinigame.Instance.MovingPointEnabled(false);
    }

    //private IEnumerator ResetMinigame(InputAction.CallbackContext context)
    //{
    //    if (!_isPlaying || _cutsMadeThisAttempt < _totalCutsNeeded) { yield break; }

    //    LeaveMinigame(context, true);

    //    CuttingMinigame.Instance.SetAccuracy(101, 101); // Set text to '--%'
    //    CuttingMinigame.Instance.DoneTextEnabled(false, _accuracy);
    //    CuttingMinigame.Instance.SetProgressBarSize(0);
    //    CuttingMinigame.Instance.MovingPointEnabled(false);   
    //    _selectedFruit = Fruit.None;
    //    _cutsMadeThisAttempt = 0;
    //}


    private void OnDisable()
    {
        _player.Controls.Viva.UniversalInteract.performed -= EnterMinigame;
        _player.Controls.Viva.LeftGrab.performed -= PerformCut;
        _player.Controls.Viva.Pause.performed -= context => LeaveMinigame(context, false);
        _player.Controls.Viva.Cancel.performed -= QuitMinigame;
        _player.Controls.Viva.InteractLeftHold.performed -= SkipMinigame;
    }
}
