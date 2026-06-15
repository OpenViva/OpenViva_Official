using UnityEngine;
using UnityEngine.InputSystem;

public class FruitCutMinigame : MonoBehaviour
{
    [SerializeField] private Camera _minigameCamera;
    [SerializeField] private SkinnedMeshRenderer _minigameHands;
    private GameObject _knife;
    private Transform _knifeOrginalTransform;
    private Transform _knifeHoldTransform;

    [SerializeField] private Player _player;
    [SerializeField] private HintManager _HUD;
    private Camera _mainCamera;

    private bool _isPlaying = false;

    private void Awake()
    {
        _mainCamera = Camera.main;

        if (_minigameHands == null)
        {
            Debug.LogWarning("No minigame hand model found. Fruit cutting minigame disabled.");
            enabled = false;
        }
        _minigameHands.enabled = false;

        _knife = transform.GetChild(1).gameObject;
        _knifeOrginalTransform = transform.GetChild(2);
        _knifeHoldTransform = transform.GetChild(3);

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
        _minigameHands.enabled = true;

        Globals.handleMovement = false;
        Globals.handleKBLook = false;
        PlayerManager.Instance.HideHands(true);

        _knife.transform.position = _knifeHoldTransform.position;
        _knife.transform.rotation = _knifeHoldTransform.rotation;

        _player.Controls.Viva.UniversalInteract.performed -= EnterMinigame;
        _player.Controls.Viva.UniversalInteract.performed += ExitMinigame;
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
    }
}
