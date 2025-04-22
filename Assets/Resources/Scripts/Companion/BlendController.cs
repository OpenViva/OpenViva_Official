using UnityEngine;
using Viva.Util;




namespace Viva
{


    public class BlendController
    {

        public delegate float OnIKControl(BlendController blendController);

        private float easeDuration;
        public readonly CompanionHandState targetHandState;
        public readonly Companion.ArmIK armIK;
        public readonly Companion.ArmIK.RetargetingInfo retargetingInfo = new Companion.ArmIK.RetargetingInfo();
        public Companion.Animation targetAnimation;
        private readonly Tools.EaseBlend boundaryEase = new Tools.EaseBlend();
        private readonly OnIKControl controlCallback;
        private CompanionHandState.HoldBlendMax holdBlendMax = new CompanionHandState.HoldBlendMax();
        private bool registered = false;
        private bool listening = false;


        public BlendController(CompanionHandState _targetHandState, Companion.Animation _targetAnim, OnIKControl _controlCallback, float _easeDuration = 0.4f)
        {
            targetHandState = _targetHandState;
            if (targetHandState != null)
            {
                armIK = new Companion.ArmIK(targetHandState.holdArmIK);
                targetAnimation = _targetAnim;
                controlCallback = _controlCallback;
                Restore();

                //fire current animation state in case already in targetAnim
                Companion self = targetHandState.owner as Companion;
                ListenForAnimation(self.lastAnim, self.currentAnim);
            }

            easeDuration = _easeDuration;
        }

        public void Restore()
        {
            if (listening)
            {
                return;
            }
            listening = true;
            Companion self = targetHandState.owner as Companion;
            self.onAnimationChange += ListenForAnimation;
        }

        private void ListenForAnimation(Companion.Animation oldAnim, Companion.Animation newAnim)
        {

            if (newAnim == targetAnimation)
            {
                Register();
                return;
            }
            if (oldAnim == targetAnimation)
            {
                Unregister();
                return;
            }
        }

        private void Register()
        {
            if (registered)
            {
                return;
            }
            registered = true;

            Companion self = targetHandState.owner as Companion;
            self.AddModifyAnimationCallback(OnModifyAnimation);
            if (boundaryEase.getTarget() != 1.0f)
            {
                boundaryEase.StartBlend(1.0f, easeDuration);
            }
        }

        private void Unregister()
        {
            if (!registered)
            {
                return;
            }
            registered = false;

            if (boundaryEase.getTarget() != 0.0f)
            {
                boundaryEase.StartBlend(0.0f, easeDuration);
                Companion self = targetHandState.owner as Companion;
                self.onAnimationChange -= ListenForAnimation;
                listening = false;
            }
        }

        private void OnModifyAnimation()
        {
            boundaryEase.Update(Time.fixedDeltaTime);
            if (boundaryEase.value == 0.0f)
            {
                Companion self = targetHandState.owner as Companion;
                self.RemoveModifyAnimationCallback(OnModifyAnimation);
                targetHandState.RequestUnsetHoldBlendMax(holdBlendMax);
            }
            else
            {
                float desiredBlend = controlCallback(this);
                //negative values will cancel out regular hold retargeting
                if (desiredBlend < 0.0f)
                {
                    holdBlendMax.value = 1.0f - Mathf.Min(boundaryEase.value, -desiredBlend);
                    targetHandState.RequestSetHoldBlendMax(holdBlendMax);
                }
                else
                {
                    armIK.Apply(retargetingInfo, Mathf.Min(boundaryEase.value, desiredBlend));
                }
            }
        }
    }
}