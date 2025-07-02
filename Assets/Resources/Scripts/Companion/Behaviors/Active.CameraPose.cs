using UnityEngine;


namespace Viva
{


    public class CameraPoseBehavior : ActiveBehaviors.ActiveTask
    {

        private float cameraPoseTimer = 0.0f;

        public CameraPoseBehavior(Companion _self) : base("Posing for Camera", _self, ActiveBehaviors.Behavior.CAMERA_POSE, null)
        {
        }

        public override void OnDeactivate()
        {
            self.SetTargetAnimation(self.GetLastReturnableIdleAnimation());
        }

        public override void OnUpdate()
        {
            cameraPoseTimer -= Time.deltaTime;
            if (cameraPoseTimer < 0.0f)
            {
                self.active.SetTask(self.active.idle, false);
            }
            else
            {
                if (self.currentAnim != Companion.Animation.STAND_POSE_PEACE_IN &&
                    self.currentAnim != Companion.Animation.STAND_POSE_PEACE_LOOP)
                {

                    self.SetTargetAnimation(Companion.Animation.STAND_POSE_PEACE_IN);
                }
            }
        }

        public void AttemptPoseForCamera(Transform source)
        {

            if (source == null)
            {
                return;
            }
            if (self.bodyState != BodyState.STAND)
            {
                return;
            }
            if (self.IsHappy())
            {
                if (self.IsCurrentAnimationIdle())
                {
                    self.active.SetTask(this, null);
                }
                cameraPoseTimer = 1.25f;
            }
        }

    }
}