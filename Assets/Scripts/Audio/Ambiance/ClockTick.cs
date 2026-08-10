using System.Collections;
using UnityEngine;

public class ClockTick : MonoBehaviour
{
    private AudioSource _audioSource;
    [SerializeField] private AudioClip _tick;
    [SerializeField] private AudioClip _tock;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        InvokeRepeating(nameof(StartTicking), 0f, 1f);
    }

    private void StartTicking()
    {
        if (_audioSource.clip != _tick) { _audioSource.clip = _tick; }
        else { _audioSource.clip = _tock; }

        _audioSource.Play();
    }
}
