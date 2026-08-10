using UnityEngine;

public class BookSFXController : MonoBehaviour
{
    private AudioSource _audioSource;
    [SerializeField] private AudioClip _menuOpenSFX;
    [SerializeField] private AudioClip _menuCloseSFX;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void BookOpened()
    {
        if (_audioSource.clip != _menuOpenSFX) { _audioSource.clip = _menuOpenSFX; }
        _audioSource.Play();
    }

    public void BookClosed()
    {
        if (_audioSource.clip != _menuCloseSFX) { _audioSource.clip = _menuCloseSFX; }
        _audioSource.Play();
    }
}
