using System.Collections.Generic;
using UnityEngine;


namespace Viva
{


    public partial class IdleBehavior : ActiveBehaviors.ActiveTask
    {

        private float nextIdleVariationTime = 3.0f;
        private Companion.Animation nextStandIdleAnim = Companion.Animation.STAND_HAPPY_IDLE1;
        private float idleRootFacingTargetTimer = 0.0f;
        private float checkForInterestsTimer = 0.0f;
        private float ignoreDesirableItemsTimer = 0.0f;
        private int idleVersion = 0;
        public bool enableFaceTargetTimer;
        public bool hasSaidGoodMorning = true;
        private bool tryingToGrabInterest = false;
        
        private float lastYummyReactTime = 0.0f;

        public IdleBehavior(Companion _self) : base("Idling", _self, ActiveBehaviors.Behavior.IDLE, null)
        {

            enableFaceTargetTimer = true;

            InitItemSubBehaviors();
            InitBagAnimations();
        }

        public void PlayAvailableRefuseAnimation()
        {
            if (self.IsTired())
            {
                self.SetTargetAnimation(Companion.Animation.STAND_TIRED_REFUSE);
            }
            else
            {
                self.SetTargetAnimation(Companion.Animation.STAND_REFUSE);
            }
        }

        public void PlayAvailableDisinterestedAnimation()
        {

            switch (self.bodyState)
            {
                case BodyState.STAND:
                    if (Random.value > 0.5f)
                    {
                        self.SetTargetAnimation(Companion.Animation.STAND_HEADPAT_INTERRUPT);
                    }
                    else
                    {
                        self.SetTargetAnimation(Companion.Animation.STAND_HEADPAT_ANGRY_END);
                    }
                    break;
            }
        }

        public bool AttemptImpress()
        {
            if (!self.IsCurrentAnimationIdle())
            {
                return false;
            }
            if (!self.IsHappy())
            {
                PlayAvailableDisinterestedAnimation();
                return false;
            }
            self.SetViewAwarenessTimeout(1.0f);
            switch (self.bodyState)
            {
                case BodyState.STAND:
                    if (self.IsTired())
                    {
                        self.SetTargetAnimation(Companion.Animation.STAND_TIRED_REFUSE);
                        return false;
                    }
                    self.SetTargetAnimation(Companion.Animation.STAND_IMPRESSED1);
                    break;
                case BodyState.FLOOR_SIT:
                    self.SetTargetAnimation(Companion.Animation.FLOOR_SIT_IMPRESSED1);
                    break;
            }
            return true;
        }

        public Companion.Animation GetAvailableWaveAnimation()
        {
            switch (self.bodyState)
            {
                case BodyState.STAND:
                    if (self.IsHappy())
                    {
                        if (self.rightHandState.holdType == HoldType.NULL || self.leftHandState.holdType == HoldType.NULL)
                        {
                            if (self.rightHandState.holdType == HoldType.NULL)
                            {
                                return Companion.Animation.STAND_WAVE_HAPPY_RIGHT;
                            }
                            else
                            {
                                return Companion.Animation.STAND_WAVE_HAPPY_LEFT;
                            }
                        }
                    }
                    else
                    {
                        if (Random.value > 0.5f)
                        {
                            return Companion.Animation.STAND_HEADPAT_INTERRUPT;  //reuses animation
                        }
                        else
                        {
                            return Companion.Animation.STAND_HEADPAT_ANGRY_END;  //reuses animation
                        }
                    }
                    return Companion.Animation.NONE;
                case BodyState.BATHING_IDLE:
                    if (self.IsHappy())
                    {
                        return Companion.Animation.BATHTUB_WAVE_HAPPY_RIGHT;
                    }
                    else
                    {
                        return Companion.Animation.NONE;
                    }
                default:
                    return Companion.Animation.NONE;
            }
        }

