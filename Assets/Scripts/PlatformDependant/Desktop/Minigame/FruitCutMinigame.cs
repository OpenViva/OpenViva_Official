using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FruitCutMinigame : MonoBehaviour
{
    [SerializeField] private Camera _minigameCamera;
    [SerializeField] private SkinnedMeshRenderer _minigameHands;
    private Animator _minigameAnimator;

    private GameObject _knife;
    private Transform _knifeOrginalTransform;
    private Transform _knifeHoldTransform;

    [SerializeField] private Player _player;
    [SerializeField] private HintManager _HUD;
    private Camera _mainCamera;

    private bool _isPlaying = false;
    private int _cutsLeft = 0;

    // Minigame fields
    [SerializeField] private float _range = 2f;
    private float _randomPoint;
    private float _movingPoint;
    private bool _goingUp = true;

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

        _knife = transform.GetChild(1).gameObject;
        _knifeOrginalTransform = transform.GetChild(2);
        _knifeHoldTransform = transform.GetChild(3);

        AssignInputs();
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

        _knife.transform.position = _knifeHoldTransform.position;
        _knife.transform.rotation = _knifeHoldTransform.rotation;

        _player.Controls.Viva.UniversalInteract.performed -= EnterMinigame;
        _player.Controls.Viva.UniversalInteract.performed += ExitMinigame;
        _player.Controls.Viva.InteractLeft.performed += TryCutFruit;

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

        _knife.transform.position = _knifeOrginalTransform.position;
        _knife.transform.rotation = _knifeOrginalTransform.rotation;

        _player.Controls.Viva.UniversalInteract.performed -= ExitMinigame;
        _player.Controls.Viva.UniversalInteract.performed += EnterMinigame;
        _player.Controls.Viva.InteractLeft.performed += TryCutFruit;
    }

    private void TryCutFruit(InputAction.CallbackContext context)
    {
    }

    private void SetFruitState()
    {
        switch (_selectedFruit)
        {
            case Fruit.Peach: _cutsLeft = 6; break;
            case Fruit.Cantaloupe: _cutsLeft = 18; break;
            case Fruit.Strawberry: _cutsLeft = 2; break;
            default: _cutsLeft = 0; break;
        }
    }
}
