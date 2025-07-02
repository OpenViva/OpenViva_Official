using Steamworks.Data;
using UnityEngine;


namespace Viva
{


    public class KissingBehavior : PassiveBehaviors.PassiveTask
    {

        private Vector3 cheekOffset = Vector3.zero;
        public Companion.Animation postKissAnim = Companion.Animation.NONE;
        private bool animBusy = false;
        private float waitToFaceTimer = 0.0f;
        private bool kissDisableFaceYaw = false;

        public KissingBehavior(Companion _self) : base(_self, 0.0f)
        {

            cheekOffset = new Vector3(0.019f, 0.016f, 0.018f);
        }


        public class TransitionToPostKiss : Companion.TransitionHandle
        {

            public TransitionToPostKiss() : base(TransitionType.NO_MIRROR)
            {
            }
            public override void Transition(Companion self)
            {
                self.UpdateAnimationTransition(self.passive.kissing.postKissAnim);
            }
        }

        private bool IsNearCheek(float side)
        {
            cheekOffset.x = Mathf.Abs(cheekOffset.x) * side;
            Vector3 cheekPos = self.head.TransformPoint(cheekOffset);

            if (Vector3.Distance(GameDirector.player.head.transform.position, cheekPos) < 0.22f)
            {
                return true;
            }
            return false;
        }

        private void checkKissingProximity()
        {
            if (animBusy)
            {
                return;
            }
            if (self.bodyState != BodyState.STAND)
            {
                return;
            }
            if (IsNearCheek(-1.0f))
            {
                if (self.IsHappy())
                {
                    self.SetTargetAnimation(Companion.Animation.STAND_KISS_HAPPY_CHEEK_LEFT);
                    facePlayer(2.0f);
                }
                else
                {
                    self.SetTargetAnimation(Companion.Animation.STAND_KISS_ANGRY_CHEEK_LEFT);
                    if (Random.value > 0.95)
                    {
                        postKissAnim = Companion.Animation.STAND_KISS_ANGRY_RIGHT_TO_ANGRY;
                    }
                    else
                    {
                        postKissAnim = Companion.Animation.STAND_KISS_ANGRY_RIGHT_TO_HAPPY;
                    }
                    facePlayer(1.0f);
                }
            }
            else if (IsNearCheek(1.0f))
            {
                if (self.IsHappy())
                {
                    self.SetTargetAnimation(Companion.Animation.STAND_KISS_HAPPY_CHEEK_RIGHT);
                    facePlayer(2.0f);
                }
                else
                {
                    self.SetTargetAnimation(Companion.Animation.STAND_KISS_ANGRY_CHEEK_RIGHT);
                    if (Random.value > 0.95)
                    {
                        postKissAnim = Companion.Animation.STAND_KISS_ANGRY_LEFT_TO_ANGRY;
                    }
                    else
                    {
                        postKissAnim = Companion.Animation.STAND_KISS_ANGRY_LEFT_TO_HAPPY;
                    }
                    facePlayer(1.0f);
                }
            }
        }

        public override void OnUpdate()
        {

            //disable during hugging
            if (self.active.IsTaskActive(self.passive.hug))
            {
                return;
            }
            checkKissingProximity();

        }

        private void facePlayer(float faceDirSpeedMult)
        {
            //self.SetRootFacingTarget( GameDirector.player.head.transform.position, 100.0f*faceDirSpeedMult, 20.0f*faceDirSpeedMult, 10.0f );
            self.SetLookAtTarget(GameDirector.player.head);
            self.autonomy.Interrupt(new AutonomyFaceDirection(self.autonomy, "face direction", delegate (TaskTarget target)
            {
                target.SetTargetPosition(GameDirector.player.head.transform.position);
            }, faceDirSpeedMult));

            
        }

        public override void OnAnimationChange(Companion.Animation oldAnim, Companion.Animation newAnim)
        {
            switch (newAnim)
            {
                case Companion.Animation.STAND_KISS_ANGRY_LEFT_TO_HAPPY:
                case Companion.Animation.STAND_KISS_ANGRY_RIGHT_TO_HAPPY:
                    self.ShiftHappiness(2);
                    break;
            }
            switch (oldAnim)
            {
                case Companion.Animation.STAND_KISS_ANGRY_LEFT_TO_HAPPY:
                case Companion.Animation.STAND_KISS_ANGRY_LEFT_TO_ANGRY:
                case Companion.Animation.STAND_KISS_ANGRY_RIGHT_TO_ANGRY:
                case Companion.Animation.STAND_KISS_ANGRY_RIGHT_TO_HAPPY:
                case Companion.Animation.STAND_KISS_ANGRY_CHEEK_RIGHT:
                case Companion.Animation.STAND_KISS_ANGRY_CHEEK_LEFT:
                    animBusy = false;
                    break;
                case Companion.Animation.STAND_KISS_HAPPY_CHEEK_RIGHT:
                case Companion.Animation.STAND_KISS_HAPPY_CHEEK_LEFT:
                    animBusy = false;
                    self.RemoveDisableFaceYaw(ref kissDisableFaceYaw);
                    break;
            }
            switch (newAnim)
            {
                case Companion.Animation.STAND_KISS_ANGRY_LEFT_TO_HAPPY:
                case Companion.Animation.STAND_KISS_ANGRY_LEFT_TO_ANGRY:
                case Companion.Animation.STAND_KISS_ANGRY_RIGHT_TO_ANGRY:
                case Companion.Animation.STAND_KISS_ANGRY_RIGHT_TO_HAPPY:
                case Companion.Animation.STAND_KISS_ANGRY_CHEEK_RIGHT:
                case Companion.Animation.STAND_KISS_ANGRY_CHEEK_LEFT:
                    animBusy = true;
                    break;
                case Companion.Animation.STAND_KISS_HAPPY_CHEEK_RIGHT:
                case Companion.Animation.STAND_KISS_HAPPY_CHEEK_LEFT:
                    animBusy = true;
                    self.ApplyDisableFaceYaw(ref kissDisableFaceYaw);
                    break;
            }
            switch (newAnim)
            {
                case Companion.Animation.STAND_KISS_ANGRY_LEFT_TO_ANGRY:
                case Companion.Animation.STAND_KISS_ANGRY_RIGHT_TO_ANGRY:
                    GameDirector.player.CompleteAchievement(Player.ObjectiveType.KISS_ANGRY_WIPE, new Achievement("KISS_ANGRY_WIPE"));
                    break;
                case Companion.Animation.STAND_KISS_ANGRY_LEFT_TO_HAPPY:
                case Companion.Animation.STAND_KISS_ANGRY_RIGHT_TO_HAPPY:
                    GameDirector.player.CompleteAchievement(Player.ObjectiveType.KISS_MAKE_HAPPY, new Achievement("KISS_MAKE_HAPPY"));
                    break;
            }
        }
    }

}