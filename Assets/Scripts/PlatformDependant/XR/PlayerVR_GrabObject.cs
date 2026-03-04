using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PlayerVR_GrabObject : XRGrabInteractable
{
    [SerializeField] private Transform _leftAttach;
    [SerializeField] private Transform _rightAttach;

    public int _isGrabbed = 0;

    [SerializeField] private HintManager _floatingHint;

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        var interactor = args.interactorObject.transform;
        string name = interactor.name;

        if (name.Contains("Left"))
        {
            attachTransform = _leftAttach;
            _isGrabbed = 1;
        }
        else if (name.Contains("Right"))
        {
            attachTransform = _rightAttach;
            _isGrabbed = 2;
        }

        FloatingCanvas.Instance.WarpToOrigin();

        base.OnSelectEntering(args);
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        _floatingHint.ClearHint(HintConstants.LeftGrabHintVR);
        _floatingHint.ClearHint(HintConstants.RightGrabHintVR);
    }

    protected override void Drop()
    {
        base.Drop();
        _isGrabbed = 0;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!collider.CompareTag("Player") || _isGrabbed != 0) { return; }

        if (collider.gameObject.name.Contains("Left"))
        {
            FloatingCanvas.Instance.WarpToObject(transform);
            _floatingHint.CreateHint(HintConstants.LeftGrabHintVR);
        }
        else if (collider.gameObject.name.Contains("Right"))
        {
            FloatingCanvas.Instance.WarpToObject(transform);
            _floatingHint.CreateHint(HintConstants.RightGrabHintVR);
        }   
    }

    private void OnTriggerExit(Collider collider)
    {
        if (!collider.CompareTag("Player")) { return; }

        FloatingCanvas.Instance.WarpToOrigin();
        _floatingHint.ClearHint(HintConstants.LeftGrabHintVR);
        _floatingHint.ClearHint(HintConstants.RightGrabHintVR);
    }

    public int GetIsGrabbed()
    {
        return _isGrabbed;
    }
}
