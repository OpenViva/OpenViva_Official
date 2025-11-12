using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKB_Movement : MonoBehaviour
{
    // This class is responsible for the movement of the player using keyboard.
    // Code by Edenity on Unity Asset Store

    // --- Movement ---
    [Header("Movement")]
    [SerializeField, Range(1f, 20f)] private float _movementSpeed;
    [Tooltip("How much faster do you want to go when running?")]
    [SerializeField, Range(1f, 20f)] private float _runMultiplier;

    // --- Look ---
    [Header("Look")]
    [Range(1f, 20f)]
    [Tooltip("speed of the camera movement")]
    [SerializeField] private float _mouseSensitivity = 10;

    // --- Jump & Gravity ---
    [Header("Jump & Gravity")]
    [SerializeField, Range(1f, 20f)] private float _jumpHeight;
    [SerializeField] private float _gravity = -9.81f;

    // --- References ---
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _camera;

    // --- Debug ---
    [Header("Debug")]
    public Vector2 moveInput;
    public Vector2 lookInput;

    // --- Fields ---
    private float _xRotation;
    private float _yRotation;
    private Vector3 _controllerVelocity;
    private bool _isRunning = false;

    private void Start()
    {
        // Locks cursor and makes it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (TryGetComponent(out CharacterController foundController))
        {
            _characterController = foundController;
        }
        else Debug.LogWarning($"Character Controller of {this} cannot be assigned!");
    }

    void Update()
    {
        HandleGravity();
        HandleLook();
        HandleMovement();
    }

    private void LateUpdate()
    {
        // rotates the controller on the y-axis so that it is on the same rotation as the camera
        transform.localRotation = Quaternion.Euler(0f, _yRotation, 0f);

        // rotates camera on the y- and x-axis
        _camera.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        if (Globals.isMenuOpen) return;

        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y) * GetCurrentSpeed();
        Vector3 totalMove = (move + _controllerVelocity) * Time.deltaTime;

        _characterController.Move(totalMove);
    }

    void HandleLook()
    {
        if (lookInput == Vector2.zero || Globals.isMenuOpen) return;

        _yRotation += lookInput.x * _mouseSensitivity * Time.deltaTime;
        _xRotation -= lookInput.y * _mouseSensitivity * Time.deltaTime;

        // Rotate camera up/down with clamp
        _xRotation = Mathf.Clamp(_xRotation, -90, 90);
    }

    public void HandleJump()
    {
        if (_characterController.isGrounded)
        {
            _controllerVelocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }
    }

    public void HandleRun(bool running)
    {
        _isRunning = running;
    }

    void HandleGravity()
    {
        // Fix for instantly snapping to the ground off edges
        if (_characterController.isGrounded && _controllerVelocity.y < 0)
        {
            _controllerVelocity.y = 0;
        }

        _controllerVelocity.y += _gravity * Time.deltaTime;
    }

    float GetCurrentSpeed()
    {
        return _isRunning ? _movementSpeed * _runMultiplier : _movementSpeed;
    }

    public void SetMovementSpeed(float newSpeed)
    {
        _movementSpeed = newSpeed;
    }

    public void DisableRunning(bool crouching)
    {
        if(crouching)
        {
            _runMultiplier = 1f;
        }
        else
        {
            _runMultiplier = 2.5f;
        }
    }
}
