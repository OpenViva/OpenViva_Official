using UnityEngine;

namespace Viva
{


    [System.Serializable]
    [CreateAssetMenu(fileName = "Pose Cache", menuName = "Pose Cache", order = 1)]
    public class PoseCache : ScriptableObject
    {

        public Vector3[] positions;
        public Quaternion[] quaternions;
    }

}