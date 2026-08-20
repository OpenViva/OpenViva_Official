#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using IngameDebugConsole;
using UnityEngine;

public class PlayerKB_Movement : MonoBehaviour
{
    // This class is responsible for the movement of the player using keyboard.
    // Code by Edenity on Unity Asset Store

    // --- Movement ---
    [Header("Movement")]
    [field: SerializeField ,Range(1f, 200f)] 
    public float MovementSpeed { get; private set; } = 2;
    [Tooltip("How much faster do you want to go when running?")]
    [SerializeField, Range(1f, 200f)] private float _runMultiplier;
    [Tooltip("How steep of an angle is detected as ground")]
    [SerializeField] private float edgeAngleTolerance = 45f;
    [Tooltip("Radius for ground detection")]
    [SerializeField] private float groundCheckRadius = 0.3f;
    [Tooltip("Distance to check for ground from player center")]
    [SerializeField] private float groundCheckDistance = 0f;
    [SerializeField] private float feetOffset = 0f;
    [Tooltip("Length of the edge detection raycasts")]
    [SerializeField] private float edgeRaycastLength = 0.53f;
    [Tooltip("Force applied to push the player off the edge")]
    [SerializeField] private float edgePushForce = 2f;

    // --- Look ---
    [Header("Look")]
    [Range(0.1f, 10f)]
    [Tooltip("speed of the camera movement")]
    [SerializeField] private float _mouseSensitivity = 2;

    [Tooltip("Mouse Smoothing (Optional)")]
    [Range(0f, 0.5f)]
    public float lookSmoothing = 0f;
    private Vector2 _currentLookVelocity;

    // --- Jump & Gravity ---
    [Header("Jump & Gravity")]
    [field: SerializeField, Range(1f, 200f)]
    public float JumpHeight { get; private set; } = 1;
    [SerializeField] private float _gravity = -9.81f;

    [Header("Jump Buffer")]
    [Tooltip("How long (seconds) the jump input is buffered for")]
    [SerializeField, Range(0f, 0.3f)] private float jumpBufferTime = 0.15f;
    [Tooltip("Coyote time: How long after leaving ground you can still jump")]
    [SerializeField, Range(0f, 0.3f)] private float coyoteTime = 0.1f;

    // --- References ---
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _camera;

    // --- Debug ---
    [Header("Debug")]
    public Vector2 moveInput;
    public Vector2 lookInput;
    [SerializeField] private bool isGrounded = false;

    [SerializeField] private bool jumpBuffered = false;
    [SerializeField] private float jumpBufferTimer = 0f;
    [SerializeField] private bool coyoteActive = false;
    [SerializeField] private float coyoteTimer = 0f;
    [SerializeField] private float _timeSinceGrounded = 0f;

    // --- Fields ---
    private Player _player;

    private float _xRotation;
    private float _yRotation;
    private Vector3 _controllerVelocity;
    private bool _isRunning = false;
    private bool _jumpQueued = false;

    private void Start()
    {
        _player = GetComponentInParent<Player>();

        DebugLogConsole.AddCommandInstance("PlayerSpeed", "Change the player speed, a value between 1 and 200", nameof(ChangePlayerSpeed), this);
        DebugLogConsole.AddCommandInstance("PlayerJump", "Change the player jump height, a value between 1 and 200", nameof(ChangePlayerJumpHeight), this);
        DebugLogConsole.AddCommandInstance("PlayerGrav", "Change the player gravity, a value between -1 and -300", nameof(ChangePlayerGravity), this);

        // Locks cursor and makes it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (TryGetComponent(out CharacterController foundController))
        {
            _characterController = foundController;
        }
        else Debug.LogWarning($"Character Controller of {this} cannot be assigned!");

        AssignInputEvents();
    }

    void Update()
    {
        isGrounded = GroundCheck();

        UpdateJumpBuffer();
        UpdateCoyoteTime();

        if (isGrounded && _jumpQueued)
        {
            ExecuteJump();
            _jumpQueued = false;
        }

        HandleGravity();
        HandleLook();
        HandleMovement();
    }

    private void LateUpdate()
    {
        // Rotates the controller on the Y-axis so that it is on the same rotation as the camera
        transform.localRotation = Quaternion.Euler(0f, _yRotation, 0f);

        // Rotates camera on the Y and X-axis
        _camera.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }

    public void HandleRun(bool running)
    {
        _isRunning = running;
    }

    void HandleMovement()
    {
        if (Globals.isMenuOpen || !Globals.handleMovement)
        {
            /*
             * Temp fix for footstep sounds.
             * When menu is opened or when handleMovement is false
             * charaController retains velocity rather than becoming zero.
             */
            _characterController.Move(Vector3.zero); 
            return;
        }

        if (!isGrounded)
        {
            DetectAndPushFromEdge();
        }

        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y) * GetCurrentSpeed();
        Vector3 totalMove = (move + _controllerVelocity) * Time.deltaTime;

