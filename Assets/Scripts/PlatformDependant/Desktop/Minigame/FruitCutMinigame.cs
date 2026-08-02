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
    [SerializeField] private HintManager _HUD;

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
    [SerializeField] private GameObject _strawberryCrop;
    [SerializeField] private GameObject _peachCrop;
    [SerializeField] private GameObject _cantaloupeCrop;

    public enum Fruit
    {
        None = 0,
        Peach = 2,
        Strawberry = 3,
        Cantaloupe = 4
    }
    private Fruit _selectedFruit = Fruit.None;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        AssignInputs();
    }

    private void Update()
    {
        if (_isPlaying) { MoveTimingLine(); }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) 
        { 
            _playerInRange = true;
            _HUD.CreateHint(HintConstants.EnterCuttingMinigameHint);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) 
        { 
            _playerInRange = false;
            _HUD.ClearHint(HintConstants.EnterCuttingMinigameHint);
        }
    }

    private void AssignInputs()
    {
        _player.Controls.Viva.UniversalInteract.performed += EnterMinigame;
        _player.Controls.Viva.LeftGrab.performed += PerformCut;
        _player.Controls.Viva.Pause.performed +=  LeaveMinigame;
        _player.Controls.Viva.Cancel.performed += QuitMinigame;
        _player.Controls.Viva.InteractLeftHold.performed += SkipMinigame;
        // _player.Controls.Viva.InteractRightHold.performed += MovePiecesToBowl;
        _player.Controls.Viva.UniversalInteractHold.performed += MovePiecesToPot;
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
            _HUD.ClearHint(HintConstants.EnterCuttingMinigameHint);
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

        if (_cutsMadeThisAttempt == _totalCutsNeeded) 
        { 
            CuttingMinigame.Instance.DoneTextEnabled(true, _accuracy);
            CuttingMinigame.Instance.SetControlHints(true);
        }

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

    private void LeaveMinigame(InputAction.CallbackContext context)
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
            _otherObjectGrabScriptL.SetIsActive(true, 1);
            _otherObjectGrabScriptL = null;
        }

        if (_otherObjectGrabScriptR != null && !_otherObjectGrabScriptR.IsActive)
        {
            _otherObjectGrabScriptR.SetIsActive(true, 2);
            _otherObjectGrabScriptR = null;
        }
    }

    private void QuitMinigame(InputAction.CallbackContext context)
    {
        if (!_isPlaying || _cutsMadeThisAttempt >= _totalCutsNeeded) { return; }

        int freeHand = 1;
        if (_otherObjectGrabScriptL != null && _otherObjectGrabScriptR != null)
        {
            _otherObjectGrabScriptR.SetIsActive(false, 2);
            _otherObjectGrabScriptR.gameObject.SetActive(true);
            _otherObjectGrabScriptR = null;
            freeHand = 2;
        }
        else if (_otherObjectGrabScriptL != null) { freeHand = 2; }

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
        cropScript.SetIsActive(false, freeHand);

        if (freeHand == 1) { _otherObjectGrabScriptL = cropScript; }
        else {  _otherObjectGrabScriptR = cropScript; }

        CuttingBoard.Instance.EnableFirstObject(_selectedFruit);
        ResetMinigame(context);
        LeaveMinigame(context);
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
        CuttingMinigame.Instance.SetControlHints(true);
    }

    //private void MovePiecesToBowl(InputAction.CallbackContext context)
    //{
    //    if (!_isPlaying || _cutsMadeThisAttempt < _totalCutsNeeded) { return; };

    //    CuttingBoard.Instance.LastTouchedBowl.CollectFruitPieces(_selectedFruit);
    //    CuttingBoard.Instance.ResetBoard();
    //    ResetMinigame(context);
    //}

    private void MovePiecesToPot(InputAction.CallbackContext context)
    {
        if (!_isPlaying || _cutsMadeThisAttempt < _totalCutsNeeded) { return; }

        if (CuttingBoard.Instance.LastTouchedPot.CollectFruitPieces(_selectedFruit))
        {
            CuttingBoard.Instance.ResetBoard();
            ResetMinigame(context);
        }
        else
        {
            StartCoroutine(CuttingMinigame.Instance.ShowContainerFullText());
        }
    }

    private void ResetMinigame(InputAction.CallbackContext context)
    {
        LeaveMinigame(context);

        CuttingMinigame.Instance.SetAccuracy(101, 101);
        CuttingMinigame.Instance.DoneTextEnabled(false, _accuracy);
        CuttingMinigame.Instance.SetProgressBarSize(0);
        CuttingMinigame.Instance.SetControlHints(false);
        _selectedFruit = Fruit.None;
        CuttingBoard.Instance.EnableFirstObject(_selectedFruit);
        _cutsMadeThisAttempt = 0;
    }

    private void OnDisable()
    {
        _player.Controls.Viva.UniversalInteract.performed -= EnterMinigame;
        _player.Controls.Viva.LeftGrab.performed -= PerformCut;
        _player.Controls.Viva.Pause.performed -= LeaveMinigame;
        _player.Controls.Viva.Cancel.performed -= QuitMinigame;
        _player.Controls.Viva.InteractLeftHold.performed -= SkipMinigame;
        // _player.Controls.Viva.InteractRightHold.performed -= MovePiecesToBowl;
        _player.Controls.Viva.UniversalInteractHold.performed -= MovePiecesToPot;
    }
}
