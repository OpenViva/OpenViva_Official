using Steamworks.Data;
using UnityEngine;
using Viva.Util;


namespace Viva
{


    public class CattailBehavior : ActiveBehaviors.ActiveTask
    {

        public CattailBehavior(Companion _self) : base("Holding a Cattail", _self, ActiveBehaviors.Behavior.CATTAIL, null)
        {
        }

        public void SmackWithHandObject()
        {
            if (Vector3.SqrMagnitude(self.floorPos - GameDirector.player.floorPos) < 2.25f)
            {
                if (Mathf.Abs(Tools.Bearing(self.transform, GameDirector.player.floorPos)) < 30.0f)
                {
                    GameDirector.instance.postProcessing.DisplayScreenEffect(GamePostProcessing.Effect.HURT);
                    self.active.SetTask(self.active.idle, true);
                    if (self.currentAnim == Companion.Animation.STAND_CATTAIL_SWING_RIGHT)
                    {
                        AttemptPlaySmackSound(self.rightHandState);
                    }
                    else
                    {
                        AttemptPlaySmackSound(self.leftHandState);
                    }
                    GameDirector.player.CompleteAchievement(Player.ObjectiveType.WATER_REED_SMACK, new Achievement("WATER_REED_SMACK"));
                    if (Random.value > 0.5f && self.happiness == Companion.Happiness.VERY_ANGRY)
                    {
                        self.ShiftHappiness(1);
                    }
                }
            }
        }

        public void StartCattailBehavior()
        {
            if (self.GetCurrentLookAtItem() != null && self.GetCurrentLookAtItem().settings.itemType == Item.Type.CHARACTER)
            {
                self.active.SetTask(this);
                DoCattailBehavior();
            }
        }

        private void DoCattailBehavior()
        {

            var empty = new AutonomyEmpty(self.autonomy, "empty");
            var moveto = new AutonomyMoveTo(self.autonomy, "move to player", delegate (TaskTarget target)
            {
                target.SetTargetPosition(GameDirector.player.floorPos + Vector3.up * 0.1f);
            }, 0.5f, BodyState.STAND);
            var face = new AutonomyFaceDirection(self.autonomy, "face player", delegate (TaskTarget target)
            {
                target.SetTargetItem(GameDirector.player.headItem);
            }, 2.0f);
            empty.AddRequirement(moveto);
            empty.AddRequirement(face);

            moveto.onSuccess += delegate
            {
            };

            empty.onSuccess += delegate
            {
                if (Mathf.Abs(Tools.Bearing(self.transform, GameDirector.player.floorPos)) < 35.0f &&
                self.IsCurrentAnimationIdle())
                {

                    int availableAttackMove = 0;
                    if (self.rightHandState.heldItem != null &&
                        self.rightHandState.heldItem.settings.itemType == Item.Type.WATER_REED)
                    {
                        availableAttackMove++;
                    }
                    if (self.leftHandState.heldItem != null &&
                        self.leftHandState.heldItem.settings.itemType == Item.Type.WATER_REED)
                    {
                        availableAttackMove += 2;
                    }
                    self.SetLookAtTarget(GameDirector.player.head);
                    self.locomotion.StopMoveTo();
                    switch (availableAttackMove)
                    {
                        case 1:
                            self.SetTargetAnimation(Companion.Animation.STAND_CATTAIL_SWING_RIGHT);
                            break;
                        case 2:
                            self.SetTargetAnimation(Companion.Animation.STAND_CATTAIL_SWING_LEFT);
                            break;
                        case 3:
                            float baseAnim = (float)Companion.Animation.STAND_CATTAIL_SWING_RIGHT;
                            //increment to next SWING_LEFT if random value > 0.5
                            self.SetTargetAnimation((Companion.Animation)(baseAnim + 0.5f + Random.value));
                            break;
                    }
                }
            };
            self.autonomy.SetAutonomy(empty);

        }

        public override void OnDeactivate()
        {
            self.locomotion.StopMoveTo();
        }

        private void AttemptPlaySmackSound(OccupyState mainHoldState)
        {
            Cattail cattail = self.rightHandState.heldItem as Cattail;
            if (cattail)
            {
                cattail.PlaySmackSound();
            }
        }

        public override void OnUpdate()
        {

            if (Vector3.SqrMagnitude(GameDirector.player.floorPos - self.floorPos) > 2.25f)
            {   //1.5f squared                
                //self.active.follow.UpdateFollowRefresh( GameDirector.player.floorPos, 0.55f );
                self.SetTargetAnimation(Companion.Animation.STAND_CHASE_LOCOMOTION);
                if (!self.IsSpeakingAtAll() && self.currentAnim == Companion.Animation.STAND_CHASE_LOCOMOTION)
                {
                    self.SpeakAtRandomIntervals(Companion.VoiceLine.ANGRY_LONG, 2.5f, 5.0f);
                }
            }
            else
            {
                // self.SetRootFacingTarget( GameDirector.player.floorPos, 140.0f, 25.0f, 20.0f );
                //self.autonomy.Interrupt(new AutonomyFaceDirection(self.autonomy, "face direction cattail", delegate (TaskTarget target)
                //{
                //    target.SetTargetPosition(GameDirector.player.floorPos);
                //}, 3.0f));                
            }
        }
    }

}