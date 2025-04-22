using UnityEngine;
using Viva.Util;


namespace Viva
{


    public partial class Companion : Character
    {

        public enum TransformOffsetAnimation
        {
            HORSE_MOUNT_RIGHT,
            HORSE_MOUNT_LEFT,
            HORSE_IDLE
        }

        [SerializeField]
        public RootAnimationOffset[] rootAnimationOffsets = new RootAnimationOffset[System.Enum.GetValues(typeof(TransformOffsetAnimation)).Length];


        public delegate void AnchorFinishCallback();

        public int faceYawDisableSum { get; private set; } = 0;
        private bool currentAnimFaceYawToggle = false;
        private bool currentAnimAwarenessModeBinded = false;
        private Quaternion cachedAnchorRotation;
        private Vector3 cachedAnchorPosition;
        private Vector3 anchorPosition;
        private Quaternion anchorRotation;
        private Tools.EaseBlend anchorBlend = new Tools.EaseBlend();
        private AnchorFinishCallback onAnchorFinish;
        public bool anchorActive { get; private set; } = false;
        private bool stopRootAnchorAfterFinished = false;
        private Quaternion spineAnchorRotation;
        private Vector3 spineAnchorPosition;
        private Transform spineAnchorReference;
        private bool? enteredTransition = null;
        private bool canExitSpineAnchor = false;


        private void ModifyAnchorAnimation()
        {
            if (!hasBalance)
            {
                return;
            }

            anchorBlend.Update(Time.deltaTime);
            anchor.localRotation = Quaternion.LerpUnclamped(
                cachedAnchorRotation,
                anchorRotation,
                anchorBlend.value
            );
            anchor.localPosition = Vector3.LerpUnclamped(
                cachedAnchorPosition,
                anchorPosition,
                anchorBlend.value
            );
        }

        public void ApplyDisableFaceYaw(ref bool source)
        {
            if (!source)
            {
                source = true;
                faceYawDisableSum++;
            }
        }
        public void RemoveDisableFaceYaw(ref bool source)
        {
            if (source)
            {
                source = false;
                faceYawDisableSum--;
            }
        }
        public bool IsFaceYawAnimationEnabled()
        {
            return faceYawDisableSum <= 0;
        }

        public void AnchorSpineUntilTransitionEnds(Transform reference)
        {

            if (spineAnchorReference == null)
            {
                enteredTransition = null;
                canExitSpineAnchor = false;
                onModifyAnimations += AnchorSpineTransition;

                spineAnchorReference = reference;

                spineAnchorPosition = spineAnchorReference.InverseTransformPoint(spine1.position);
                spineAnchorRotation = Quaternion.Inverse(spineAnchorReference.rotation) * spine1.rotation;
            }
        }

        public void StopAnchorSpineTransition()
        {
            spineAnchorReference = null;
        }

        private void AnchorSpineTransition()
        {
            bool inTransition = animator.IsInTransition(1);
            if (inTransition)
            {
                if (enteredTransition == null)
                {
                    enteredTransition = true;
                }
            }
            else if (enteredTransition.HasValue && enteredTransition.Value == true)
            {
                canExitSpineAnchor = true;
            }
            if (canExitSpineAnchor || spineAnchorReference == null)
            {
                onModifyAnimations -= AnchorSpineTransition;
                // onModifyAnimations += ResetAnchorRotationAfter;
            }
            else
            {
                Vector3 oldSpine1Pos = spine1.position;
                Quaternion oldSpine1Rot = spine1.rotation;

                spine1.position = spineAnchorReference.TransformPoint(spineAnchorPosition);
                spine1.rotation = spineAnchorReference.rotation * spineAnchorRotation;

                Vector3 finalSpine1Pos = spine1.position;
                Quaternion finalSpine1Rot = spine1.rotation;

                Quaternion diff = finalSpine1Rot * Quaternion.Inverse(oldSpine1Rot);
                anchor.rotation = diff * spineAnchorReference.rotation;
                anchor.position += finalSpine1Pos - oldSpine1Pos;


                spine1.position = finalSpine1Pos;
                spine1.rotation = finalSpine1Rot;
            }
        }

        public void BeginAnchorTransformAnimation(
                Vector3 newAnchorPosition,
                Quaternion newAnchorRotation,
                float transitionLength,
                AnchorFinishCallback _onAnchorFinish = null,
                bool _stopRootAnchorAfterFinished = true
                )
        {
            cachedAnchorPosition = anchor.localPosition;
            cachedAnchorRotation = anchor.localRotation;
            anchorPosition = newAnchorPosition;
            anchorRotation = newAnchorRotation;
            onAnchorFinish = _onAnchorFinish;
            stopRootAnchorAfterFinished = _stopRootAnchorAfterFinished;

            if (!anchorActive)
            {
                AddModifyAnimationCallback(ModifyAnchorAnimation);
            }

            anchorActive = true;
            anchorBlend.reset(0.0f);
            anchorBlend.StartBlend(1.0f, transitionLength);
        }

        public void StopActiveAnchor()
        {
            if (anchorActive)
            {
                RemoveModifyAnimationCallback(ModifyAnchorAnimation);
                anchorActive = false;
                onModifyAnimations += ResetAnchorRotationAfter;
            }
        }
        
        Vector3 rootDirEuler = Vector3.zero;
        float faceYawVelocity = 0.0f;
        
        public void SetFacingRootTarget(Vector3 facingTarget, float speedMultiplier = 1.0f, float minSuccessBearing = 10.0f)
        {
            
            float faceYawAcc = 20.0f * speedMultiplier;
            float faceYawMaxVel = 200 * speedMultiplier;
            
            
            rootDirEuler.y = anchor.eulerAngles.y;

            
            Vector3 readPos = ConstrainFromFloorPos(facingTarget);
            Debug.DrawLine(floorPos, readPos, Color.green, 0.1f);
            Debug.DrawLine(anchor.position, anchor.position + anchor.forward, Color.red, 0.1f);
            float bearing = Tools.Bearing(anchor, readPos);
            if (faceYawDisableSum > 0)
            {   //disable if sum is greater than 1
                faceYawVelocity *= Mathf.Pow(0.8f, animationDelta * 10.0f);
            }
            else
            {
                faceYawVelocity += Mathf.Sign(bearing) * faceYawAcc * animationDelta;

                float absBearing = Mathf.Abs(bearing);
                float absMaxVelocity = faceYawMaxVel * animationDelta;
                if (absBearing < 25)
                {
                    float ratio = 1.0f - absBearing / 25;
                    absMaxVelocity *= 1.0f - ratio * ratio;
                }
                faceYawVelocity = Mathf.Clamp(faceYawVelocity, -absMaxVelocity, absMaxVelocity);
            }
            if (bearing > 0.0f)
            {
                if (bearing + faceYawVelocity <= 0.0f)
                {
                    faceYawVelocity = bearing;
                }
            }
            else if (bearing + faceYawVelocity >= 0.0f)
            {
                faceYawVelocity = bearing;
            }
            rootDirEuler.y = anchor.eulerAngles.y + faceYawVelocity;
            anchor.eulerAngles = rootDirEuler;
//            Debug.Log(anchor.eulerAngles.ToString());
        }
    }
}