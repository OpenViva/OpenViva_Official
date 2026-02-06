using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PlayerVR_GrabObject : XRGrabInteractable
{
    [SerializeField] private Transform _leftAttach;
    [SerializeField] private Transform _rightAttach;

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

        base.OnSelectEntering(args);
    }
}
