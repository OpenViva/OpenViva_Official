using UnityEngine;


namespace Viva
{


    public partial class PokeBehavior : PassiveBehaviors.PassiveTask
    {

        private Companion.Animation GetBathtubIdleFacePokedAnimation(int pokeSideIsLeft)
        {
            if (self.IsHappy())
            {
                int baseEnumVal = (int)Companion.Animation.BATHTUB_IDLE_FACE_POKE_1_RIGHT;
                int pokeIntensity = (int)(Random.value * 1.9f); //0 or 1
                Companion.Animation pokeAnimation = (Companion.Animation)(baseEnumVal + pokeIntensity * 2 + pokeSideIsLeft);
                if (self.currentAnim == pokeAnimation)
                {
                    //alternate animation so it always replay a new poke animation
                    pokeIntensity = ++pokeIntensity % 2;
                    pokeAnimation = (Companion.Animation)(baseEnumVal + pokeIntensity * 2 + pokeSideIsLeft);
                }
                return pokeAnimation;
            }
            else
            {
                if (self.currentAnim != Companion.Animation.BATHTUB_SINK_ANGRY)
                {
                    return Companion.Animation.BATHTUB_SINK_ANGRY;
                }
                else
                {
                    return Companion.Animation.BATHTUB_ANGRY_IDLE_LOOP;
                }
            }
        }

        private Companion.Animation GetBathtubRelaxPostFacePokedAnimation(int pokeBearingSign)
        {
            if (self.IsHappy())
            {
                return Companion.Animation.BATHTUB_RELAX_TO_HAPPY_IDLE;
            }
            else
            {
                return Companion.Animation.BATHTUB_RELAX_TO_ANGRY_IDLE;
            }
        }

        private Companion.Animation GetBathtubIdlePostFacePokedAnimation(int pokeBearingSign)
        {
            if (self.IsHappy())
            {
                return Companion.Animation.BATHTUB_HAPPY_IDLE_LOOP;
            }
            else
            {
                return Companion.Animation.BATHTUB_ANGRY_IDLE_LOOP;
            }
        }
    }

}