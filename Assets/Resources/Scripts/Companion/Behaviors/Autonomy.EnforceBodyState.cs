using System.Collections.Generic;


namespace Viva
{


    public class AutonomyEnforceBodyState : Autonomy.Task
    {

        public BodyState targetBodyState { get; private set; }
        private List<Companion.Animation> transferAnims = new List<Companion.Animation>();


        public AutonomyEnforceBodyState(Autonomy _autonomy, string _name, BodyState _targetBodyState) : base(_autonomy, _name)
        {
            targetBodyState = _targetBodyState;

            onFixedUpdate += OnFixedUpdate;
            onRegistered += RecalculateTransferAnims;
            onRegistered += delegate { self.OnBodyStateChanged += OnBodyStateChanged; };
            onUnregistered += delegate { self.OnBodyStateChanged -= OnBodyStateChanged; };
        }

        public void SetTargetBodyState(BodyState _targetBodyState)
        {
            if (targetBodyState == _targetBodyState)
            {
                return;
            }
            targetBodyState = _targetBodyState;
            RecalculateTransferAnims();
        }

        private void OnBodyStateChanged(BodyState oldBodyState, BodyState newBodyState)
        {
            RecalculateTransferAnims();
        }

        private void RecalculateTransferAnims()
        {
            Companion.FindBodyStatePath(self, transferAnims, self.bodyState, targetBodyState);
        }

        public override bool? Progress()
        {
            if (self.bodyState == targetBodyState)
            {
                return true;
            }
            return null;
        }

        public void OnFixedUpdate()
        {
            if (transferAnims.Count == 0)
            {
                return;
            }
            if (self.IsCurrentAnimationIdle())
            {

                var exitAnim = transferAnims[0];
                if (exitAnim != Companion.Animation.NONE)
                {
                    self.SetTargetAnimation(exitAnim);
                }
            }
        }
    }

}