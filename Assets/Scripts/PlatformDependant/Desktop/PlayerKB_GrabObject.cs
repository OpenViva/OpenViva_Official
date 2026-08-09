#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using System;
using UnityEngine;

// This script allows the player to grab and release objects (KB&M only)

public class PlayerKB_GrabObject : MonoBehaviour
{
    private GameObject _playerLeftHand; // The player's left hand object
    private GameObject _playerRightHand; // The player's right hand object
    protected Collider _playerLeftCollider; // The collider of the player's left hand
    protected Collider _playerRightCollider; // The collider of the player's right hand
    private Rigidbody _grabbableObjectRB; // The Rigidbody of the object to be grabbed

    private bool _playerInRange = false; // Check whether the player is in range to grab the object
    protected bool _isGrabbedInLeft = false; // Check whether the object is grabbed in the left hand
    protected bool _isGrabbedInRight = false; // Check whether the object is grabbed in the right hand
    private bool _didOnce = false; // For crop subclass only

    [SerializeField] private int _objectIndex; // The index of the object in the ObjectHoldPositions script
    public bool IsActive = true; // The item cannot be grabbed if this is false
    private bool _isOpen = false; // If this object is not a bag, this variable is always false

    private Outline _outline;

    protected HintManager _hud;
    protected bool _hintIsShowing = false;

    private AnimationIndexes _animationIndexes;

    public event Action<bool> OnGrabbedLeft;
    public bool IsGrabbedLeft
    {   
        get => _isGrabbedInLeft;
        set
        {
            if (_isGrabbedInLeft == value) return;
            _isGrabbedInLeft = value;
            OnGrabbedLeft?.Invoke(value);
        }
    }

    public event Action<bool> OnGrabbedRight;
    public bool IsGrabbedRight
    {
        get => _isGrabbedInRight;
        set
        {
            if (_isGrabbedInRight == value) { return; }
            _isGrabbedInRight = value;
            OnGrabbedRight?.Invoke(value);
        }
    }

    // --- Fields ---
    protected Player _player;

    protected virtual void Start()
    {
        _player = FindFirstObjectByType<Player>();

        // Find the player's hand objects in the scene
        _playerLeftHand = GameObject.Find("hand_l");
        _playerRightHand = GameObject.Find("hand_r");

        // Set the grabbable object and its Rigidbody
        _grabbableObjectRB = GetComponent<Rigidbody>();

        // Set the colliders of the player's hands
        _playerLeftCollider = _playerLeftHand.GetComponent<Collider>();
        _playerRightCollider = _playerRightHand.GetComponent<Collider>();

        GameObject hud = GameObject.Find("HUD");
        _hud = hud.GetComponent<HintManager>();

        _outline = GetComponent<Outline>();

        _animationIndexes = PlayerManager.Instance.AnimationKB.GetComponent<AnimationIndexes>();

        AssignInputs();
    }

    protected virtual void GrabLeft()
    {
        // If the player is in range and the object is not already grabbed, grab it with the left hand
        if (!IsActive) { return; }

        if (_isOpen || !gameObject.activeSelf) { return; }

        if (_playerInRange && !_isGrabbedInLeft && !_isGrabbedInRight && !PlayerManager.Instance.LeftHandOccupied && _grabbableObjectRB != null)
        {
            _grabbableObjectRB.useGravity = false;
            _grabbableObjectRB.isKinematic = true;
            transform.SetParent(_playerLeftHand.transform);
            transform.SetLocalPositionAndRotation(ObjectHoldPositions.Instance.GetObjectPositionLeft(_objectIndex), ObjectHoldPositions.Instance.GetObjectRotationLeft(_objectIndex));
            IsGrabbedLeft = true;
            if (_outline != null) { _outline.enabled = false; }
            if (!_didOnce) { _didOnce = true; OnGrabbed(); }
            PlayerManager.Instance.LeftHandOccupied = true;

            _hud.ClearHint(HintConstants.GrabHint);
            _hud.CreateHint(HintConstants.LeftReleaseHint);

            _animationIndexes.PlayAnimationLeft(_objectIndex);
        }
        // If the object is already grabbed in the left hand, release it
        else if (_isGrabbedInLeft)
        {
            transform.SetParent(null);
            _grabbableObjectRB.isKinematic = false;
            _grabbableObjectRB.useGravity = true;
            IsGrabbedLeft = false;

            _hud.ClearHint(HintConstants.LeftReleaseHint);
            PlayerManager.Instance.LeftHandOccupied = false;

            _animationIndexes.PlayAnimationLeft(-1);
        }
    }

