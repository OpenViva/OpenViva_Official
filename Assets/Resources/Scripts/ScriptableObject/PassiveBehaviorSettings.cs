using UnityEngine;


namespace Viva
{


    [System.Serializable]
    [CreateAssetMenu(fileName = "passiveBehaviorSettings", menuName = "Logic/Passive Behavior Settings", order = 1)]
    public class PassiveBehaviorSettings : ScriptableObject
    {
        [Range(0.01f, 0.8f)]
        [SerializeField]
        public float hugPlayerHeadMaxProximityDistance = 0.3f;
        [Range(0.01f, 0.5f)]
        [SerializeField]
        public float hugPlayerHeadMinProximityDistance = 0.3f;
        [SerializeField]
        public float hugPlayerPitchProximityOffset = 0.0f;
        [SerializeField]
        public float hugPlayerRollProximityOffset = 0.0f;
        [SerializeField]
        public PhysicMaterial lolibasePhysicsMaterial;
        [SerializeField]
        public float hugPlayerAnimSideMinDistance = 0.3f;
        [SerializeField]
        public float hugPlayerAnimSideMaxDistance = 0.3f;
        [SerializeField]
        public Texture2D globalDirtTexture = null;
    }

}