        public override bool OnGesture(Item source, Gesture gesture)
        {
            if (gesture == Gesture.HELLO)
            {
                if (self.IsCurrentAnimationIdle() &&
                    self.CanSeePoint(source.transform.position))
                {

                    Companion.Animation waveAnimation = GetAvailableWaveAnimation();
                    if (waveAnimation != Companion.Animation.NONE)
                    {

                        self.SetTargetAnimation(waveAnimation);
                        self.SetLookAtTarget(source.transform);
                        // self.SetRootFacingTarget( source.transform.position, 100.0f, 10.0f, 15.0f );
                        self.autonomy.Interrupt(new AutonomyFaceDirection(self.autonomy, "face direction", delegate (TaskTarget target)
                        {
                            target.SetTargetPosition(source.transform.position);
                        }, 2.0f));
                        return true;
                    }
                }
            }
            return false;
        }


        public override void OnUpdate()
        {
            UpdateIdleRootFacingTargetTimer();
            
            //update shoulder items lolic
            if (self.rightShoulderState.occupied)
            {
                UpdateShoulderItemInteraction(self.rightShoulderState);
            }
            else
            {
                UpdateShoulderItemInteraction(self.leftShoulderState);
            }
            if (self.IsCurrentAnimationIdle())
            {
                if (Random.value > 0.5f)
                {   //update random hand item
                    UpdateIdleHoldItemInteraction(self.rightCompanionHandState);
                }
                else
                {
                    UpdateIdleHoldItemInteraction(self.leftCompanionHandState);
                }
                CheckForVisibleNewInterests();
                CheckToSayGoodMorning();
                UpdateIdleVariations();
            }
        }

        private void CheckToSayGoodMorning()
        {
            //shinobu has no good morning voice line
            //TODO: Move after waking up instead of checking constantly
            if (self.headModel.voiceIndex != (byte)Voice.VoiceType.SHINOBU)
            {
                if (!hasSaidGoodMorning && self.IsHappy())
                {
                    Player player = GameDirector.instance.FindNearbyPlayer(self.head.position, 3.0f);
                    if (self.GetCurrentLookAtItem() != null && self.GetCurrentLookAtItem().mainOwner == player)
                    {
                        self.SetTargetAnimation(Companion.Animation.STAND_SOUND_GOOD_MORNING);
                    }
                }
            }
        }

        public void IgnoreDesirableItems(float duration)
        {
            ignoreDesirableItemsTimer = duration;
        }

