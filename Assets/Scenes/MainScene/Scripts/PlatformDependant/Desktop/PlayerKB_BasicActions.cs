#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles basic desktop actions: movement, look, crouch, map.
/// Scroll (hand extend/retract) is handled by PlayerKB_GrabController → DesktopHandDriver.
/// </summary>
public class PlayerKB_BasicActions : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerKB_Movement _playerMovement;
    [SerializeField] private GameObject _map;

    DesktopInput _playerInput;

    void Awake()
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

        // Crouch + Map
        _playerInput.Viva.Crouch.performed += OnCrouch;
        _playerInput.Viva.OpenMap.performed += OnChangeMapVisibility;
    }

    void Start()
    {
        if (_playerMovement == null)
            TryGetComponent(out _playerMovement);
    }

    void OnEnable() => _playerInput.Viva.Enable();
    void OnDisable() => _playerInput.Viva.Disable();

    void OnCrouch(InputAction.CallbackContext context)
    {
        if (_playerMovement != null)
            _playerMovement.HandleCrouch();
    }

    void OnChangeMapVisibility(InputAction.CallbackContext context)
    {
        if (_map == null) return;
        _map.SetActive(!_map.activeSelf);
    }

    void OnDestroy()
    {
        if (_playerInput != null)
        {
            _playerInput.Viva.Crouch.performed -= OnCrouch;
            _playerInput.Viva.OpenMap.performed -= OnChangeMapVisibility;
            _playerInput.Dispose();
        }
    }
}

#endif