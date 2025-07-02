using UnityEngine;


namespace Viva
{

    [System.Serializable]
    [CreateAssetMenu(fileName = "CompanionSettings", menuName = "Logic/Companion Settings", order = 1)]
    public class CompanionSettings : ScriptableObject
    {

        [SerializeField]
        private SoundSet m_bodyImpactSoftSound;
        public SoundSet bodyImpactSoftSound { get { return m_bodyImpactSoftSound; } }
        [SerializeField]
        private SoundSet m_bodyImpactHardSound;
        public SoundSet bodyImpactHardSound { get { return m_bodyImpactHardSound; } }
        [SerializeField]
        private SoundSet m_getUpSound;
        public SoundSet getUpSound { get { return m_getUpSound; } }
    }

}