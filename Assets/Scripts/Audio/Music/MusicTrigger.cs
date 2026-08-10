using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip _track;
    private Collider _thisCollider;
    [SerializeField] private Collider _siblingCollider;

    private void Awake()
    {
        _thisCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        if (MusicManager.Instance.TriggerLastEntered == _siblingCollider)
        {
            StopCoroutine(MusicManager.Instance.SwitchToTrack(_track, _thisCollider));
            StartCoroutine(MusicManager.Instance.SwitchToTrack(_track, _thisCollider));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        StopCoroutine(MusicManager.Instance.SwitchToTrack(_track, _thisCollider));
        StartCoroutine(MusicManager.Instance.SwitchToTrack(_track, _thisCollider));
    }
}
