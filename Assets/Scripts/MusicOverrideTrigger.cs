using UnityEngine;


namespace Viva
{

    public class MusicOverrideTrigger : MonoBehaviour
    {
        private int counter = 0;

        [SerializeField]
        private GameDirector.Music musicToOverride;

        public void OnTriggerEnter(Collider collider)
        {
            Player player = collider.GetComponent<Player>();
            if (!player) return;

            if (counter == 0)
            {
                GameDirector.instance.SetOverrideMusic(musicToOverride);
            }
            counter++;
        }

        public void OnTriggerExit(Collider collider)
        {
            Player player = collider.GetComponent<Player>();
            if (!player) return;

            counter--;
            if (counter == 0)
            {
                GameDirector.instance.SetOverrideMusic(null);
            }
        }
    }

}
