using UnityEngine;
using System.Collections.Generic;

public class FootstepManager : MonoBehaviour
{
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

    [SerializeField] private AudioSource _leftFoot;
    [SerializeField] private AudioSource _rightFoot;

    private CharacterController _playerCC;
    private PlayerKB_Movement _playerMovement;

    private void Awake()
    {
        _playerCC = GetComponentInParent<CharacterController>();
        _playerMovement = GetComponentInParent<PlayerKB_Movement>();
    }

    private void Update()
    {
        if (_playerCC.velocity.magnitude < 0.5f || !_playerMovement.GetIsGrouned()) { return; }

        DetermineFloorType();
        _timeSinceLastStep += Time.deltaTime;
        TryStep();
    }

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
            foreach (FloorType fT in foundFloors) { if (fT.Priority < current.Priority) { current = fT; }}
            _standingOn = current.Type;
        }
    }

    private void TryStep()
    {
        _timeSinceLastStep += Time.deltaTime;
        float playerSpeed = _playerCC.velocity.magnitude;
        float stepFrequency = Mathf.Pow(Mathf.Sin(60 * Mathf.Deg2Rad), playerSpeed) * 2;
        if (_timeSinceLastStep < stepFrequency) { return; }

        if (_leftFootForward)
        {
            _leftFoot.clip = SelectTrack();
            _leftFoot.Play();
            _leftFootForward = false;
        }
        else
        {
            _rightFoot.clip = SelectTrack();
            _rightFoot.Play();
            _leftFootForward = true;
        }

        _timeSinceLastStep = 0f;

        AudioClip SelectTrack()
        {
            AudioClip selected = null;
            int triesLeft = 100;

            switch (_standingOn)
            {
                case FloorTypes.Carpet:

                    while (selected == null && triesLeft > 0)
                    {
                        int randomNumber = Random.Range(0, _carpetClips.Count);
                        if (_carpetClips[randomNumber] != _lastPlayedClip)
                        {
                            selected = _carpetClips[randomNumber];
                        }
                        triesLeft--;
                    }
                    break;

                case FloorTypes.Dirt:

                    while (selected == null && triesLeft > 0)
                    {
                        int randomNumber = Random.Range(0, _dirtClips.Count);
                        if (_dirtClips[randomNumber] != _lastPlayedClip)
                        {
                            selected = _dirtClips[randomNumber];
                        }
                        triesLeft--;
                    }
                    break;

                case FloorTypes.Tile:

                    while (selected == null && triesLeft > 0)
                    {
                        int randomNumber = Random.Range(0, _tileClips.Count);
                        if (_tileClips[randomNumber] != _lastPlayedClip)
                        {
                            selected = _tileClips[randomNumber];
                        }
                        triesLeft--;
                    }
                    break;

                case FloorTypes.Wood:

                    while (selected == null && triesLeft > 0)
                    {
                        int randomNumber = Random.Range(0, _woodClips.Count);
                        if (_woodClips[randomNumber] != _lastPlayedClip)
                        {
                            selected = _woodClips[randomNumber];
                        }
                        triesLeft--;
                    }
                    break;

                case FloorTypes.Stone:

                    while (selected == null && triesLeft > 0)
                    {
                        int randomNumber = Random.Range(0, _stoneClips.Count);
                        if (_stoneClips[randomNumber] != _lastPlayedClip)
                        {
                            selected = _stoneClips[randomNumber];
                        }
                        triesLeft--;
                    }
                    break;

                case FloorTypes.WetStone:

                    while (selected == null && triesLeft > 0)
                    {
                        int randomNumber = Random.Range(0, _wetStoneClips.Count);
                        if (_wetStoneClips[randomNumber] != _lastPlayedClip)
                        {
                            selected = _wetStoneClips[randomNumber];
                        }
                        triesLeft--;
                    }
                    break;

                case FloorTypes.Water:

                    while (selected == null && triesLeft > 0)
                    {
                        int randomNumber = Random.Range(0, _waterClips.Count);
                        if (_waterClips[randomNumber] != _lastPlayedClip)
                        {
                            selected = _waterClips[randomNumber];
                        }
                        triesLeft--;
                    }
                    break;
            }

            _lastPlayedClip = selected;
            return selected;
        }
    }

}
