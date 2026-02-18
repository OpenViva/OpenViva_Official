#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;
using UnityEngine.InputSystem;

// This script allows the player to grab and release objects (KB&M only)

public class PlayerKB_GrabObject : MonoBehaviour
{
    private GameObject _playerLeftHand; // The player's left hand object
    private GameObject _playerRightHand; // The player's right hand object
    private Collider _playerLeftCollider; // The collider of the player's left hand
    private Collider _playerRightCollider; // The collider of the player's right hand
    private GameObject _grabbableObject; // The object to be grabbed
    private Rigidbody _grabbableObjectRB; // The Rigidbody of the object to be grabbed

    private bool _playerInRange = false; // Check whether the player is in range to grab the object
    private bool _isGrabbedInLeft = false; // Check whether the object is grabbed in the left hand
    private bool _isGrabbedInRight = false; // Check whether the object is grabbed in the right hand

    private ObjectHoldPositions _holdPositions; // Calls the script that holds the positions and rotations of all grabbable objects
    [SerializeField] private int _objectIndex; // The index of the object in the ObjectHoldPositions script
    private bool _isActive = true; // The item cannot be grabbed if this is false
    private bool _isOpen = false; // If this object is not a bag, this variable is always true

    private Outline _outline;

    private PlayerKB_HUD _hud;
    private bool _hintIsShowing = false;

    [SerializeField] private AnimationIndexes _animationIndexes;

    // --- Fields ---
    private Player _player;

    private void Start()
    {
        _player = FindFirstObjectByType<Player>();

        _holdPositions = new ObjectHoldPositions();

        // Find the player's hand objects in the scene
        _playerLeftHand = GameObject.Find("hand_l");
        _playerRightHand = GameObject.Find("hand_r");

        // Set the grabbable object and its Rigidbody
        _grabbableObject = this.gameObject;
        _grabbableObjectRB = _grabbableObject.GetComponent<Rigidbody>();

        // Set the colliders of the player's hands
        _playerLeftCollider = _playerLeftHand.GetComponent<Collider>();
        _playerRightCollider = _playerRightHand.GetComponent<Collider>();

        GameObject hud = GameObject.Find("HUD");
        _hud = hud.GetComponent<PlayerKB_HUD>();

        _outline = GetComponent<Outline>();

        AssignInputs();
    }

    private void GrabLeft()
    {
        // If the player is in range and the object is not already grabbed, grab it with the left hand
        if (_isActive && !_isOpen)
        {
            if (_playerInRange && !_isGrabbedInLeft && !_isGrabbedInRight)
            {
                _grabbableObjectRB.useGravity = false;
                _grabbableObjectRB.isKinematic = true;
                _grabbableObject.transform.SetParent(_playerLeftHand.transform);
                _grabbableObject.transform.localPosition = _holdPositions.GetObjectPositionLeft(_objectIndex);
                _grabbableObject.transform.localRotation = _holdPositions.GetObjectRotationLeft(_objectIndex);
                _isGrabbedInLeft = true;

                _hud.ClearHint(Hint.HintType.GrabHint);
                _hud.CreateHint(Hint.HintType.LeftReleaseHint);

                _animationIndexes.PlayAnimationLeft(_objectIndex);
            }
            // If the object is already grabbed in the left hand, release it
            else if (_isGrabbedInLeft)
            {
                _grabbableObject.transform.SetParent(null);
                _grabbableObjectRB.isKinematic = false;
                _grabbableObjectRB.useGravity = true;
                _isGrabbedInLeft = false;

                _hud.ClearHint(Hint.HintType.LeftReleaseHint);

                _animationIndexes.PlayAnimationLeft(-1);
            }
        }
    }

    private void GrabRight()
    {
        // If the player is in range and the object is not already grabbed, grab it with the right hand
        if (_isActive && !_isOpen)
        {
            if (_playerInRange && !_isGrabbedInRight && !_isGrabbedInLeft)
            {
                _grabbableObjectRB.useGravity = false;
                _grabbableObjectRB.isKinematic = true;
                _grabbableObject.transform.SetParent(_playerRightHand.transform);
                _grabbableObject.transform.localPosition = _holdPositions.GetObjectPositionRight(_objectIndex);
                _grabbableObject.transform.localRotation = _holdPositions.GetObjectRotationRight(_objectIndex);
                _isGrabbedInRight = true;

                _hud.ClearHint(Hint.HintType.GrabHint);
                _hud.CreateHint(Hint.HintType.RightReleaseHint);

                _animationIndexes.PlayAnimationRight(_objectIndex);
            }
            // If the object is already grabbed in the right hand, release it
            else if (_isGrabbedInRight)
            {
                _grabbableObject.transform.SetParent(null);
                _grabbableObjectRB.isKinematic = false;
                _grabbableObjectRB.useGravity = true;
                _isGrabbedInRight = false;

                _hud.ClearHint(Hint.HintType.RightReleaseHint);

                _animationIndexes.PlayAnimationRight(-1);
            }
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        // Check if the player is in range to grab the bag
        if (collider == _playerLeftCollider || collider == _playerRightCollider)
        {
            _playerInRange = true;
            _outline.enabled = true;
            if (!_hintIsShowing && !_isGrabbedInLeft && !_isGrabbedInRight)
            {
                _hud.CreateHint(Hint.HintType.GrabHint);
                _hintIsShowing = true;
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        // Check if the player is out of range to grab the bag
        if (collider == _playerLeftCollider || collider == _playerRightCollider)
        {
           _playerInRange = false;
           _outline.enabled = false;
           _hud.ClearHint(Hint.HintType.GrabHint);
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
        if (set == false)
        {
            _grabbableObject.transform.SetParent(null);
        }
        else
        {
            gameObject.SetActive(true);
            if (handedness == 1)
            {
                _grabbableObject.transform.SetParent(_playerLeftHand.transform);
                _grabbableObject.transform.localPosition = _holdPositions.GetObjectPositionLeft(_objectIndex);
                _grabbableObject.transform.localRotation = _holdPositions.GetObjectRotationLeft(_objectIndex);
                _isGrabbedInLeft = true;
                _isGrabbedInRight = false;
            }
            else
            {
                _grabbableObject.transform.SetParent(_playerRightHand.transform);
                _grabbableObject.transform.localPosition = _holdPositions.GetObjectPositionRight(_objectIndex);
                _grabbableObject.transform.localRotation = _holdPositions.GetObjectRotationRight(_objectIndex);
                _isGrabbedInRight = true;
                _isGrabbedInLeft = false;
            }
        }

        _grabbableObjectRB.isKinematic = set;
        _grabbableObjectRB.useGravity = !set;
        _isActive = set;
        if (set == false)
        {
            _isGrabbedInLeft = set;
            _isGrabbedInRight = set;
            gameObject.SetActive(false);
        }
    }

    // If this object is a bag, set whether it is open or closed to enable/disable grabbing
    public void SetIsOpen(bool isOpen)
    {
        _isOpen = isOpen;
    }

    private void AssignInputs()
    {
        _player.Controls.Viva.LeftGrab.performed += context => GrabLeft();
        _player.Controls.Viva.RightGrab.performed += context => GrabRight();
    }
}
#endif