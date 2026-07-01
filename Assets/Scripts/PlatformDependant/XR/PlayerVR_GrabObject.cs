using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PlayerVR_GrabObject : XRGrabInteractable
{
    [SerializeField] private Transform _leftAttach;
    [SerializeField] private Transform _rightAttach;

    private int _isGrabbed = 0;
    public event Action<int> OnGrabbedStateChanged;
    public int IsGrabbed
    {
        get => _isGrabbed;
        set
        {
            if (_isGrabbed == value) return;
            _isGrabbed = value;
            OnGrabbedStateChanged?.Invoke(_isGrabbed);
        }
    }

    private HintManager _hintManager;

    public int ObjectIndex;
    private AnimationIndexes _animationIndexes;

    private void Start()
    {
        _animationIndexes = PlayerManager.Instance.AnimationVR.GetComponent<AnimationIndexes>();

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
            IsGrabbed = 1;
            _animationIndexes.PlayAnimationLeft(ObjectIndex);
        }
        else if (name.Contains("Right"))
        {
            attachTransform = _rightAttach;
            IsGrabbed = 2;
            _animationIndexes.PlayAnimationRight(ObjectIndex);
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
        IsGrabbed = 0;
        _animationIndexes.PlayAnimationLeft(-1);
        _animationIndexes.PlayAnimationRight(-1);
        transform.SetParent(null);
        GetComponent<Rigidbody>().isKinematic = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!collider.CompareTag("Player") || IsGrabbed != 0 || Globals.isDesktopMode) { return; }

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
}
