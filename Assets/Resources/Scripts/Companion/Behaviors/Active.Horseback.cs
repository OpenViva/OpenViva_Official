using UnityEngine;
using Viva.Util;

namespace Viva
{


    public partial class HorsebackBehavior : ActiveBehaviors.ActiveTask
    {

        public class HorseSession : SerializedTaskData
        {
            [VivaFileAttribute]
            public VivaSessionAsset horseAsset { get; set; }

            public Horse horse { get { return horseAsset as Horse; } }
        }

        private Vector3 targetHorseSideWalkPos;
        private Tools.EaseBlend stabilizeMount = new Tools.EaseBlend();
        public HorseSession horseSession { get { return session as HorseSession; } }
        public bool isOnHorse { get; private set; } = false;


        public HorsebackBehavior(Companion _self) : base("Riding a Horse", _self, ActiveBehaviors.Behavior.HORSEBACK, new HorseSession())
        {
        }

        public bool AttemptRideHorsePassenger(Horse targetHorse)
        {
            if (targetHorse == null)
            {
                return false;
            }
            if (targetHorse.backSeat.owner != null)
            {
                return false;
            }
            self.active.SetTask(this, null);
            horseSession.horseAsset = targetHorse;
            RideHorse();
            return true;
        }

        public override void OnActivate()
        {

            GameDirector.player.objectFingerPointer.selectedCompanions.Remove(self);
            self.OnUnselected();
        }

        public override void OnDeactivate()
        {
            if (isOnHorse)
            {
                isOnHorse = false;
                self.onRagdollModeBegin -= EndHorseLogic;

                self.autonomy.RemoveFromQueue("ride horse global");
            }
            horseSession.horseAsset = null;
        }

        private void ConfusedAndFinalizeHorseback()
        {
            var playAnim = CompanionUtility.CreateSpeechAnimation(self, AnimationSet.CONFUSED, SpeechBubble.INTERROGATION);
            playAnim.onSuccess += delegate { self.active.SetTask(self.active.idle); };
            self.autonomy.SetAutonomy(playAnim);
        }

        private void DisappointAndFinalizeHorseback()
        {
            var failAnim = new AutonomyPlayAnimation(self.autonomy, "fail beg distance", Companion.Animation.STAND_HAPPY_DISAPPOINTMENT);
            failAnim.onSuccess += delegate { self.active.SetTask(self.active.idle); };
            self.autonomy.SetAutonomy(failAnim);
        }

        private void RideHorse()
        {
            if (horseSession.horse == null || horseSession.horse.backSeat.owner != null)
            {
                ConfusedAndFinalizeHorseback();
                return;
            }

            var moveToHorseSide = new AutonomyMoveTo(self.autonomy, "walk to horse side", delegate (TaskTarget target)
            {
                if (horseSession.horse)
                {
                    target.SetTargetPosition(horseSession.horse.transform.position);
                }
            }, 0.0f, BodyState.STAND, delegate (TaskTarget target)
            {
                float side = Vector3.Dot(Tools.FlatForward(horseSession.horse.spine1.forward), self.anchor.forward);

                target.SetTargetPosition(horseSession.horse.spine1.position - horseSession.horse.spine1.forward * side);
            });
            moveToHorseSide.onGeneratePathRequest = GeneratePathRequest;
            moveToHorseSide.onUpdate += delegate
            {
                if (horseSession.horse.backSeat.owner != null)
                {
                    DisappointAndFinalizeHorseback();
                }
            };
            moveToHorseSide.onSuccess += OnReachHorseSide;
            moveToHorseSide.onFail += ConfusedAndFinalizeHorseback;

            self.autonomy.SetAutonomy(moveToHorseSide);
        }

        private LocomotionBehaviors.PathRequest[] GeneratePathRequest(Vector3 target)
        {

            if (horseSession.horse == null)
            {
                return null;
            }
            //find valid horse side
            Vector3? rightSide = HorseSideIsClear(1);
            Vector3? leftSide = HorseSideIsClear(-1);
            if (rightSide == null && leftSide == null)
            {
                return null;
            }
            int valid;
            if (rightSide != null && leftSide != null)
            {
                valid = 2;
            }
            else
            {
                valid = 1;
            }

            LocomotionBehaviors.PathRequest[] requests = new LocomotionBehaviors.PathRequest[valid];
            valid = 0;
            if (rightSide != null)
            {
                requests[valid++] = new LocomotionBehaviors.NavSearchPoint(rightSide.Value, -horseSession.horse.spine1.forward);
            }
            if (leftSide != null)
            {
                requests[valid++] = new LocomotionBehaviors.NavSearchPoint(leftSide.Value, horseSession.horse.spine1.forward);
            }
            return requests;
        }

        private void OnReachHorseSide()
        {
            float horseSide = -Vector3.Dot(self.floorPos - horseSession.horse.spine1.position, horseSession.horse.spine1.forward);
            Companion.Animation entryAnim = horseSide > 0.0f ? Companion.Animation.STAND_TO_HORSEBACK_IDLE_LEFT : Companion.Animation.STAND_TO_HORSEBACK_IDLE_RIGHT;

            var overseeAnchor = new AutonomyEmpty(self.autonomy, "ride horse global", delegate { return null; });

            // var playJoyAnim = new AutonomyPlayAnimation(self.autonomy, "joy anim", Companion.Animation.HORSEBACK_JOY);
            // playJoyAnim.FlagForSuccess();
            //
            // var playJoyAnimInterval = new AutonomyWait(self.autonomy, "play joy anim", 20.0f);
            // playJoyAnimInterval.loop = true;
            // playJoyAnimInterval.onSuccess += delegate { playJoyAnim.Reset(); };

            var mountHorse = new AutonomyPlayAnimation(self.autonomy, "mount horse anim", entryAnim);
            mountHorse.onAnimationEnter += OnMountHorseEnterAnimation;
            mountHorse.onAnimationExit += OnMountHorseIdleAnimation;
            mountHorse.onRemovedFromQueue += EndHorseLogic;

            overseeAnchor.AddRequirement(new AutonomyFilterUse(self.autonomy, "horse seat", horseSession.horse.backSeat, 0.0f, true));
            overseeAnchor.AddRequirement(mountHorse);
            

            self.autonomy.SetAutonomy(overseeAnchor);
        }

