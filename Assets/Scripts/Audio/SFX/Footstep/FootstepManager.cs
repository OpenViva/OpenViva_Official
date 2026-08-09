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

    [SerializeField] private AudioSource _leftFoot;
    [SerializeField] private AudioSource _rightFoot;

    private CharacterController _playerCC;

    private void Awake()
    {
        _playerCC = GetComponentInParent<CharacterController>();
    }

    private void Update()
    {
        if (_playerCC.velocity.magnitude < 0.5f) { return; }
        DetermineFloorType();
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
}
