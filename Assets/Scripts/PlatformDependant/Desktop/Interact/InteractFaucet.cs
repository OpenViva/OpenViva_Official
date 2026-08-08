using UnityEngine;

public class InteractFaucet : MonoBehaviour
{
    private Player _player;

    [SerializeField] private ParticleSystem _tapFX;
    [SerializeField] private AudioSource _tapSFX;

    private bool _playerInRange = false;

    private Outline _outline;

    [SerializeField] private HintManager _hud;

    private void Start()
    {
        _player = FindFirstObjectByType<Player>();
        _outline = GetComponent<Outline>();
        AssignInputs();
    }

    private void AssignInputs()
    {
        _player.Controls.Viva.LeftGrab.performed += context => Interact();
        _player.Controls.Viva.RightGrab.performed += context => Interact();
    }

    private void Interact()
    {
        if (!_playerInRange) { return; }

        if (_tapFX.isStopped)
        {
            _tapFX.Play();
            _tapSFX.Play();
        }
        else
        {
            _tapFX.Stop();
            _tapSFX.Stop();
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag(TagConstants.Player))
        {
            _playerInRange = true;
            _outline.enabled = true;
            _hud.CreateHint(HintConstants.InteractHint);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.CompareTag(TagConstants.Player))
        {
            _playerInRange = false;
            _outline.enabled = false;
            _hud.ClearHint(HintConstants.InteractHint);
        }
    }
}
