using System.Collections;
using UnityEngine;

public class ChurchTimer : MonoBehaviour
{
    public static ChurchTimer Instance { get; private set; }
    private AudioSource _audioSource;
    private const float AUDIO_CLIP_LENGTH = 13.096f;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        _audioSource = GetComponent<AudioSource>();
    }

    public IEnumerator TryRingBells(int value)
    {
        if (value % 3 == 0)
        {
            _audioSource.Play();
            yield return new WaitForSeconds(AUDIO_CLIP_LENGTH);
            _audioSource.Stop();
        }
    }
}
