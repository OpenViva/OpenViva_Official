using OccaSoftware.Altos.Runtime;
using UnityEngine;


namespace Viva
{


    public class TiredBehavior : PassiveBehaviors.PassiveTask
    {


        public bool tired { get { return self.Tired; } }
        private bool hasShownTired = false;
        private float rubEyesTimer = 10.0f;
        public const float tiredTimeStart = 18.0f;
        public const float tiredTimeEnd = 4.0f;

        public TiredBehavior(Companion _self) : base(_self, 0.0f)
        {
        }

        public override void OnUpdate()
        {

            if (!tired)
            {
                float currentTime = GameDirector.newSkyDirector.skyDefinition.CurrentTime;
                if ((currentTime >= tiredTimeStart || currentTime < tiredTimeEnd) && AltosSkyDirector.Instance.skyOverride == null)
                {
                    BecomeTired();
                }
            }
            else if (!hasShownTired)
            {
                if (self.IsCurrentAnimationIdle())
                {
                    self.SetTargetAnimation(Companion.Animation.STAND_TO_STAND_TIRED);
                }
            }
            else
            {
                UpdateTired();
            }
        }

        private void UpdateTired()
        {

            //rub eyes occasionally
            if (!self.IsCurrentAnimationIdle())
            {
                return;
            }
            if (self.bodyState == BodyState.STAND)
            {
                rubEyesTimer -= Time.deltaTime;
                if (rubEyesTimer < 0.0f)
                {
                    if (self.rightHandState.heldItem != null)
                    {
                        self.SetTargetAnimation(Companion.Animation.STAND_TIRED_RUB_EYES_RIGHT);
                    }
                    else
                    {
                        self.SetTargetAnimation(Companion.Animation.STAND_TIRED_RUB_EYES_LEFT);
                    }
                }
            }
        }

        public void BecomeTired()
        {

            if (tired)
            {
                return;
            }
            self.ShiftHappiness(4); //treat as happy
            self.Tired = true;
            hasShownTired = false;
        }

        public override void OnAnimationChange(Companion.Animation oldAnim, Companion.Animation newAnim)
        {

            switch (newAnim)
            {
                case Companion.Animation.STAND_TIRED_LOCOMOTION:
                    hasShownTired = true;
                    break;
                case Companion.Animation.STAND_TIRED_RUB_EYES_RIGHT:
                case Companion.Animation.STAND_TIRED_RUB_EYES_LEFT:
                    rubEyesTimer = 45.0f + Random.value * 15.0f;
                    break;
            }
        }
    }

}