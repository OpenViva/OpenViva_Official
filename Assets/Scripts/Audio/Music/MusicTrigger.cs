using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip _track;
    private Collider _thisCollider;
    [SerializeField] private Collider _siblingCollider;
    private bool _started = false;
    [SerializeField] private float _maxVolume;

    private void Awake()
    {
        _thisCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        if (_started) { OnStartOrEnable(); }
    }

    private void Start()
    {
        _started = true;
        OnStartOrEnable();
    }

    private void OnStartOrEnable()
    {
        if (MusicManager.Instance.TriggerLastEntered == _siblingCollider)
        {
            StopCoroutine(MusicManager.Instance.SwitchToTrack(_track, _thisCollider, _maxVolume));
            StartCoroutine(MusicManager.Instance.SwitchToTrack(_track, _thisCollider, _maxVolume));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(TagConstants.MainPlayer)) { return; }
        StopCoroutine(MusicManager.Instance.SwitchToTrack(_track, _thisCollider, _maxVolume));
        StartCoroutine(MusicManager.Instance.SwitchToTrack(_track, _thisCollider, _maxVolume));
    }
}
