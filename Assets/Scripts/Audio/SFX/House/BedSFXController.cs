using UnityEngine;

public class BedSFXController : MonoBehaviour
{
    private AudioSource _jumpSFX;

    private void Awake()
    {
        _jumpSFX = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TagConstants.MainPlayer)) { _jumpSFX.Play(); }
    }
}
