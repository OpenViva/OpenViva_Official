using UnityEngine;

public class OvenSFXController : MonoBehaviour
{
    public static OvenSFXController Instance { get; private set; }

    private AudioSource _audioSource;

    // Clips
    [SerializeField] private AudioClip _burnSFX;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        _audioSource = GetComponentInChildren<AudioSource>();
    }

    public void PlayBurnSFX()
    {
        if (_audioSource.clip != _burnSFX) { _audioSource.clip = _burnSFX; }
        if (!_audioSource.isPlaying) { _audioSource.Play(); }
    }
}
