using UnityEngine;


namespace Viva
{


    public class FollowBehavior : ActiveBehaviors.ActiveTask
    {

        public FollowBehavior(Companion _self) : base("Following", _self, ActiveBehaviors.Behavior.FOLLOW, null)
        {
            
        }

        private AutonomyEmpty follow;
        private AutonomyWait waitTryAgain;
        private Character lastCaller;

        public override bool OnGesture(Item source, Gesture gesture)
        {
            switch (gesture)
            {
                case Gesture.STOP:
                    Debug.Log("Stopping Follow");
                    StopFollowing();
                    return true;
                case Gesture.FOLLOW:
                    lastCaller = source.mainOwner;
                    return AttemptFollow(source.mainOwner);

                    
            }
            return false;
        }

        public bool AttemptFollow(Character source, bool requiresLOS = true)
        {
            StopFollowing();
            if (source == null)
            {
                return false;
            }
            //Debug.Log($"Attempt Follow A: {source.name}");
            //if the player isint in view dont follow
            if (!self.CanSeePoint(source.head.transform.position) && requiresLOS)
            {
                return false;
            }
            //Debug.Log("Attempt Follow B");
            if (self.IsHappy() || self.IsTired() && self.IsHappy())
            {

                Character followTargetCharacter = source;

                if (AttemptFollowFromHorse(followTargetCharacter))
                {
                    return true;
                }

                BodyState targetBodyState = followTargetCharacter.IsSittingOnFloor() ? BodyState.FLOOR_SIT : BodyState.STAND;

                follow = new AutonomyEmpty(self.autonomy, "follow empty");

                var moveto = new AutonomyMoveTo(self.autonomy, "follow MoveTo", delegate (TaskTarget target)
                {
                    target.SetTargetPosition(followTargetCharacter.floorPos + Vector3.up * 0.1f);
                },
                1.0f, targetBodyState);
                
                moveto.keepDistance = false;
                moveto.allowRunning = true;
                moveto.onFail += TryAgainLater;

                if (Random.value > 0.5f) //Have chance to agree
                {
                    var agree = new AutonomyPlayAnimation(self.autonomy, "follow agree", Companion.Animation.STAND_AGREE);
                    agree.onSuccess += delegate
                    {
                        follow.RemoveRequirement(agree);
                    };
                    follow.AddRequirement(agree);
                }

                follow.AddRequirement(moveto);
                
                follow.onMoodChange += MoodChange;
                self.autonomy.SetAutonomy(follow);

                //Debug.Log("Attempt Follow C");
                self.active.SetTask(self.active.follow, null);
                return true;
            }
            
            self.active.idle.PlayAvailableRefuseAnimation();
            
            if (self.bodyState == BodyState.STAND)
            { //Make sure to only face direction when standing
                var faceDir = new AutonomyFaceDirection(self.autonomy, "face direction", delegate (TaskTarget target)
                {
                    target.SetTargetPosition(source.transform.position);
                }, 2.0f);
                faceDir.onSuccess += delegate { self.active.SetTask(self.active.idle, null); };

                self.autonomy.SetAutonomy(faceDir);
            }
            return false;
        }

        private void TryAgainLater()
        {
            waitTryAgain = new AutonomyWait(self.autonomy, "tryAgainTimer", 1.0f);
            waitTryAgain.onSuccess += delegate{
                waitTryAgain.Reset();
                AttemptFollow( lastCaller, false);
            };
            self.autonomy.SetAutonomy(waitTryAgain);
        }

        private void MoodChange(Companion.Happiness happiness)
        {
            if (happiness == Companion.Happiness.VERY_ANGRY) //Stop Following if theyre really mad
            {
                var refuseAnim = new AutonomyPlayAnimation(self.autonomy, "follow refuse anim",
                    self.IsTired() ? Companion.Animation.STAND_TIRED_REFUSE : Companion.Animation.STAND_REFUSE);
                refuseAnim.onSuccess += delegate { self.active.SetTask(self.active.idle, null); };

                refuseAnim.AddPassive(new AutonomyFaceDirection(self.autonomy, "face direction", delegate (TaskTarget target)
                {
                    target.SetTargetPosition(GameDirector.player.head.transform.position);
                }, 2.0f));

                self.autonomy.SetAutonomy(refuseAnim);
            }
            
        }

        public void StopFollowing()
        {
            if (follow != null)
            {
                self.autonomy.RemoveFromQueue("follow empty");
                self.locomotion.StopMoveTo();
                follow = null;
            
                self.autonomy.RemoveFromQueue( "tryAgainTimer" );
                self.active.SetTask(self.active.idle, null);
            }
            
        }

        private bool AttemptFollowFromHorse(Character owner)
        {
            Player player = owner as Player;
            if (player == null)
            {
                return false;
            }
            Horse horse = player.controller.vehicle as Horse;
            if (horse == null)
            {
                return false;
            }
            return self.active.horseback.AttemptRideHorsePassenger(horse);
        }
    }

}