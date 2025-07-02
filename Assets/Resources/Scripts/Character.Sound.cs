using UnityEngine;
using Viva.Util;


namespace Viva
{


    public abstract partial class Character : VivaSessionAsset
    {

        public FootstepInfo footstepInfo;
        
        private void InitFootstepSounds()
        {
            footstepInfo.lastFloorPos = floorPos;
        }

        public void UpdateFootstepCheck()
        {
            //Return if player is on horse
            if (GameDirector.player.controller as HorseControls != null)
            {
                return;
            }
            if (Vector3.SqrMagnitude(floorPos - footstepInfo.lastFloorPos) > 1.0f)
            {

                footstepInfo.lastFloorPos = floorPos;
                PlayFootstep(false);
            }
        }

        protected virtual void OnFootstep() { }


        public void PlayFootstep(bool rightFoot)
        {
            Vector3 positionToPlay;

            Transform playTransform;

            if(characterType == Type.PLAYER)
            {
                var player = this as Player;
                positionToPlay = footstepInfo.transform.localPosition;
                playTransform = footstepInfo.transform;
                //footstep regions take priority
                if (footstepInfo.IsAnyFootstepRegionActive())
                {
                    footstepInfo.SetFootStepTypeFromRegions();
                }
                else if (groundTerrain != null)
                {
                    footstepInfo.SetFootStepTypeBasedOnTerrain(groundTerrain, footstepInfo.transform.position);
                }
                else
                {
                    footstepInfo.SetFootStepTypeBasedOnCollider(player.surfaceCollider);
                }
            }
            else
            {
                var companion = this as Companion;
                if (companion.rightFootRigidBody.transform == null || companion.rightFootSurfaceCollider == null && companion.leftFootSurfaceCollider) return;
                positionToPlay =
                    rightFoot ? companion.rightFootRigidBody.transform.position : companion.rightFootRigidBody.transform.position;
                playTransform = rightFoot ? companion.rightFootRigidBody.transform : companion.rightFootRigidBody.transform;
                //footstep regions take priority
                if (footstepInfo.IsAnyFootstepRegionActive())
                {
                    footstepInfo.SetFootStepTypeFromRegions();
                }
                else
                {
                    footstepInfo.SetFootStepTypeBasedOnCollider(rightFoot ? companion.rightFootSurfaceCollider : companion.leftFootSurfaceCollider);
                }
            }
            
            

            SoundManager.main.RequestHandle(positionToPlay, playTransform).PlayOneShot(footstepInfo.sounds[(int)footstepInfo.CurrentFootStepType].GetRandomAudioClip());

            if (footstepInfo.CurrentFootStepType == FootstepInfo.FootStepType.WATER)
            {
                Vector3 pos = floorPos + head.forward * 0.2f;
                Quaternion rot = Quaternion.LookRotation(Tools.FlatForward(head.forward), Vector3.up) * Quaternion.Euler(-60.0f, 0.0f, 0.0f);
                GameDirector.instance.SplashWaterFXAt(pos, rot, 0.7f, 1.8f, 15);
            }
            OnFootstep();
        }

        
    }

}