    protected virtual void GrabRight()
    {
        // If the player is in range and the object is not already grabbed, grab it with the right hand
        if (!IsActive) { return; }

        if (_isOpen || !gameObject.activeSelf) { return; }

        if (_playerInRange && !_isGrabbedInRight && !_isGrabbedInLeft && !PlayerManager.Instance.RightHandOccupied && _grabbableObjectRB != null)
        {
            _grabbableObjectRB.useGravity = false;
            _grabbableObjectRB.isKinematic = true;
            transform.SetParent(_playerRightHand.transform);
            transform.SetLocalPositionAndRotation(ObjectHoldPositions.Instance.GetObjectPositionRight(_objectIndex), ObjectHoldPositions.Instance.GetObjectRotationRight(_objectIndex));
            IsGrabbedRight = true;
            if (_outline != null) { _outline.enabled = false; }
            if (!_didOnce) { _didOnce = true; OnGrabbed(); }
            PlayerManager.Instance.RightHandOccupied = true;

            _hud.ClearHint(HintConstants.GrabHint);
            _hud.CreateHint(HintConstants.RightReleaseHint);

            _animationIndexes.PlayAnimationRight(_objectIndex);
        }
        // If the object is already grabbed in the right hand, release it
        else if (_isGrabbedInRight)
        {
            transform.SetParent(null);
            _grabbableObjectRB.isKinematic = false;
            _grabbableObjectRB.useGravity = true;
            IsGrabbedRight = false;

            _hud.ClearHint(HintConstants.RightReleaseHint);
            PlayerManager.Instance.RightHandOccupied = false;

            _animationIndexes.PlayAnimationRight(-1);
        }
    }

    // TriggerEnetered and TriggerExited may seem unnecessary, but do not change. (Check subclass Crop.cs)
    private void OnTriggerEnter(Collider collider)
    {
        TriggerEntered(collider, HintConstants.GrabHint);
    }

    protected virtual void TriggerEntered(Collider collider, string hint)
    {
        if (collider == _playerLeftCollider || collider == _playerRightCollider)
        {
            _playerInRange = true;
            if (_outline != null && GetIsGrabbed() == 0) { _outline.enabled = true; }
            if (!_hintIsShowing && !_isGrabbedInLeft && !_isGrabbedInRight)
            {
                _hud.CreateHint(hint);
                _hintIsShowing = true;
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        TriggerExited(collider, HintConstants.GrabHint);
    }

    protected virtual void TriggerExited(Collider collider, string hint)
    {
        if (collider == _playerLeftCollider || collider == _playerRightCollider)
        {
            _playerInRange = false;
            if (_outline != null) { _outline.enabled = false; }
            _hud.ClearHint(hint);
            _hintIsShowing = false;
        }
    }

    // Checks if the item is being held and in which hand
    public int GetIsGrabbed()
    {
        if (_isGrabbedInLeft)
        {
            return 1;
        }
        else if (_isGrabbedInRight)
        {
            return 2;
        }
        return 0;
    }

    // When the item is placed in the inventory, set it as inactive and disable physics. Do the inverse when taken out of the inventory
    public void SetIsActive(bool set, int handedness)
    {
        if (_playerLeftHand == null) { Start(); }

        if (set == false)
        {
            transform.SetParent(null, true);

            if (handedness == 1)
            {
                _animationIndexes.PlayAnimationLeft(-1);
                _hud.ClearHint(HintConstants.LeftReleaseHint);
                PlayerManager.Instance.LeftHandOccupied = false;
            }
            else
            {
                _animationIndexes.PlayAnimationRight(-1);
                _hud.ClearHint(HintConstants.RightReleaseHint);
                PlayerManager.Instance.RightHandOccupied = false;
            }
        }
        else
        {
            gameObject.SetActive(true);
            if (handedness == 1)
            {
                transform.SetParent(_playerLeftHand.transform, true);
                transform.SetLocalPositionAndRotation(ObjectHoldPositions.Instance.GetObjectPositionLeft(_objectIndex), ObjectHoldPositions.Instance.GetObjectRotationLeft(_objectIndex));
                _animationIndexes.PlayAnimationLeft(_objectIndex);
                IsGrabbedLeft = true;
                IsGrabbedRight = false;
                PlayerManager.Instance.LeftHandOccupied = true;
            }
            else
            {
                transform.SetParent(_playerRightHand.transform, true);
                transform.SetLocalPositionAndRotation(ObjectHoldPositions.Instance.GetObjectPositionRight(_objectIndex), ObjectHoldPositions.Instance.GetObjectRotationRight(_objectIndex));
                _animationIndexes.PlayAnimationRight(_objectIndex);
                IsGrabbedRight = true;
                IsGrabbedLeft = false;
                PlayerManager.Instance.RightHandOccupied = true;
            }
        }

        _grabbableObjectRB.isKinematic = set;
        _grabbableObjectRB.useGravity = !set;
        IsActive = set;
        if (set == false)
        {
            IsGrabbedLeft = false;
            IsGrabbedRight = false;
            gameObject.SetActive(false);
        }
    }

    // Crop subclass only
    protected virtual void OnGrabbed()
    {
        _outline.enabled = false;
    }

    // If this object is a bag, set whether it is open or closed to enable/disable grabbing
    public void SetIsOpen(bool isOpen)
    {
        _isOpen = isOpen;
    }

    protected virtual void AssignInputs()
    {
        _player.Controls.Viva.LeftGrab.performed += context => GrabLeft();
        _player.Controls.Viva.RightGrab.performed += context => GrabRight();
    }

    private void OnDestroy()
    {
        IsActive = false;
    }
}
#endif