using System.Collections;
using UnityEngine;

public class InteractDoorKB : MonoBehaviour
{
    protected Player _player;

    [SerializeField] protected Collider _playerLeftHand;
    [SerializeField] protected Collider _playerRightHand;

    private bool _playerInRange = false;
    private bool _isOpen = false;
    private bool _isMoving = false;

    [SerializeField] Animator _doorAnimator;
    protected Outline _outline;

    [SerializeField] protected HintManager _canvas;
    private bool _doOnce = true;

    private AudioSource _audioSource;

    protected virtual void Start()
    {
        _player = FindFirstObjectByType<Player>();
        _outline = GetComponent<Outline>();
        AssignInputs();

        _audioSource = GetComponentInChildren<AudioSource>();
    }

    protected virtual void AssignInputs()
    {
        _player.Controls.Viva.LeftGrab.performed += context => interactDoor();
        _player.Controls.Viva.RightGrab.performed += context => interactDoor();
    }

    protected void interactDoor()
    {
        if (_playerInRange && !_isMoving)
        {
            _isMoving = true;
            if (!_isOpen)
            {
                _doorAnimator.Play("Opening");
            }
            else
            {
                _doorAnimator.Play("Closing");
            }

            StartCoroutine(SetIsMoving());
            if (!_audioSource.isPlaying) { _audioSource.Play(); }
            _isOpen = !_isOpen;
        }
    }

    private IEnumerator SetIsMoving()
    {
        yield return new WaitForSeconds(1f);
        _isMoving = false;
    }

    protected virtual void OnTriggerEnter(Collider collider)
    {
        if (collider == _playerLeftHand || collider == _playerRightHand)
        {
            _playerInRange = true;
            _outline.enabled = true;
            if (_doOnce)
            {
                _canvas.CreateHint(HintConstants.InteractHint);
                _doOnce = false;
            }
        }
    }

    protected virtual void OnTriggerExit(Collider collider)
    {
        if (collider == _playerLeftHand || collider == _playerRightHand)
        {
            _playerInRange = false;
            _outline.enabled = false ;
            _canvas.ClearHint(HintConstants.InteractHint);
            _doOnce = true;
        }
    }
}
