namespace Viva
{


    public partial class HeadpatBehavior : PassiveBehaviors.PassiveTask
    {

        private Companion.Animation GetTiredHeadpatStartAnimation()
        {
            return Companion.Animation.STAND_TIRED_HEADPAT_IDLE;
        }
        private Companion.Animation GetTiredHeadpatIdleAnimation()
        {
            return GetTiredHeadpatStartAnimation(); //same animation
        }
    }

}