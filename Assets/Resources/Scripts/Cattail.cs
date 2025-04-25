using UnityEngine;

namespace Viva
{


    public class Cattail : Item
    {

        [SerializeField]
        private SoundSet smackFX;

        public void PlaySmackSound()
        {
            SoundManager.main.RequestHandle(transform.position, transform).PlayOneShot(smackFX.GetRandomAudioClip());
        }
    }

}