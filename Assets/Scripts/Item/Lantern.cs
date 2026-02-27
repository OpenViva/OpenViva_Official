using UnityEngine;
using UnityEngine.InputSystem;

public class Lantern : MonoBehaviour
{
    private PlayerKB_GrabObject _grabScript;
    private int _isGrabbed = 0;
    [SerializeField] private bool _isOn;
    [SerializeField] private GameObject _diode;

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

        _controls.Viva.Interact.performed += Toggle;
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

    private void Toggle(InputAction.CallbackContext context)
    {
        if (_isGrabbed == 0) return;

        _diode.SetActive(!_isOn);
        _isOn = !_isOn;
    }
}