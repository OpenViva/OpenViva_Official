using UnityEngine;


namespace Viva
{

    public class IndoorMusicTrigger : MonoBehaviour
    {
        private static int indoorCounter = 0;

        public void OnTriggerEnter(Collider collider)
        {
            Player player = collider.GetComponent<Player>();
            if (!player) return;

            if (indoorCounter == 0)
            {
                GameDirector.instance.SetUserIsIndoors(true);
            }
            indoorCounter++;
        }

        public void OnTriggerExit(Collider collider)
        {
            Player player = collider.GetComponent<Player>();
            if (!player) return;

            indoorCounter--;
            if (indoorCounter == 0)
            {
                GameDirector.instance.SetUserIsIndoors(false);
            }
        }
    }

}