using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PlayerVR_GrabObject : XRGrabInteractable
{
    [SerializeField] private Transform _leftAttach;
    [SerializeField] private Transform _rightAttach;

    private bool _isGrabbed = false;

    [SerializeField] private HintManager _floatingHint;

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        var interactor = args.interactorObject.transform;
        string name = interactor.name;

        if (name.Contains("Left"))
        {
            attachTransform = _leftAttach;
        }
        else if (name.Contains("Right"))
        {
            attachTransform = _rightAttach;
        }

        FloatingCanvas.Instance.WarpToOrigin();

        base.OnSelectEntering(args);
        _isGrabbed = true;
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        _isGrabbed = false;
        _floatingHint.ClearHint(HintConstants.LeftGrabHintVR);
        _floatingHint.ClearHint(HintConstants.RightGrabHintVR);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!collider.CompareTag("Player") || _isGrabbed) { return; }

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
}
