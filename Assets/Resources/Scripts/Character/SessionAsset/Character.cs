using UnityEngine;


namespace Viva
{

    public delegate void OnGenericCallback();
    public delegate void OnGestureCallback();


    public abstract partial class Character : VivaSessionAsset
    {

        public enum Type
        {
            COMPANION = 1,
            PLAYER = 2
        }
        
        [VivaFileAttribute]
        public string characterName = "Character";
        [SerializeField]
        private Type m_characterType;
        public Type characterType { get { return m_characterType; } private set { m_characterType = value; } }
        [SerializeField]
        private Item m_headItem;
        public Item headItem { get { return m_headItem; } }
        [SerializeField]
        private Rigidbody m_velTracker;
        public Rigidbody velTracker { get { return m_velTracker; } }
        public Terrain groundTerrain;
        [SerializeField]
        private Transform m_head;
        public Transform head { get { return m_head; } }
        [SerializeField]
        private OccupyState[] m_occupyStates = new OccupyState[System.Enum.GetValues(typeof(Occupation)).Length];
        public OccupyState[] occupyStates { get { return m_occupyStates; } }

        public HeadState headState { get { return occupyStates[(int)Occupation.HEAD] as HeadState; } }
        public HandState rightHandState { get { return occupyStates[(int)Occupation.HAND_RIGHT] as HandState; } }
        public HandState leftHandState { get { return occupyStates[(int)Occupation.HAND_LEFT] as HandState; } }
        public ShoulderState rightShoulderState { get { return occupyStates[(int)Occupation.SHOULDER_RIGHT] as ShoulderState; } }
        public ShoulderState leftShoulderState { get { return occupyStates[(int)Occupation.SHOULDER_LEFT] as ShoulderState; } }
        
        public OnGenericCallback onModifyAnimations = null;

        public void AddModifyAnimationCallback(OnGenericCallback callback)
        {
            onModifyAnimations -= callback;
            onModifyAnimations += callback;
        }

        public void RemoveModifyAnimationCallback(OnGenericCallback callback)
        {
            onModifyAnimations -= callback;
        }

        //Find HandState by currently held Item
        public OccupyState FindOccupyStateByHeldItem(Item heldItem)
        {
            for (int i = 0; i < occupyStates.Length; i++)
            {
                var occupyState = occupyStates[i];
                if (occupyState == null)
                {
                    continue;
                }
                if (occupyState.heldItem == heldItem)
                {
                    return occupyState;
                }
            }
            return null;
        }

        //Find HandState by hand Item itself
        public OccupyState FindHandStateBySelfItem(Item selfItem)
        {
            for (int i = (int)Occupation.HAND_RIGHT; i <= (int)Occupation.HAND_LEFT; i++)
            {
                var handState = occupyStates[i] as HandState;
                if (handState == null)
                {
                    continue;
                }
                if (handState.selfItem != selfItem)
                {
                    return handState;
                }
            }
            return null;
        }
        //Find Opposite HandState by Hand Item itself
        public HandState FindOppositeHandStateByOccupyState(OccupyState occState)
        {
            bool isRightHand = occState.rightSide;
            int oppositeIndex = isRightHand ? (int)Occupation.HAND_LEFT : (int)Occupation.HAND_RIGHT;

            return occupyStates[oppositeIndex] as HandState;
        }
        
        public T GetItemIfHeldByEitherHand<T>() where T : Item
        {
            T item = rightHandState.GetItemIfHeld<T>();
            if (item == null)
            {
                item = leftHandState.GetItemIfHeld<T>();
            }
            return item;
        }

        protected abstract Vector3 CalculateFloorPosition();
        public Vector3 floorPos { get { return CalculateFloorPosition(); } }
        public abstract bool IsSittingOnFloor();

        protected abstract void OnCharacterAwake();
        public abstract void OnCharacterFixedUpdate();
        public abstract void OnCharacterUpdate();
        public abstract void OnCharacterLateUpdatePostIK();
        public abstract void OnCharacterSplashed(Vector3 sourcePos);

        public virtual void OnCharacterCollisionEnter(CharacterCollisionCallback ccc, Collision collision)
        {
        }
        public virtual void OnCharacterCollisionStay(CharacterCollisionCallback ccc, Collision collision)
        {
        }
        public virtual void OnCharacterCollisionExit(CharacterCollisionCallback ccc, Collision collision)
        {
        }
        public virtual void OnCharacterTriggerEnter(CharacterTriggerCallback ccc, Collider collider)
        {
        }
        public virtual void OnCharacterTriggerStay(CharacterTriggerCallback ccc, Collider collider)
        {
        }
        public virtual void OnCharacterTriggerExit(CharacterTriggerCallback ccc, Collider collider)
        {
        }
        protected override void OnAwake()
        {
            InitFootstepSounds();
            OnCharacterAwake();
        }
        
        public Vector3 ConstrainFromFloorPos(Vector3 p)
        {
            Vector3 diff = p - floorPos;
            diff.y = 0.0f;
            return floorPos + diff.normalized;
        }

  
    }

}