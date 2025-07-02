using UnityEngine;

namespace Viva
{

    public static class CompanionUtility
    {

        public static AutonomyPlayAnimation CreateSpeechAnimation(Companion companion, AnimationSet animationSet, SpeechBubble bubble)
        {
            var playConfuseAnim = new AutonomyPlayAnimation(companion.autonomy, "speech", companion.GetAnimationFromSet(animationSet));
            playConfuseAnim.onAnimationEnter += delegate
            {
                companion.speechBubbleDisplay.DisplayBubble(GameDirector.instance.GetSpeechBubbleTexture(bubble));
            };
            return playConfuseAnim;
        }

        public static AutonomyFaceDirection SetRootFacingTarget(Companion companion, string name, float speedMultiplier, Vector3 position)
        {
            var setRootFacingTarget = new AutonomyFaceDirection(companion.autonomy, name, delegate (TaskTarget target)
            {
                target.SetTargetPosition(position);
            }, speedMultiplier);

            return setRootFacingTarget;        
        }
    }

}