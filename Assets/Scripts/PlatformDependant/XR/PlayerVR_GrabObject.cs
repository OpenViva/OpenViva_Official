using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PlayerVR_GrabObject : XRGrabInteractable
{
    [SerializeField] private Transform _leftAttach;
    [SerializeField] private Transform _rightAttach;

    private int _isGrabbed = 0;

    private HintManager _hintManager;

    [SerializeField] private int _objectIndex;
    private AnimationIndexes _animationIndexes;

    private void Start()
    {
        _animationIndexes = PlayerManager.Instance._animationVR.GetComponent<AnimationIndexes>();

        GameObject floatingHint = GameObject.Find("FloatingHint");
        _hintManager = floatingHint.GetComponent<HintManager>();
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        var interactor = args.interactorObject.transform;
        string name = interactor.name;

        if (name.Contains("Left"))
        {
            attachTransform = _leftAttach;
            _isGrabbed = 1;
            _animationIndexes.PlayAnimationLeft(_objectIndex);
        }
        else if (name.Contains("Right"))
        {
            attachTransform = _rightAttach;
            _isGrabbed = 2;
            _animationIndexes.PlayAnimationRight(_objectIndex);
        }

        FloatingCanvas.Instance.WarpToOrigin();

        base.OnSelectEntering(args);
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        _hintManager.ClearHint(HintConstants.LeftGrabHintVR);
        _hintManager.ClearHint(HintConstants.RightGrabHintVR);
    }

    protected override void Drop()
    {
        base.Drop();
        _isGrabbed = 0;
        _animationIndexes.PlayAnimationLeft(-1);
        _animationIndexes.PlayAnimationRight(-1);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!collider.CompareTag("Player") || _isGrabbed != 0) { return; }

        if (collider.gameObject.name.Contains("Left"))
        {
            FloatingCanvas.Instance.WarpToObject(transform);
            _hintManager.CreateHint(HintConstants.LeftGrabHintVR);
        }
        else if (collider.gameObject.name.Contains("Right"))
        {
            FloatingCanvas.Instance.WarpToObject(transform);
            _hintManager.CreateHint(HintConstants.RightGrabHintVR);
        }   
    }

    private void OnTriggerExit(Collider collider)
    {
        if (!collider.CompareTag("Player")) { return; }

        FloatingCanvas.Instance.WarpToOrigin();
        _hintManager.ClearHint(HintConstants.LeftGrabHintVR);
        _hintManager.ClearHint(HintConstants.RightGrabHintVR);
    }

    public int GetIsGrabbed()
    {
        return _isGrabbed;
    }
}
