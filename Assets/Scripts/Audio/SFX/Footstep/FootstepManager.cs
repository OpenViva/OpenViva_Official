using UnityEngine;
using System.Collections.Generic;

public class FootstepManager : MonoBehaviour
{
    public enum FloorType
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

    [SerializeField] private List<AudioClip> _carpetClips;
    [SerializeField] private List<AudioClip> _dirtClips;
    [SerializeField] private List<AudioClip> _tileClips;
    [SerializeField] private List<AudioClip> _woodClips;
    [SerializeField] private List<AudioClip> _stoneClips;
    [SerializeField] private List<AudioClip> _wetStoneClips;
    [SerializeField] private List<AudioClip> _waterClips;

    private AudioClip _lastPlayedClip = null;

    [SerializeField] private AudioSource _leftFoot;
    [SerializeField] private AudioSource _rightFoot;

    private CharacterController _playerCC;
    private bool _playerIsRunning = false;

    private void Awake()
    {
        _playerCC = GetComponentInParent<CharacterController>();
    }

    private void Update()
    {
        if (_playerCC.velocity.magnitude < 0.5f) { return; }
        DetermineFloorType();
    }

    private void SetPlayerIsRunning(bool set) { _playerIsRunning = set; }

    private void DetermineFloorType()
    {
        Collider[] collidersInSphere = Physics.OverlapSphere(transform.position, 0.1f);
    }
}
