using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKB_BasicActions : MonoBehaviour
{
    // This class manages basic actions for a keyboard player.
    // Code by Saien

    // FIELDS
    [SerializeField] private GameObject player;
    private CharacterController playerController;
    [SerializeField] private PlayerKB_Movement playerMovement;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject map;

    private bool isCrouching = false;
    private int currentHandPos = 10;
    private bool mapOpen = false;

    private DesktopInput playerInput;

    // PROPERTIES
    private void Awake()
    {
        playerInput = new DesktopInput();

        // Move binding
        playerInput.Viva.Move.performed += ctx => playerMovement.moveInput = ctx.ReadValue<Vector2>();
        playerInput.Viva.Move.canceled += ctx => playerMovement.moveInput = Vector2.zero;

        // Look binding
        playerInput.Viva.Look.performed += ctx => playerMovement.lookInput = ctx.ReadValue<Vector2>();
        playerInput.Viva.Look.canceled += ctx => playerMovement.lookInput = Vector2.zero;

        // Run binding
        playerInput.Viva.Run.performed += ctx => playerMovement.HandleRun(true);
        playerInput.Viva.Run.canceled += ctx => playerMovement.HandleRun(false);

        // Jump binding
        playerInput.Viva.Jump.performed += ctx => playerMovement.HandleJump();

        // Other bindings
        playerInput.Viva.Crouch.performed += OnCrouch;
        playerInput.Viva.ExtendHands.performed += OnExtendHands;
        playerInput.Viva.RetractHands.performed += OnRetractHands;
        playerInput.Viva.OpenMap.performed += OnChangeMapVisibility;
    }
    void Start()
    {
        playerController = player.GetComponent<CharacterController>();
    }

    private void OnEnable() => playerInput.Viva.Enable();
    private void OnDisable() => playerInput.Viva.Disable();

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (!isCrouching)
        {
            playerMovement.SetMovementSpeed(1f);
            playerMovement.DisableRunning(true);
            playerController.height /= 4;
            isCrouching = true;
        }
        else
        {
            player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 0.1f, player.transform.position.z);
            playerMovement.SetMovementSpeed(3.5f);
            playerMovement.DisableRunning(false);
            playerController.height *= 4;
            isCrouching = false;
        }
    }

    private void OnExtendHands(InputAction.CallbackContext context)
    {
        if (currentHandPos <= 50)
        {
            playerPrefab.transform.Translate(Vector3.right * 0.01f);
            currentHandPos++;
        }
        
    }

    private void OnRetractHands(InputAction.CallbackContext context)
    {
        if (currentHandPos >= 0)
        {
            playerPrefab.transform.Translate(Vector3.left * 0.01f);
            currentHandPos--;
        }
    }

    private void OnChangeMapVisibility(InputAction.CallbackContext context)
    {
        mapOpen = !mapOpen;
        map.SetActive(mapOpen);
    }
}
