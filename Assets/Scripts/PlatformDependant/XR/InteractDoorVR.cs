using UnityEngine;
using UnityEngine.InputSystem;

public class InteractDoorVR : InteractDoorKB
{
    [SerializeField] private InputActionReference _XRILeftSelect;
    [SerializeField] private InputActionReference _XRIRightSelect;

    protected override void Start()
    {
        base.Start();
    }

    protected override void AssignInputs()
    {
        _XRILeftSelect.action.performed += context => interactDoor();
        _XRIRightSelect.action.performed += context => interactDoor();
    }

    protected override void OnTriggerEnter(Collider collider)
    {
        if (!(collider == _playerLeftHand || collider == _playerRightHand)) {  return; }

        base.OnTriggerEnter(collider);
        FloatingCanvas.Instance.WarpToObject(gameObject.GetComponent<Collider>().bounds.center);
    }

    protected override void OnTriggerExit(Collider collider)
    {
        if (!(collider == _playerLeftHand || collider == _playerRightHand)) {  return; }

        base.OnTriggerExit(collider);
        FloatingCanvas.Instance.WarpToOrigin();
    }
}