        public override bool RequestPermission(ActiveBehaviors.Permission permission)
        {
            switch (permission)
            {
                case ActiveBehaviors.Permission.BEGIN_RIGHT_HANDHOLD:
                case ActiveBehaviors.Permission.BEGIN_LEFT_HANDHOLD:
                    return false;
            }
            return true;
        }

        private Vector3? HorseSideIsClear(int side)
        {
            if (!GamePhysics.GetRaycastInfo(horseSession.horse.spine1.position, horseSession.horse.spine1.forward * side + Vector3.down * 1.5f, 2.5f, WorldUtil.wallsMask))
            {
                return null;
            }
            return GamePhysics.result().point;
        }

        private void LateUpdatePreLookAtOnHorse()
        {

            //stabilize spine2, spine3, and head
            stabilizeMount.Update(Time.deltaTime);
            StabilizeBone(self.spine2, stabilizeMount.value, 30.0f);
            StabilizeBone(self.spine2.GetChild(0), stabilizeMount.value, 30.0f);

            //prepare hands for grabbing the horse IK
            self.SetupArmIKForLateUpdate(self.rightCompanionHandState, Companion.IKAnimation.HORSE_MOUNT_HOLD_RIGHT);
            self.SetupArmIKForLateUpdate(self.leftCompanionHandState, Companion.IKAnimation.HORSE_MOUNT_HOLD_LEFT);
        }

        public void BeginMount()
        {
            if (horseSession.horse == null)
            {
                return;
            }
            stabilizeMount.reset(0.0f);
            stabilizeMount.StartBlend(1.0f, 1.0f);
        }

        public override bool OnGesture(Item source, Gesture gesture)
        {

            bool sourceComesFromHorseRider = false;
            Player player = source.mainOwner as Player;
            if (player != null)
            {
                if (player.controller as HorseControls != null)
                {
                    sourceComesFromHorseRider = true;
                }
            }
            if (gesture == Gesture.FOLLOW)
            {
                if (isOnHorse)
                {
                    if (!sourceComesFromHorseRider)
                    {
                        BeginUnmount();
                    }
                }
                else if (!sourceComesFromHorseRider)
                {
                    // if( self.active.currentType == type ){
                    // 	self.active.follow.AttemptFollow( source );
                    // }
                }
            }
            return false;
        }

        public void BeginUnmount()
        {
            self.SetTargetAnimation(self.GetLastReturnableIdleAnimation());
            self.StopActiveAnchor();
            self.anchor.transform.SetParent(self.transform, true);
            self.active.SetTask(self.active.idle, true);
        }

        public void OnHardStop()
        {
            self.SetTargetAnimation(Companion.Animation.HORSEBACK_HARD_STOP);
        }

        private void StabilizeBone(Transform bone, float blend, float maxDegrees)
        {
            Vector3 stabilizedDir = Vector3.LerpUnclamped(bone.forward, Tools.FlatForward(bone.forward), blend);
            Quaternion newRotation = Quaternion.LookRotation(stabilizedDir, Vector3.up);
            bone.rotation = Quaternion.RotateTowards(bone.rotation, newRotation, maxDegrees);
        }

        private void OnMountHorseIdleAnimation()
        {
            if (horseSession.horse != null)
            {
                self.BeginAnchorTransformAnimation(
                    self.rootAnimationOffsets[(int)Companion.TransformOffsetAnimation.HORSE_IDLE].position,
                    Quaternion.Euler(self.rootAnimationOffsets[(int)Companion.TransformOffsetAnimation.HORSE_IDLE].eulerRotation),
                    0.0f
                );
                self.AnchorSpineUntilTransitionEnds(horseSession.horse.spine0);
            }
        }

        private void EndHorseLogic()
        {
            self.active.SetTask(self.active.idle);
        }

        private void OnMountHorseEnterAnimation()
        {
            if (!isOnHorse)
            {
                self.onRagdollModeBegin += EndHorseLogic;
                isOnHorse = true;

                Companion.TransformOffsetAnimation offsetAnimation;
                if (self.currentAnim == Companion.Animation.STAND_TO_HORSEBACK_IDLE_RIGHT)
                {
                    offsetAnimation = Companion.TransformOffsetAnimation.HORSE_MOUNT_RIGHT;
                }
                else
                {
                    offsetAnimation = Companion.TransformOffsetAnimation.HORSE_MOUNT_LEFT;
                }

                self.anchor.transform.SetParent(horseSession.horse.spine0, true);
                self.BeginAnchorTransformAnimation(
                    self.rootAnimationOffsets[(int)offsetAnimation].position,
                    Quaternion.Euler(self.rootAnimationOffsets[(int)offsetAnimation].eulerRotation),
                    1.5f,
                    null
                );
            }
        }
    }

}