using UnityEngine;


namespace Viva
{


    public class ShoulderState : OccupyState
    {

        private RootAnimationOffset wearOffset;
        private IKAnimationTarget iKAnimation;
        private Companion.ArmIK armOverrideIK;
        private float ikBusyBlendPenalty = 0.0f;
        private Companion.ArmIK.RetargetingInfo bagRetargeting = new Companion.ArmIK.RetargetingInfo();

        public void Pickup(
                    Item item,
                    OnNewItemCallback onItemChangeCallback,
                    HoldType _holdtype,
                    RootAnimationOffset newWearOffset,
                    IKAnimationTarget newIKAnimation,
                    float blendDuration)
        {

            var comp = owner as Companion;
            BeginRigidBodyGrab(item.rigidBody, comp.rightArmMuscle.rigidbody, false, HoldType.OBJECT, blendDuration);
            Pickup(item);
            
            // OnPickup(
            // 	item,
            // 	_holdtype,
            // 	blendDuration
            // );GrabItemRigidBody

            wearOffset = newWearOffset;
            iKAnimation = newIKAnimation;
        }

        public static ShoulderState AddAndAwakeShoulderState(GameObject target, Occupation _occupation, Character newOwner)
        {
            bool oldState = target.activeSelf;
            target.SetActive(false);
            ShoulderState shoulderState = target.AddComponent<ShoulderState>();

            shoulderState.m_occupation = _occupation;
            shoulderState.m_owner = newOwner;

            target.SetActive(oldState); //call Awake if enabled
            return shoulderState;
        }

        protected override void GetRigidBodyBlendConnectedAnchor(out Vector3 targetLocalPos, out Quaternion targetLocalRot)
        {
            targetLocalPos = Vector3.zero;
            targetLocalRot = Quaternion.identity;
        }

        protected override void OnPostPickupItem()
        {
            heldItem.gameObject.layer = WorldUtil.itemDetectorLayer;
        }

        protected override void OnPreDropItem()
        {
            wearOffset = null;
            iKAnimation = null;
            heldItem.gameObject.layer = WorldUtil.itemsLayer;
        }

        // protected override void OnUpdateHold(){
        // 	if( heldItem == null ){
        // 		return;
        // 	}

        // Vector3 restPosition = Vector3.up*wearOffset.position.z;
        // restPosition += transform.InverseTransformDirection( Vector3.up )*wearOffset.position.y;
        // Vector3 currShoulderRestRotation = wearOffset.eulerRotation;
        // if( rightSide ){
        // 	currShoulderRestRotation.x *= -1.0f;
        // 	currShoulderRestRotation.x += 180.0f;

        // 	currShoulderRestRotation.z = -90.0f+( -90.0f-currShoulderRestRotation.z );
        // }
        // Quaternion restRotation = Quaternion.LookRotation( Vector3.down, transform.up )*Quaternion.Euler( currShoulderRestRotation );
        // itemTransformBlend.Blend( restPosition, restRotation );	

        // 	AnimateArmOverride( owner as Companion );	
        // }

        private void AnimateArmOverride(Companion companion)
        {
            CompanionHandState handState;
            if (rightSide)
            {
                handState = companion.rightCompanionHandState;
            }
            else
            {
                handState = companion.leftCompanionHandState;
            }

            // if( handState.holdType != HoldType.FREE ){
            // 	ikBusyBlendPenalty = Mathf.Clamp01( ikBusyBlendPenalty+Time.deltaTime*3.0f );
            // }else{
            // 	ikBusyBlendPenalty = Mathf.Clamp01( ikBusyBlendPenalty-Time.deltaTime*3.0f );
            // }
            // float shoulderArmBlend = Mathf.Clamp01( blendProgress-handState.overrideRetargeting.blend.value-ikBusyBlendPenalty );
            // if( shoulderArmBlend == 0.0f ){
            // 	return;
            // }

            // ///TODO: Apply animation override using new system

            // if( armOverrideIK == null ){
            // 	armOverrideIK = new Companion.ArmIK( handState.armIK );
            // }
            // bagRetargeting.blend.reset( shoulderArmBlend );
            // bagRetargeting.target = heldItem.transform.TransformPoint( FlipXIfRight( iKAnimation.target, rightSide ) );
            // bagRetargeting.pole = heldItem.transform.TransformPoint( FlipXIfRight( iKAnimation.pole, rightSide ) );
            // bagRetargeting.handRotation = heldItem.transform.rotation*Quaternion.Euler( iKAnimation.eulerRotation );
            // armOverrideIK.Apply( bagRetargeting );
        }

        private Vector3 FlipXIfRight(Vector3 a, bool right)
        {
            if (right)
            {
                a.x *= -1.0f;
            }
            return a;
        }
    }

}