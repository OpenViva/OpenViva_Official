using UnityEngine;
using System.Collections.Generic;

public class FootstepManager : MonoBehaviour
{
    #region Class Data
    public enum FloorTypes
    {
        None,
        Carpet,
        Dirt,
        Tile,
        Wood,
        Stone,
        WetStone,
        Water
    }
    private FloorTypes _standingOn = FloorTypes.Wood;

    [SerializeField] private List<AudioClip> _carpetClips;
    [SerializeField] private List<AudioClip> _dirtClips;
    [SerializeField] private List<AudioClip> _tileClips;
    [SerializeField] private List<AudioClip> _woodClips;
    [SerializeField] private List<AudioClip> _stoneClips;
    [SerializeField] private List<AudioClip> _wetStoneClips;
    [SerializeField] private List<AudioClip> _waterClips;

    private AudioClip _lastPlayedClip = null;
    private float _timeSinceLastStep = 0f;
    private bool _leftFootForward = true;
    private bool _wasGrounded = true;

    [SerializeField] private AudioSource _leftFoot;
    [SerializeField] private AudioSource _rightFoot;

    private CharacterController _playerCC;
    private PlayerKB_Movement _playerMovement;
    #endregion

    private void Awake()
    {
        _playerCC = GetComponentInParent<CharacterController>();
        _playerMovement = GetComponentInParent<PlayerKB_Movement>();
    }

    private void Update()
    {
        bool isGrounded = _playerMovement.GetPlayerGrounded();

        if (isGrounded && !_wasGrounded)
        {
            HandleLanding();
        }

        _wasGrounded = isGrounded;

        if (_playerCC.velocity.magnitude < 0.5f || !_playerMovement.GetIsGrouned())
        {
            return;
        }

        _timeSinceLastStep += Time.deltaTime;

        float playerSpeed = _playerCC.velocity.magnitude;
        float stepFrequency = Mathf.Pow(Mathf.Sin(60 * Mathf.Deg2Rad), playerSpeed);

        if (_timeSinceLastStep >= stepFrequency)
        {
            TryStep();
        }
    }

    private void HandleLanding()
    {
        DetermineFloorType();

        _leftFoot.pitch = Random.Range(0.9f, 1.1f);
        _rightFoot.pitch = Random.Range(0.9f, 1.1f);

        _leftFoot.clip = SelectTrack();
        _rightFoot.clip = SelectTrack();

        _leftFoot.Play();
        _rightFoot.Play();

        _timeSinceLastStep = 0;
    }

    private void TryStep()
    {
        DetermineFloorType();

        AudioSource activeFoot = _leftFootForward ? _leftFoot : _rightFoot;

        activeFoot.pitch = Random.Range(0.95f, 1.05f);
        activeFoot.clip = SelectTrack();
        activeFoot.Play();

        _leftFootForward = !_leftFootForward;
        _timeSinceLastStep = 0;
    }

    #region Helper Methods
    private void DetermineFloorType()
    {
        Collider[] collidersInSphere = Physics.OverlapSphere(transform.position, 0.1f, Physics.AllLayers, QueryTriggerInteraction.Collide);

        List<FloorType> foundFloors = new();
        foreach (Collider c in collidersInSphere)
        {
            if (c.TryGetComponent(out FloorType fT)) { foundFloors.Add(fT); }
        }

        if (foundFloors.Count > 0)
        {
            FloorType current = foundFloors[0];
            foreach (FloorType fT in foundFloors)
            {
                if (fT.Priority < current.Priority) { current = fT; }
            }
            _standingOn = current.Type;
        }
    }

    private AudioClip SelectTrack()
    {
        List<AudioClip> currentPool = _standingOn switch
        {
            FloorTypes.Carpet => _carpetClips,
            FloorTypes.Dirt => _dirtClips,
            FloorTypes.Tile => _tileClips,
            FloorTypes.Wood => _woodClips,
            FloorTypes.Stone => _stoneClips,
            FloorTypes.WetStone => _wetStoneClips,
            FloorTypes.Water => _waterClips,
            _ => null
        };

        if (currentPool == null || currentPool.Count == 0)
        {
            return null;
        }

        AudioClip selected = currentPool[Random.Range(0, currentPool.Count)];

        // Prevent the same clip from playing twice
        if (currentPool.Count > 1)
        {
            int triesLeft = 10;
            while (selected == _lastPlayedClip && triesLeft > 0)
            {
                selected = currentPool[Random.Range(0, currentPool.Count)];
                triesLeft--;
            }
        }

        _lastPlayedClip = selected;
        return selected;
    }
    #endregion
}