        public void CheckForVisibleNewInterests()
        {
            //Debug.Log("V:"+self.GetViewResultCount());
            if (self.bodyState != BodyState.STAND)
            {
                return;
            }
            //do not check for new interests if in a polling state
            if (self.active.isPolling)
            {
                return;
            }
            checkForInterestsTimer -= Time.deltaTime;
            ignoreDesirableItemsTimer -= Time.deltaTime;
            if (checkForInterestsTimer > 0.0f)
            {
                return;
            }
            checkForInterestsTimer = 0.4f;
            List<Item> candidates = new List<Item>();
            for (int i = 0; i < self.GetViewResultCount(); i++)
            {
                Item item = self.GetViewResult(i);

                if (item == null)
                {
                    continue;
                }
                if (item.settings.itemType == Item.Type.REINS)
                {   //DO NOT ALLOW PICKING UP HORSE REIN
                    continue;
                }
                if (item.HasPickupReason(Item.PickupReasons.BEING_PRESENTED))
                {
                }
                else if (item.HasPickupReason(Item.PickupReasons.HIGHLY_DESIRABLE))
                {
                    if (self.ShouldIgnore(item))
                    {
                        continue;
                    }
                    if (item.HasAttribute(Item.Attributes.DISABLE_PICKUP))
                    {
                        continue;
                    }
                    if (ignoreDesirableItemsTimer > 0.0f)
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
                candidates.Add(item);
            }
            if (candidates.Count > 0)
            {
                float minDist = Mathf.Infinity;
                Item closest = null;
                for (int i = 0; i < candidates.Count; i++)
                {
                    float dist = Vector3.SqrMagnitude(candidates[i].transform.position - self.head.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = candidates[i];
                    }
                }
                //cannot be interested of an already owned item
                if (closest.mainOwner == self)
                {
                    return;
                }

                if (tryingToGrabInterest) return; //if already trying to pickup return my brain is fried rn so i dont know any other solutions

                if (self.GetPreferredHandState(closest) == null)
                {
                    return;
                }

                // var pickup = new AutonomyPickup(self.autonomy, "pickup interest", closest,
                //     self.GetPreferredHandState(closest), true);
                //
                // pickup.onRegistered += delegate
                // {
                //     tryingToGrabInterest = true;
                // };
                // pickup.onUnregistered += delegate
                // {
                //     tryingToGrabInterest = false;
                // };
                //
                // self.autonomy.SetAutonomy(pickup);
                //self.active.pickup.AttemptGoAndPickup( closest, self.active.pickup.FindPreferredHandState( closest ) );
            }
        }

        public Companion.Animation GetAvailableIdleAnimation()
        {
            if (self.IsHappy() && !self.IsTired())
            {
                idleVersion = (idleVersion + 1) % 2;    //cycle
                switch (self.bodyState)
                {
                    case BodyState.STAND:
                        switch (idleVersion)
                        {
                            case 0:
                                return Companion.Animation.NONE;
                            case 1:
                                return Companion.Animation.STAND_HAPPY_IDLE2;
                        }
                        break;
                }
            }
            return Companion.Animation.NONE;
        }

        private void UpdateIdleVariations()
        {
            
            //allow only if not holding anything (disables with items and handholding)
            if (self.rightHandState.heldItem != null || self.leftHandState.heldItem != null)
            {
                return;
            }
            if (Time.time >= nextIdleVariationTime)
            {
                nextIdleVariationTime = Time.time + 10.0f + Random.value * 15.0f;   //10~25 sec. wait
                var anim = GetAvailableIdleAnimation();
                if (anim != Companion.Animation.NONE)
                {
                    self.SetTargetAnimation(anim);
                }
            }
        }

        private void UpdateIdleRootFacingTargetTimer()
        {
            
            self.SetFacingRootTarget(GameDirector.player.head.transform.position);
            
            if (self.currentLookAtTransform != null && !self.locomotion.isMoveToActive())
            {
                idleRootFacingTargetTimer -= Time.deltaTime * System.Convert.ToInt32(enableFaceTargetTimer);
                if (idleRootFacingTargetTimer < 0.0f)
                {
                    idleRootFacingTargetTimer = 4.0f + Random.value * 4.0f;
                    if (self.active.RequestPermission(ActiveBehaviors.Permission.ALLOW_ROOT_FACING_TARGET_CHANGE))
                    {
                        if (self.bodyState != BodyState.AWAKE_PILLOW_UP && self.bodyState != BodyState.AWAKE_PILLOW_SIDE_LEFT && self.bodyState != BodyState.AWAKE_PILLOW_SIDE_RIGHT)
                        {
                            self.SetFacingRootTarget(self.currentLookAtTransform.position);
                        }
                    }
                }
            }
        }
        public override void OnAnimationChange(Companion.Animation oldAnim, Companion.Animation newAnim)
        {

            if (newAnim == Companion.Animation.STAND_SOUND_GOOD_MORNING)
            {
                hasSaidGoodMorning = true;
            }
            switch (oldAnim)
            {
                case Companion.Animation.STAND_HAPPY_DONUT_LAST_BITE_RIGHT:
                case Companion.Animation.STAND_HAPPY_DONUT_LAST_BITE_LEFT:
                    if (Time.time - lastYummyReactTime > 20.0f)
                    {
                        lastYummyReactTime = Time.time;
                        if (self.headModel.voiceIndex == (byte)Voice.VoiceType.SHINOBU)
                        {
                            self.SetTargetAnimation(Companion.Animation.STAND_SHINOBU_YUMMY);
                        }
                    }
                    break;
            }
        }
    }

}