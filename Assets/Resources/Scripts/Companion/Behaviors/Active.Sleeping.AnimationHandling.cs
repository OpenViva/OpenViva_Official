namespace Viva
{


    public partial class SleepingBehavior : ActiveBehaviors.ActiveTask
    {

        // public Companion.Animation GetLaySidePillowHeadpatStartAnimation(){

        // 	if( !layingOnRightSide.HasValue ){
        // 		return Companion.Animation.NONE;
        // 	}
        // 	if( phase == SleepingPhase.SLEEPING ){
        // 		if( layingOnRightSide.Value ){
        // 			return Companion.Animation.SLEEP_PILLOW_SIDE_IDLE_RIGHT;
        // 		}else{
        // 			return Companion.Animation.SLEEP_PILLOW_SIDE_IDLE_LEFT;
        // 		}
        // 	}else{
        // 		if( layingOnRightSide.Value ){
        // 			return Companion.Animation.LAY_PILLOW_SIDE_HAPPY_IDLE_RIGHT;
        // 		}else{
        // 			return Companion.Animation.LAY_PILLOW_SIDE_HAPPY_IDLE_LEFT;
        // 		}
        // 	}
        // }

        // public Companion.Animation GetSleepSidePillowHeadpatStartAnimation(){

        // 	if( !layingOnRightSide.HasValue ){
        // 		return Companion.Animation.NONE;
        // 	}
        // 	if( layingOnRightSide.Value ){
        // 		return Companion.Animation.SLEEP_PILLOW_SIDE_HEADPAT_START_RIGHT;
        // 	}else{
        // 		return Companion.Animation.SLEEP_PILLOW_SIDE_HEADPAT_START_LEFT;
        // 	}
        // }


        // public Companion.Animation GetSleepPillowUpHeadpatStartAnimation(){
        // 	return Companion.Animation.SLEEP_PILLOW_UP_IDLE;
        // }

        // public Companion.Animation GetSleepPillowUpHeadpatIdleAnimation(){
        // 	return GetSleepPillowUpHeadpatStartAnimation();
        // }

        // public Companion.Animation GetAwakePillowUpHeadpatStartAnimation(){
        // 	return self.GetLastReturnableIdleAnimation();
        // }

        // public Companion.Animation GetAwakePillowUpHeadpatIdleAnimation(){
        // 	if( self.IsHappy() ){
        // 		return Companion.Animation.AWAKE_HAPPY_PILLOW_UP_HEADPAT_LOOP;
        // 	}else{
        // 		return self.GetLastReturnableIdleAnimation();
        // 	}
        // }

        // public Companion.Animation GetSleepSidePillowHeadpatIdleAnimation(){
        // 	if( !layingOnRightSide.HasValue ){
        // 		return Companion.Animation.NONE;
        // 	}
        // 	if( layingOnRightSide.Value ){
        // 		return Companion.Animation.SLEEP_PILLOW_SIDE_IDLE_RIGHT;
        // 	}else{
        // 		return Companion.Animation.SLEEP_PILLOW_SIDE_IDLE_LEFT;
        // 	}
        // }

        // public Companion.Animation GetLaySidePillowHeadpatIdleAnimation(){
        //     return GetLaySidePillowHeadpatStartAnimation();	//same animation
        // }

        public Companion.Animation GetSleepSidePillowFacePokeAnimation(int pokeSideIsLeft)
        {
            if (!layingOnRightSide.HasValue)
            {
                return Companion.Animation.NONE;
            }
            if (phase == SleepingPhase.SLEEPING)
            {
                if (layingOnRightSide.Value)
                {
                    return Companion.Animation.SLEEP_PILLOW_SIDE_BOTHER_RIGHT;
                }
                else
                {
                    return Companion.Animation.SLEEP_PILLOW_SIDE_BOTHER_LEFT;
                }
            }
            else
            {
                //TODO: sleep side pillow awake poke animations?
            }
            return Companion.Animation.NONE;
        }

        public Companion.Animation GetSleepSidePillowPostFacePokeAnimation()
        {

            if (!layingOnRightSide.HasValue)
            {
                return Companion.Animation.NONE;
            }

            if (CheckIfShouldWakeUpFromBother())
            {
                return GetWakeUpAnimation(false);
            }
            if (self.passive.poke.pokeCount >= 2)
            {
                if (layingOnRightSide.Value)
                {
                    return Companion.Animation.SLEEP_PILLOW_SIDE_TO_SLEEP_PILLOW_UP_RIGHT;
                }
                else
                {
                    return Companion.Animation.SLEEP_PILLOW_SIDE_TO_SLEEP_PILLOW_UP_LEFT;
                }
            }
            return Companion.Animation.NONE;
        }

        public Companion.Animation GetSleepPillowUpPostFacePokeAnimation()
        {

            if (CheckIfShouldWakeUpFromBother())
            {
                return GetWakeUpAnimation(false);
            }
            if (self.passive.poke.pokeCount >= 2)
            {
                if (UnityEngine.Random.value > 0.5f)
                {
                    return Companion.Animation.SLEEP_PILLOW_UP_TO_SLEEP_PILLOW_SIDE_LEFT;
                }
                else
                {
                    return Companion.Animation.SLEEP_PILLOW_UP_TO_SLEEP_PILLOW_SIDE_RIGHT;
                }
            }
            return Companion.Animation.NONE;
        }
    }

}