using UnityEngine;
using UnityEngine.InputSystem;

public class LanternKB : MonoBehaviour
{
    private PlayerKB_GrabObject _grabScript;
    private int _isGrabbed = 0;
    [SerializeField] public bool _isOn;
    [SerializeField] public GameObject _diode;

    [SerializeField] private HintManager _hud;
    private bool _doOnce = true;

    // --- Fields ---
    private Player _player;
    private PlayerControls _controls;

    private void Start()
    {
        _grabScript = GetComponent<PlayerKB_GrabObject>();

        _player = FindFirstObjectByType<Player>();
        _controls = _player.Controls;

        _controls.Viva.Interact.performed += ToggleLantern;
    }

    private void Update()
    {
        _isGrabbed = _grabScript.GetIsGrabbed();

        if (_isGrabbed != 0)
        {
            if (_doOnce)
            {
                _hud.CreateHint(HintConstants.LanternHint);
                _doOnce = false;
            }
        }
        else
        {
            _hud.ClearHint(HintConstants.LanternHint);
            _doOnce = true;
        }
    }

    private void ToggleLantern(InputAction.CallbackContext context)
    {
        if (_isGrabbed == 0) return;

        _diode.SetActive(!_isOn);
        _isOn = !_isOn;
    }
}