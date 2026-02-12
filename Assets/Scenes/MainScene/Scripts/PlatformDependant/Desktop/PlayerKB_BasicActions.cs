#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;
using UnityEngine.InputSystem;

// Allows the player to perform basic actions (KB&M)

public class PlayerKB_BasicActions : MonoBehaviour
{

    // --- References ---
    [Header("References")]
    [SerializeField] private PlayerKB_Movement _playerMovement;
    [SerializeField] private CharacterController _characterController;

    [Tooltip("The object that holds the player hands/view model")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _map;

    // --- Fields ---
    [SerializeField] private Player _player;
    private PlayerControls _controls;

    private bool _isCrouching = false;
    private int _currentHandPos = 10;
    private bool _mapOpen = false;
    private bool _bagOpen = false;

    private void Awake()
    {
        _player = GetComponentInParent<Player>();

        _controls = _player.Controls;

        _controls.Viva.Crouch.performed += OnCrouch;
        _controls.Viva.ScrollUp.performed += OnExtendHands;
        _controls.Viva.ScrollDown.performed += OnRetractHands;
        _controls.Viva.OpenMap.performed += OnChangeMapVisibility;
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

    public void SetBagOpen(bool set)
    {
        _bagOpen = set;
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        // Toggle crouch when [C] is pressed
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
        // Extend the hands forward when when the mouse wheel is scrolled up
        if (_currentHandPos <= 50 && !_bagOpen)
        {
            _playerPrefab.transform.Translate(Vector3.right * 0.01f);
            _currentHandPos++;
        }
        
    }

    private void OnRetractHands(InputAction.CallbackContext context)
    {
        // Retract the hands backward when the mouse wheel is scrolled down
        if (_currentHandPos >= 0 && !_bagOpen)
        {
            _playerPrefab.transform.Translate(Vector3.left * 0.01f);
            _currentHandPos--;
        }
    }

    private void OnChangeMapVisibility(InputAction.CallbackContext context)
    {
        // Toggle minimap visibility when [M] is pressed
        _mapOpen = !_mapOpen;
        _map.SetActive(_mapOpen);
    }
}

#endif