using UnityEngine;
using UnityEngine.InputSystem;

public class FruitCutMinigame : MonoBehaviour
{
    [SerializeField] private Camera _minigameCamera;

    [SerializeField] private Player _player;
    [SerializeField] private HintManager _HUD;
    private PlayerKB_Movement _movementScript;
    private Camera _mainCamera;

    private bool _isPlaying = false;

    private void Awake()
    {
        _movementScript = _player.GetComponentInChildren<PlayerKB_Movement>();
        if (_movementScript == null)
        {
            Debug.LogWarning($"No PlayerKB_Movement script found in the children of '{_player.gameObject}'. " +
                $"\nMake sure that PlayerKB is a child of the Player GameObject and that it has a movement script." +
                $"\nFruit Cutting minigame disabled.");
            enabled = false;
        }
        _mainCamera = Camera.main;

        AssignInputs();
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

    private void EnterMinigame(InputAction.CallbackContext context)
    {
        if (_isPlaying || !Globals.isDesktopMode || _minigameCamera == null) { return; }

        _isPlaying = true;

        _mainCamera.enabled = false;
        _minigameCamera.enabled = true;

        _movementScript.SetInMinigame(true);

        _player.Controls.Viva.UniversalInteract.performed -= EnterMinigame;
        _player.Controls.Viva.UniversalInteract.performed += ExitMinigame;
    }

    private void ExitMinigame(InputAction.CallbackContext context)
    {
        if (!_isPlaying) { return; }

        _isPlaying = false;

        _mainCamera.enabled = true;
        _minigameCamera.enabled = false;

        _movementScript.SetInMinigame(false);

        _player.Controls.Viva.UniversalInteract.performed -= ExitMinigame;
        _player.Controls.Viva.UniversalInteract.performed += EnterMinigame;
    }
}