        _characterController.Move(totalMove);
    }

    void HandleLook()
    {
        if (lookInput == Vector2.zero || Globals.isMenuOpen || !Globals.handleKBLook) return;

        Vector2 input = lookInput;

        if (lookSmoothing > 0f)
        {
            input = Vector2.SmoothDamp(Vector2.zero, lookInput, ref _currentLookVelocity,
                                       lookSmoothing, Mathf.Infinity, Time.unscaledDeltaTime);
        }

        _yRotation += input.x * _mouseSensitivity;
        _xRotation -= input.y * _mouseSensitivity;

        // Rotate camera up/down with clamp
        _xRotation = Mathf.Clamp(_xRotation, -90, 90);
    }

    #region Jump Logic
    public void HandleJump()
    {
        // Can jump if grounded OR within coyote time
        if (isGrounded || coyoteActive)
        {
            ExecuteJump();
        }
        else
        {
            _jumpQueued = true; // Otherwise buffer the jump input
        }
    }

    private void ExecuteJump()
    {
        _controllerVelocity.y = Mathf.Sqrt(JumpHeight * -2f * _gravity);
    }

    private void UpdateJumpBuffer()
    {
        if (_jumpQueued)
        {
            jumpBufferTimer -= Time.deltaTime;
            jumpBuffered = jumpBufferTimer > 0f;

            // Buffer expired
            if (jumpBufferTimer <= 0f)
            {
                _jumpQueued = false;
                jumpBuffered = false;
            }
        }
        else
        {
            jumpBuffered = false;
            jumpBufferTimer = jumpBufferTime;
        }
    }

    private void UpdateCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            coyoteActive = true;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
            coyoteActive = coyoteTimer > 0f;
        }

        _timeSinceGrounded = isGrounded ? 0f : _timeSinceGrounded + Time.deltaTime;
    }
    #endregion

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
        return _isRunning ? MovementSpeed * _runMultiplier : MovementSpeed;
    }

    #region Public Methods
    public bool GetPlayerGrounded()
    {
        return isGrounded;
    }

    public void SetMovementSpeed(float newSpeed)
    {
        MovementSpeed = newSpeed;
    }

    public void DisableRunning(bool crouching)
    {
        if (crouching)
        {
            _runMultiplier = 1f;
        }
        else
        {
            _runMultiplier = 2.5f;
        }
    }

    public bool GetIsGrouned() { return isGrounded; }
    #endregion

    #region Helper Methods
    void DetectAndPushFromEdge()
    {
        // Cast rays in 4 directions (forward, backward, left, right) to detect edges at the player's feet
        Vector3[] directions = { transform.forward, -transform.forward, transform.right, -transform.right };

        // Get the player's feet position
        Vector3 feetPosition = transform.position + Vector3.up * (feetOffset + 0.1f);

        foreach (var direction in directions)
        {
            RaycastHit hit;

            // Cast the ray from the player's feet in the specified direction
            if (Physics.Raycast(feetPosition, direction, out hit, edgeRaycastLength))
            {
                // If the surface is too steep (an edge), push the player away from it
                if (Vector3.Angle(hit.normal, Vector3.up) > 45f) // Steep surfaces are considered edges
                {
                    Vector3 pushDirection = hit.normal.normalized; // Opposite of the platform's surface normal
                    _characterController.Move(edgePushForce * Time.deltaTime * pushDirection);
                    return; // Only apply one push per frame
                }
            }
        }
    }

    bool GroundCheck()
    {
        Vector3 sphereOrigin = transform.position + Vector3.up * (groundCheckRadius + 0.1f);

        // Perform a SphereCast slightly below the player to detect the ground
        if (Physics.SphereCast(sphereOrigin, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance))
        {
            if (hit.collider.gameObject == gameObject) return false;

            // Ensure the surface normal is facing upward enough to be considered "ground"
            if (Vector3.Angle(hit.normal, Vector3.up) < edgeAngleTolerance)
            {
                return true;
            }
        }

        return false; // Not grounded
    }
    #endregion

    void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * (groundCheckRadius + 0.05f);
        Gizmos.color = GroundCheck() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, groundCheckRadius);

        Gizmos.color = Color.blue;
        // Get the player's feet position
        Vector3 feetPosition = transform.position + Vector3.up * (feetOffset + 0.1f);

        // Draw edge detection rays from the player's feet
        Vector3[] directions = { transform.forward, -transform.forward, transform.right, -transform.right };
        foreach (var direction in directions)
        {
            Gizmos.DrawLine(feetPosition, feetPosition + direction * edgeRaycastLength);
        }
    }

    private void AssignInputEvents()
    {
        // Move binding
        _player.Controls.Viva.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        _player.Controls.Viva.Move.canceled += ctx => moveInput = Vector2.zero;

        // Look binding
        _player.Controls.Viva.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        _player.Controls.Viva.Look.canceled += ctx => lookInput = Vector2.zero;

        // Run binding
        _player.Controls.Viva.Run.performed += ctx => HandleRun(true);
        _player.Controls.Viva.Run.canceled += ctx => HandleRun(false);

        // Jump binding
        _player.Controls.Viva.Jump.performed += ctx => HandleJump();
    }

    #region Debug Commands
    public void ChangePlayerSpeed(float newSpeed)
    {
        MovementSpeed = Mathf.Clamp(newSpeed, 1f, 200f);

        Debug.Log("[Default: 2] Changed player speed to: " + newSpeed);
    }

    public void ChangePlayerJumpHeight(float newJumpHeight)
    {
        JumpHeight = Mathf.Clamp(newJumpHeight, 1f, 200f);

        Debug.Log("[Default: 1] Changed player jump height to: " + newJumpHeight);
    }

    public void ChangePlayerGravity(float newGravity)
    {
        _gravity = Mathf.Clamp(newGravity, -300f, -1f);

        Debug.Log("[Default: -9.81] Changed player gravity to: " + newGravity);
    }
    #endregion
}

#endif