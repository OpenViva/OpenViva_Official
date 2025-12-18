#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKB_BasicActions : MonoBehaviour
{
    // This class manages basic actions for a keyboard player.
    // Code by Saien

    // --- References ---
    [Header("References")]
    [Tooltip("The object that holds the player movement (PlayerKB/PlayerVR)")]
    [SerializeField] private GameObject _player;
    [SerializeField] private PlayerKB_Movement _playerMovement;
    [SerializeField] private CharacterController _characterController;
    [Tooltip("The object that holds the player hands/view model")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _map;

    // --- Fields ---
    private bool _isCrouching = false;
    private int _currentHandPos = 10;
    private bool _mapOpen = false;
    private DesktopInput _playerInput;

    private void Awake()
    {
        _playerInput = new DesktopInput();

        // Move binding
        _playerInput.Viva.Move.performed += ctx => _playerMovement.moveInput = ctx.ReadValue<Vector2>();
        _playerInput.Viva.Move.canceled += ctx => _playerMovement.moveInput = Vector2.zero;

        // Look binding
        _playerInput.Viva.Look.performed += ctx => _playerMovement.lookInput = ctx.ReadValue<Vector2>();
        _playerInput.Viva.Look.canceled += ctx => _playerMovement.lookInput = Vector2.zero;

        // Run binding
        _playerInput.Viva.Run.performed += ctx => _playerMovement.HandleRun(true);
        _playerInput.Viva.Run.canceled += ctx => _playerMovement.HandleRun(false);

        // Jump binding
        _playerInput.Viva.Jump.performed += ctx => _playerMovement.HandleJump();

        // Other bindings
        _playerInput.Viva.Crouch.performed += OnCrouch;
        _playerInput.Viva.ExtendHands.performed += OnExtendHands;
        _playerInput.Viva.RetractHands.performed += OnRetractHands;
        _playerInput.Viva.OpenMap.performed += OnChangeMapVisibility;
    }
    void Start()
    {
        if (TryGetComponent(out CharacterController foundController))
        {
            _characterController = foundController;
        }
        else Debug.LogWarning($"Character Controller of {this} cannot be found!");

        if (TryGetComponent(out PlayerKB_Movement foundMovement))
        {
            _playerMovement = foundMovement;
        }
        else Debug.LogWarning($"Player Movement of {this} cannot be found!");
    }

    private void OnEnable() => _playerInput.Viva.Enable();
    private void OnDisable() => _playerInput.Viva.Disable();

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (!_isCrouching)
        {
            _playerMovement.SetMovementSpeed(1f);
            _playerMovement.DisableRunning(true);
            _characterController.height /= 6;
            _isCrouching = true;
        }
        else
        {
            _playerMovement.SetMovementSpeed(3.5f);
            _playerMovement.DisableRunning(false);
            _characterController.Move(Vector3.up * 0.1f);
            _characterController.height *= 6;
            _isCrouching = false;
        }
    }

    private void OnExtendHands(InputAction.CallbackContext context)
    {
        if (_currentHandPos <= 50)
        {
            _playerPrefab.transform.Translate(Vector3.right * 0.01f);
            _currentHandPos++;
        }
        
    }

    private void OnRetractHands(InputAction.CallbackContext context)
    {
        if (_currentHandPos >= 0)
        {
            _playerPrefab.transform.Translate(Vector3.left * 0.01f);
            _currentHandPos--;
        }
    }

    private void OnChangeMapVisibility(InputAction.CallbackContext context)
    {
        _mapOpen = !_mapOpen;
        _map.SetActive(_mapOpen);
    }
}

#endif