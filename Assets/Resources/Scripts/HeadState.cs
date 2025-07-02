using System;
using UnityEngine;


namespace Viva
{


    public class HeadState : OccupyState
    {

        [SerializeField]
        private Rigidbody headRigidBody;

        private Vector3 hatPosition;
        
        
        private void Start()
        {
            hatPosition = headRigidBody.gameObject.transform.localPosition;
        }

        public void WearOnHead(Item item, Vector4 hatOffset, float blendDuration)
        {
            if (item.rigidBody == null)
            {
                return;
            }
            
            headRigidBody.gameObject.transform.localPosition = hatOffset;
            BeginRigidBodyGrab(item.rigidBody, headRigidBody, false, HoldType.OBJECT, blendDuration);
            Pickup(item);
        }

        protected override void GetRigidBodyBlendConnectedAnchor(out Vector3 targetLocalPos, out Quaternion targetLocalRot)
        {
            targetLocalPos = Vector3.zero;
            targetLocalRot = Quaternion.identity;
        }

        protected override void OnPostPickupItem()
        {
        }

        protected override void OnPreDropItem()
        {
        }
    }

}