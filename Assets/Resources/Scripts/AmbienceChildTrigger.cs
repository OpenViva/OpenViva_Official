using UnityEngine;

namespace Viva
{
    public class AmbienceChildTrigger : MonoBehaviour
    {
        private GlobalAmbienceVolume parentVolume;

        private void Start()
        {
            parentVolume = GetComponentInParent<GlobalAmbienceVolume>();
        }

        private void OnTriggerEnter(Collider other)
        {
            parentVolume.OnTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            parentVolume.OnTriggerExit(other);
        }
    }
}