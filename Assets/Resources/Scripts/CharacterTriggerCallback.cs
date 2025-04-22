using UnityEngine;


namespace Viva
{


    public class CharacterTriggerCallback : MonoBehaviour
    {

        public enum Type
        {
            VIEW,
            RIGHT_INDEX_FINGER,
            LEFT_INDEX_FINGER,
            RIGHT_PALM,
            LEFT_PALM,
            ROOT,
            HEAD,
            PLAYER_PROXIMITY
        }

        [SerializeField]
        private Character m_owner;
        public Character owner { get { return m_owner; } }
        [SerializeField]
        private Type m_collisionPart;
        public Type collisionPart { get { return m_collisionPart; } }

        public bool callTriggerStay = true;


        private void OnTriggerEnter(Collider collider)
        {
            owner.OnCharacterTriggerEnter(this, collider);
        }

        private void OnTriggerExit(Collider collider)
        {
            owner.OnCharacterTriggerExit(this, collider);
        }
        private void OnEnable()
        {
            if (callTriggerStay)
            {
                gameObject.AddComponent<CharacterTriggerStayHandler>().Initialize(this);
            }
        }

        private void OnDisable()
        {
            var handler = gameObject.GetComponent<CharacterTriggerStayHandler>();
            if (handler != null)
            {
                Destroy(handler);
            }
        }

        
    }
    
    public class CharacterTriggerStayHandler : MonoBehaviour
    {
        private CharacterTriggerCallback parent;

        public void Initialize(CharacterTriggerCallback parent)
        {
            this.parent = parent;
        }

        private void OnTriggerStay(Collider collider)
        {
            parent.owner.OnCharacterTriggerStay(parent, collider);
        }
    }

}