using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryVR : MonoBehaviour
{
    [SerializeField] private InputActionReference _XRIButtonDownY;
    [SerializeField] private InputActionReference _XRIButtonDownB;

    [SerializeField] private HintManager _controlHintsL;
    [SerializeField] private HintManager _controlHintsR;

    private Animator _animator;
    private bool _isOpen = false;
    private PlayerVR_GrabObject _bagScript;

    private void Start()
    {
        _animator = GetComponentInParent<Animator>();
        _XRIButtonDownY.action.performed += ToggleBag;
        _XRIButtonDownB.action.performed += ToggleBag;
    }

    private void OnEnable()
    {
        _XRIButtonDownB.action.Enable();
        _XRIButtonDownY.action.Enable();
        _bagScript.OnGrabbedStateChanged += ShowHints;
    }

    private void OnDisable()
    {
        _XRIButtonDownB.action.Disable();
        _XRIButtonDownY.action.Disable();
        _bagScript.OnGrabbedStateChanged -= ShowHints;
    }

    private void ToggleBag(InputAction.CallbackContext context)
    {
        
    }

    private void ShowHints(int handedness)
    {
        _controlHintsL.ClearHint(HintConstants.LeftOpenBagHintVR);
        _controlHintsR.ClearHint(HintConstants.RightOpenBagHintVR);
        _controlHintsL.ClearHint(HintConstants.LeftCloseBagHintVR);
        _controlHintsR.ClearHint(HintConstants.RightCloseBagHintVR);

        switch(handedness)
        {
            case 1:
                if (!_isOpen)
                {
                    _controlHintsL.CreateHint(HintConstants.LeftOpenBagHintVR);
                }
                else
                {
                    _controlHintsL.CreateHint(HintConstants.LeftCloseBagHintVR);
                }
                break;
            case 2:
                if (!_isOpen)
                {
                    _controlHintsR.CreateHint(HintConstants.RightOpenBagHintVR);
                }
                else
                {
                    _controlHintsR.CreateHint(HintConstants.RightCloseBagHintVR);
                }
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject @object = other.gameObject;
        if (@object.GetComponent<PlayerVR_GrabObject>().IsGrabbed == 0)
        {
            PlaceInBag(@object);
        }

        if (@object.name == "Controller_BaseLeft")
        {
            PrepareToRemove(0);
        }
        if (@object.name == "Controller_BaseRight")
        {
            PrepareToRemove(1);
        }
    }

    private void PlaceInBag(GameObject item)
    {

    }

    private void PrepareToRemove(int handedness)
    {

    }

}
