using MinigameUIController;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FruitCutMinigame : MonoBehaviour
{
    // Setup Fields
    private Camera _mainCamera;
    [SerializeField] private Player _player;

    [SerializeField] private Camera _minigameCamera;
    [SerializeField] private SkinnedMeshRenderer _minigameHands;
    [SerializeField] private Animator _minigameHandAnimator;
    [SerializeField] private GameObject _knife;
    [SerializeField] private Transform _knifeOriginalTransform;
    [SerializeField] private Transform _knifeHoldTransform;

    private bool _isPlaying;

    // Gameplay Fields
    [SerializeField] private float _range = 1.5f;
    private float _randomizedTarget;
    private float _timingLine;
    private bool _goingUp = true;

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

    private void AssignInputs()
    {
        _player.Controls.Viva.UniversalInteract.performed += EnterMinigame;
    }

    private void EnterMinigame(InputAction.CallbackContext context)
    {
        if (_isPlaying || !Globals.isDesktopMode) { return; }

        // Check for fruit
        bool fruitFound = false;
        if (_selectedFruit == Fruit.None)
        {
            PlayerKB_GrabObject grabScript = null;
            if (PlayerManager.Instance.LeftHandOccupied)
            {
                int itemInLeft = PlayerManager.Instance.GetItemLeft();
                if (Enum.IsDefined(typeof(Fruit), itemInLeft))
                {
                    _selectedFruit = (Fruit)itemInLeft;
                    grabScript = PlayerManager.Instance.GetObjectLeft().GetComponent<PlayerKB_GrabObject>();
                    grabScript.SetIsActive(false, 1);
                    fruitFound = true;
                }
            }
            else if (PlayerManager.Instance.RightHandOccupied)
            {
                int itemInLeft = PlayerManager.Instance.GetItemRight();
                if (Enum.IsDefined(typeof(Fruit), itemInLeft))
                {
                    _selectedFruit = (Fruit)itemInLeft;
                    grabScript = PlayerManager.Instance.GetObjectRight().GetComponent<PlayerKB_GrabObject>();
                    grabScript.SetIsActive(false, 2);
                    fruitFound = true;
                }
            }

            if (fruitFound)
            {
                CuttingBoard.Instance.EnableFirstObject(_selectedFruit);
                SetNewTarget();
            }
        }
        else { fruitFound = true; }

        // Setup
        if (!fruitFound) { return; }
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

        // Show UI
        CuttingMinigame.Instance.CuttingMinigameUIEnabled(true);
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
}
