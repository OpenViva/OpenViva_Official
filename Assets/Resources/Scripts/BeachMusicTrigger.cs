using UnityEngine;


namespace Viva
{

    public class BeachMusicTrigger : MonoBehaviour
    {
        private static int beachCounter = 0;

        public void OnTriggerEnter(Collider collider)
        {
            Player player = collider.GetComponent<Player>();
            if (!player) return;

            if (beachCounter == 0)
            {
                GameDirector.instance.SetUserIsInBeach(true);
            }
            beachCounter++;
        }

        public void OnTriggerExit(Collider collider)
        {
            Player player = collider.GetComponent<Player>();
            if (!player) return;

            beachCounter--;
            if (beachCounter == 0)
            {
                GameDirector.instance.SetUserIsInBeach(false);
            }
        }
    }

}