using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PlayerVR_GrabObject : XRGrabInteractable
{
    [SerializeField] private Transform _leftAttach;
    [SerializeField] private Transform _rightAttach;

    private bool _isGrabbed = false;

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

        PlayerVR_HUD.Instance.WarpToOrigin();

        base.OnSelectEntering(args);
        _isGrabbed = true;
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        _isGrabbed = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!collider.CompareTag("Player") || _isGrabbed) { return; }

        if (collider.gameObject.name.Contains("Left"))
        {
            PlayerVR_HUD.Instance.WarpToObject(transform, 0);
        }
        else if (collider.gameObject.name.Contains("Right"))
        {
            PlayerVR_HUD.Instance.WarpToObject(transform, 1);
        }   
    }

    private void OnTriggerExit(Collider collider)
    {
        if (!collider.CompareTag("Player")) { return; }
    
        PlayerVR_HUD.Instance.WarpToOrigin();
    }
}
