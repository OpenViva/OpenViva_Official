using UnityEngine;


namespace Viva
{


    public partial class BegBehavior : ActiveBehaviors.ActiveTask
    {


        private Vector3 begTargetPos = Vector3.zero;
        private Item targetItem = null;

        public BegBehavior(Companion _self) : base("Begging", _self, ActiveBehaviors.Behavior.BEG, null)
        {
            self.onGiftItemCallstack.AddCallback(OnItemBeg);
        }

        private bool OnItemBeg(Item item)
        {
            Viva.DevTools.LogExtended("Begging for Item " + item, true, true);
            if (self.active.currentTask == this)
            {
                return false;
            }
            //cannot beg for item without an owner
            if (item.mainOwner == null)
            {
                Debug.LogError("Cannot beg for item without an owner");
                return false;
            }
            float sqDist = Vector3.SqrMagnitude(self.head.position - item.transform.position);
            if (sqDist > 24.0f)
            {
                Debug.LogWarning("Too far away to beg for item");
                return false;
            }
            if (self.rightHandState.occupied && self.leftHandState.occupied)
            {
                return false;
            }
            targetItem = item;
            self.active.SetTask(this, null);
            PreBegAction();
            return true;
        }

        private void PreBegAction()
        {
            self.SetLookAtTarget(targetItem.transform, 2.4f);

            //random chance startle
            if (Random.value > 0.5f && self.IsHappy())
            {
                var startleFirst = new AutonomyPlayAnimation(self.autonomy, "startle before pickup", Companion.Animation.STAND_GIDDY_SURPRISE);
                startleFirst.AddRequirement(new AutonomyEnforceBodyState(self.autonomy, "enforce stand", BodyState.STAND));
                startleFirst.AddPassive(new AutonomyFaceDirection(self.autonomy, "face beg item", delegate (TaskTarget target) { target.SetTargetItem(targetItem); }));

                self.autonomy.SetAutonomy(startleFirst);

                startleFirst.onSuccess += BeginBegForItem;
            }
            else
            {
                BeginBegForItem();
            }
        }

        private void BeginBegForItem()
        {
            if (targetItem == null)
            {
                ConfusedAndIdle();
                return;
            }

            var beg = new AutonomyPickup(self.autonomy, "beg pickup", targetItem, self.GetPreferredHandState(targetItem));
            beg.onSuccess += OnSucceedPickup;
            beg.onFail += ConfusedAndIdle;

            self.autonomy.SetAutonomy(beg);
        }

        private void OnSucceedPickup()
        {
            Viva.DevTools.LogExtended("Pickup successful, targetItem: " + targetItem, true, true);
            if (targetItem == null)
            {
                return;
            }
            if(targetItem.name.Contains("Ducky"))
                Tutorial.instance.ContinueTutorial(MenuTutorial.WAIT_TO_GIVE_DUCKY);
            Debug.Log(self.headModel.name + " picked up " + targetItem.name);
            self.SetLookAtTarget(null);

            var playEndBegAnim = new AutonomyPlayAnimation(self.autonomy, "end beg anim", Companion.Animation.STAND_HAPPY_BEG_END);
            self.autonomy.SetAutonomy(playEndBegAnim);

            //playEndBegAnim.onSuccess += PlayPostPickupAnim;
            playEndBegAnim.onFail += ConfusedAndIdle;

            self.active.SetTask(self.active.idle);
        }

        // private void PlayPostPickupAnim()
        // {
        //     var postPickupAnim = Companion.Animation.NONE;// self.active.pickup.GetPostPickupAnimationByItemType( targetItem.settings.itemType );
        //     var playPostPickupAnim = new AutonomyPlayAnimation(self.autonomy, "end beg anim", postPickupAnim);
        //
        //     self.autonomy.SetAutonomy(playPostPickupAnim);
        // }

        private void ConfusedAndIdle()
        {
            var playAnim = CompanionUtility.CreateSpeechAnimation(self, AnimationSet.CONFUSED, SpeechBubble.INTERROGATION);
            playAnim.onSuccess += delegate { self.active.SetTask(self.active.idle); };
            self.autonomy.SetAutonomy(playAnim);
        }
    }

}