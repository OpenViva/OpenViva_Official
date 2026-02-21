#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;

/// <summary>
/// Rigidbody-based player movement — faithfully replicates old OpenViva's
/// KeyboardController.OnFixedUpdateControl + Player.Keyboard physics.
///
/// Also bootstraps the entire interaction system at runtime (physics hands,
/// grab controller, hand driver, gestures) so everything "just works" without
/// any manual prefab wiring.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerKB_Movement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Base walk speed (old OpenViva: 0.2)")]
    public float walkSpeed = 0.2f;
    [Tooltip("Sprint multiplier when Alt/Shift held (old: 3x)")]
    public float sprintMultiplier = 3f;
    [Tooltip("Velocity damping per FixedUpdate (old: 0.85)")]
    public float moveDamping = 0.85f;

    [Header("Look")]
    [Tooltip("Mouse sensitivity (old: 70, applied * dt * 0.01)")]
    public float mouseSensitivity = 70f;
    [Tooltip("Mouse rotation smooth decay (old: 0.6)")]
    public float mouseDecay = 0.6f;
    [Tooltip("Max pitch angle from horizontal (old: 75)")]
    public float maxPitchAngle = 75f;

    [Header("Player Height")]
    [Tooltip("Standing head height (old: 1.4)")]
    public float standingHeight = 1.4f;
    [Tooltip("Crouching head height (old: 0.5)")]
    public float crouchHeight = 0.5f;
    [Tooltip("Crouch transition speed (old: 5)")]
    public float crouchSpeed = 5f;

    [Header("References")]
    [SerializeField] private Transform _camera;

    // --- Public input state (set by BasicActions) ---
    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public Vector2 lookInput;

    // --- Runtime ---
    Rigidbody rb;
    CapsuleCollider capsule;
    Vector3 moveVel;
    Vector2 mouseVelocitySum;
    float currentHeight;
    float targetHeight;
    bool _isRunning;
    bool _isCrouching;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        // Remove CharacterController if still present (conflicts with Rigidbody)
        var cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            Debug.LogWarning("[PlayerKB] Removing conflicting CharacterController");
            Destroy(cc);
        }

        // Auto-find camera if not assigned in inspector
        if (_camera == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) _camera = cam.transform;
        }
        if (_camera == null)
            _camera = Camera.main?.transform;

        // Match old OpenViva player Rigidbody setup
        rb.mass = 100f;
        rb.linearDamping = 1f;
        rb.angularDamping = 50f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        currentHeight = standingHeight;
        targetHeight = standingHeight;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // FOV = 65 (old OpenViva)
        if (_camera != null)
        {
            var cam = _camera.GetComponent<Camera>();
            if (cam != null) cam.fieldOfView = 65f;
        }

        // Setup capsule to match
        capsule.radius = 0.09f;
        capsule.height = standingHeight + 0.3f;
        capsule.center = Vector3.up * (capsule.height * 0.5f);
        capsule.direction = 1;

        // --- Bootstrap the entire interaction system ---
        BootstrapInteractionSystem();
    }

    void FixedUpdate()
    {
        if (_camera == null) return;
        if (Globals.isMenuOpen) return;

        // --- 1. Apply mouse rotation from PREVIOUS frame's accumulated velocity ---
        float angleSlow = 1f - Mathf.Abs(_camera.forward.y) * 0.5f;
        _camera.rotation *= Quaternion.Euler(mouseVelocitySum.x, mouseVelocitySum.y * angleSlow, 0f);
        mouseVelocitySum *= mouseDecay;

        // --- 2. Movement (old OpenViva style) ---
        Vector3 accel = new Vector3(moveInput.x, 0f, moveInput.y);
        if (accel != Vector3.zero)
        {
            Vector3 headForward = _camera.TransformDirection(accel);
            headForward.y = 0f;
            float speed = _isRunning ? walkSpeed * sprintMultiplier : walkSpeed;
            moveVel += headForward.normalized * speed;
        }

        moveVel *= moveDamping;
        moveVel.y = rb.linearVelocity.y;
        rb.linearVelocity = moveVel;

        // --- 3. Capsule + head height ---
        currentHeight += (targetHeight - currentHeight) * Time.fixedDeltaTime * crouchSpeed;
        _camera.localPosition = Vector3.up * currentHeight;
        UpdateCapsule();

        // --- 4. Add new mouse input for NEXT frame ---
        if (lookInput != Vector2.zero)
        {
            mouseVelocitySum += new Vector2(-lookInput.y, lookInput.x) * mouseSensitivity * Time.fixedDeltaTime * 0.01f;
        }

        // --- 5. Fix camera roll + clamp pitch ---
        _camera.rotation = Quaternion.LookRotation(_camera.forward, Vector3.up);
        _camera.rotation = Quaternion.RotateTowards(
            Quaternion.LookRotation(new Vector3(_camera.forward.x, 0f, _camera.forward.z)),
            _camera.rotation,
            maxPitchAngle
        );
    }

    void UpdateCapsule()
    {
        capsule.height = currentHeight + 0.3f;
        capsule.center = Vector3.up * (capsule.height * 0.5f);
    }

    #region Public API

    public void HandleRun(bool running) => _isRunning = running;

    public void HandleJump() { }

    public void HandleCrouch()
    {
        _isCrouching = !_isCrouching;
        targetHeight = _isCrouching ? crouchHeight : standingHeight;
    }

    public void SetMovementSpeed(float newSpeed) => walkSpeed = newSpeed;

    public void DisableRunning(bool crouching)
    {
        if (crouching) _isRunning = false;
    }

    #endregion

    #region Interaction System Bootstrap

    /// <summary>
    /// Auto-creates and wires all interaction components at runtime.
    /// Creates separate invisible physics hand GameObjects (like old OpenViva's
    /// handRigidBodyPrefab) so they don't conflict with the Animator bone hierarchy.
    /// </summary>
    void BootstrapInteractionSystem()
    {
        if (_camera == null)
        {
            Debug.LogError("[PlayerKB] No camera — cannot bootstrap interaction system");
            return;
        }

        // Find bone transforms in the hierarchy
        Transform wristL = FindChild(transform, "wrist_l");
        Transform wristR = FindChild(transform, "wrist_r");

        if (wristL == null || wristR == null)
        {
            Debug.LogError("[PlayerKB] Cannot find wrist_l/wrist_r — interaction system not created");
            return;
        }

        // Enable wrist_r Animator (disabled by default in prefab)
        var wristRAnimator = wristR.GetComponent<Animator>();
        if (wristRAnimator != null && !wristRAnimator.enabled)
            wristRAnimator.enabled = true;

        // --- Create physics hands (separate from bone hierarchy, like old game) ---
        var leftGrab = CreatePhysicsHand("PhysicsHand_L", true, wristL);
        var rightGrab = CreatePhysicsHand("PhysicsHand_R", false, wristR);

        // Ignore collision between physics hands and player capsule
        if (leftGrab.handCollider != null)
            Physics.IgnoreCollision(capsule, leftGrab.handCollider);
        if (rightGrab.handCollider != null)
            Physics.IgnoreCollision(capsule, rightGrab.handCollider);

        // --- HandAnimator on wrists ---
        var leftAnim = EnsureComponent<HandAnimator>(wristL.gameObject);
        leftAnim.isLeftHand = true;
        leftAnim.handAnimator = wristL.GetComponent<Animator>();

        var rightAnim = EnsureComponent<HandAnimator>(wristR.gameObject);
        rightAnim.isLeftHand = false;
        rightAnim.handAnimator = wristRAnimator;

        // --- DesktopHandDriver ---
        var handDriver = EnsureComponent<DesktopHandDriver>(gameObject);
        handDriver.cameraTransform = _camera;
        handDriver.leftHandGrab = leftGrab;
        handDriver.rightHandGrab = rightGrab;
        handDriver.leftWristBone = wristL;
        handDriver.rightWristBone = wristR;

        // --- PlayerKB_GrabController ---
        var grabController = EnsureComponent<PlayerKB_GrabController>(gameObject);
        grabController.leftHand = leftGrab;
        grabController.rightHand = rightGrab;
        grabController.handDriver = handDriver;
        grabController.leftHandAnimator = leftAnim;
        grabController.rightHandAnimator = rightAnim;
        grabController.playerCamera = _camera.GetComponent<Camera>();

        // --- DesktopGestures ---
        var gestures = EnsureComponent<DesktopGestures>(gameObject);
        gestures.leftHandAnimator = leftAnim;
        gestures.rightHandAnimator = rightAnim;
        gestures.leftHandGrab = leftGrab;
        gestures.rightHandGrab = rightGrab;

        Debug.Log("[PlayerKB] Interaction system bootstrapped: physics hands + grab + gestures ready");
    }

    /// <summary>
    /// Creates an invisible physics hand GameObject (like old OpenViva's handRigidBodyPrefab).
    /// NOT parented to the bone hierarchy — independent physics object that chases bone position.
    /// HandGrabSystem.Awake() auto-configures the Rigidbody (mass=5, no gravity, etc.)
    /// </summary>
    HandGrabSystem CreatePhysicsHand(string name, bool isLeft, Transform wristBone)
    {
        // Don't create duplicates
        var existing = GameObject.Find(name);
        if (existing != null)
        {
            var existingGrab = existing.GetComponent<HandGrabSystem>();
            if (existingGrab != null) return existingGrab;
        }

        var go = new GameObject(name);
        // NOT parented to player — independent physics object (child Rigidbodies are bad practice)
        go.transform.position = wristBone.position;
        go.transform.rotation = wristBone.rotation;

        // SphereCollider for physical interactions (old: radius 0.05)
        var col = go.AddComponent<SphereCollider>();
        col.radius = 0.05f;

        // HandGrabSystem auto-adds Rigidbody via RequireComponent
        // Its Awake() configures: mass=5, gravity=false, maxAngularVelocity=64, etc.
        var grab = go.AddComponent<HandGrabSystem>();
        grab.isLeftHand = isLeft;
        grab.gripPoint = go.transform;
        grab.handCollider = col;

        return grab;
    }

    #endregion

    #region Helpers

    static T EnsureComponent<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }

    static Transform FindChild(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            var found = FindChild(child, name);
            if (found != null) return found;
        }
        return null;
    }

    #endregion
}

#endif