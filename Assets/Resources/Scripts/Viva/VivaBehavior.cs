using UnityEngine;

namespace Viva
{
    public abstract class VivaBehavior : MonoBehaviour
    {

        public enum VivaLogType
        {
            AI,
            General,
            Player,
            Serialization
        }

        public void VivaLog(VivaLogType type, string log, string color)
        {
                Debug.LogFormat("<color=white>[{0}]: </color><color={2}>{1}</color>", type, log.ToString().PadRight(10), color);
        }
    }
}
