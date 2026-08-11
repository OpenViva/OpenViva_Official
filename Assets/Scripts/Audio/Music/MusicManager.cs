using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private AudioSource _audioSource;
    [SerializeField] private GameObject _dayTriggers;
    [SerializeField] private GameObject _nightTriggers;
    public Collider TriggerLastEntered;

    private float _transitionTimer;
    [SerializeField] private float _transitionDuration = 5f;
    private float _maxVolume;
    private bool _fadedOut = false;
    private AudioClip _nextTrack = null;

    private bool _isDay = true;
    public bool IsDay 
    { 
        get { return _isDay; }
        set
        {
            if (_isDay == value) { return; }
            _isDay = value;
            SwitchTriggers();
        }
    }

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _maxVolume = _audioSource.volume;
        IsDay = true;
    }

    private void Update()
    {
        if (_transitionTimer <= 0 && _fadedOut)
        {
            _fadedOut = false;
            _audioSource.volume = _maxVolume;
        }

        if (_transitionTimer <= 0) { return; }

        PerformFade();
    }

    private void SwitchTriggers()
    {
        if (_isDay)
        {
            _nightTriggers.SetActive(false);
            _dayTriggers.SetActive(true);
        }
        else
        {
            _dayTriggers.SetActive(false);
            _nightTriggers.SetActive(true);
        }
    }

    public IEnumerator SwitchToTrack(AudioClip track, Collider triggerEntered, float maxVolume)
    {
        _nextTrack = track;
        if (_audioSource.clip == track) { yield break; }

        yield return new WaitForSeconds(_transitionTimer);

        _transitionTimer = _transitionDuration; 
        TriggerLastEntered = triggerEntered;
        _maxVolume = maxVolume;
    }

    private void PerformFade()
    {
        _transitionTimer -= Time.deltaTime;

        // Fade out for 40% of transition duration
        if (!_fadedOut && _transitionTimer >= _transitionDuration * 0.6f)
        {
            float volume = _maxVolume - ((_transitionDuration - _transitionTimer) / (_transitionDuration - (_transitionDuration - (_transitionDuration * 0.4f))) * _maxVolume);
            if (volume > _audioSource.volume) { return; }
            _audioSource.volume = volume;
        }

        // After fade out, switch tracks and wait for 20% of transition duration
        if (!_fadedOut && _transitionTimer < _transitionDuration * 0.6)
        {
            if (_nextTrack == null)
            {
                Debug.Log("Next music track was not loaded correctly.");
                return;
            }
            _fadedOut = true;
            _audioSource.volume = 0f;
            _audioSource.Stop();
            _audioSource.clip = _nextTrack;
        }

        // Fade in for 40% of transition duration.
        if (_fadedOut && _transitionTimer <= _transitionDuration * 0.4f)
        {
            if (!_audioSource.isPlaying) { _audioSource.Play(); }
            float volume = _maxVolume - (_transitionTimer / (_transitionDuration * 0.4f) * _maxVolume);
            _audioSource.volume = volume;
        }
    }
}
