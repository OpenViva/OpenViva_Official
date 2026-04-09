using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryKB : MonoBehaviour
{
    private Player _player;
    [SerializeField] private HintManager _hud;
    private PlayerKB_GrabObject _grabScript;
    private Animator _animator;

    private int _grabbedIn;
    private bool _isOpen;

    private void Awake()
    {
        _grabScript = GetComponent<PlayerKB_GrabObject>();
    }

    private void Start()
    {
        _player = FindFirstObjectByType<Player>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        int fromScript = _grabScript.GetIsGrabbed();
        if (_grabbedIn != fromScript)
        {
            SwitchInputs(fromScript);
            ShowHints(fromScript);
            _grabbedIn = fromScript;
        }
    }

    private void SwitchInputs(int handedness)
    {
        if (handedness == 1)
        {
            _player.Controls.Viva.InteractRight.performed -= ToggleBag;
            _player.Controls.Viva.InteractLeft.performed += ToggleBag;
            _player.Controls.Viva.RightGrab.performed -= PlaceInBag;
            _player.Controls.Viva.LeftGrab.performed += PlaceInBag;
        }

        if (handedness == 2)
        {
            _player.Controls.Viva.InteractLeft.performed -= ToggleBag;
            _player.Controls.Viva.InteractRight.performed += ToggleBag;
            _player.Controls.Viva.LeftGrab.performed -= PlaceInBag;
            _player.Controls.Viva.RightGrab.performed += PlaceInBag;
        }
    }

    private void ToggleBag(InputAction.CallbackContext context)
    {
        if (_isOpen)
        {
            _animator.Play("Close");
            _isOpen = false;
        }
        else
        {
            _animator.Play("Open");
            _isOpen = true;
        }
        ShowHints(_grabbedIn);
    }

    private void ShowHints(int handedness)
    {
        _hud.ClearHint(HintConstants.LeftOpenBagHint);
        _hud.ClearHint(HintConstants.RightOpenBagHint);
        _hud.ClearHint(HintConstants.LeftCloseBagHint);
        _hud.ClearHint(HintConstants.RightCloseBagHint);

        switch (handedness)
        {
            case 1:
                if (!_isOpen)
                { _hud.CreateHint(HintConstants.LeftOpenBagHint); }
                else
                { _hud.CreateHint(HintConstants.LeftCloseBagHint); }
                break;
            case 2:
                if (!_isOpen)
                { _hud.CreateHint(HintConstants.RightOpenBagHint); }
                else
                { _hud.CreateHint(HintConstants.RightCloseBagHint); }
                break;
        }
    }

    private void PlaceInBag(InputAction.CallbackContext context)
    {
        
    }
}
