using UnityEngine;

public class PlayerKB_Movement : MonoBehaviour
{
    // This class is responsible for the movement of the player using keyboard.
    // Code by Edenity on Unity Asset Store

    // --- Movement ---
    [Header("Movement")]
    [SerializeField, Range(1f, 20f)] private float _movementSpeed;

    [Tooltip("How much faster do you want to go when running?")]
    [SerializeField, Range(1f, 20f)] private float _runMultiplier;

    // --- Jump & Gravity ---
    [Header("Jump & Gravity")]
    [SerializeField, Range(1f, 20f)] private float _jumpHeight;

    [SerializeField] private float _gravity = -9.81f;

    // --- References ---
    [Header("References")]
    [SerializeField] private CharacterController _characterController;

    // --- Fields ---
    public Vector2 moveInput;
    public Vector2 lookInput;
    private Vector3 _controllerVelocity;
    private bool _isRunning = false;

    void Update()
    {
        HandleGravity();

        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y) * GetCurrentSpeed();
        Vector3 totalMove = (move + _controllerVelocity) * Time.deltaTime;

        _characterController.Move(totalMove);
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
