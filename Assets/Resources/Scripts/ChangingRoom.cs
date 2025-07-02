using UnityEngine;

namespace Viva
{


    public class ChangingRoom : Mechanism
    {

        [SerializeField]
        private ChangingRoomBasket[] baskets;


        public ChangingRoomBasket GetNextEmptyBasket()
        {
            foreach (var basket in baskets)
            {
                if (basket.outfit == null)
                {
                    return basket;
                }
            }
            return null;
        }

        public override bool AttemptCommandUse(Companion targetCompanion, Character commandSource)
        {
            if (targetCompanion == null)
            {
                return false;
            }
            return targetCompanion.active.onsenSwimming.AttemptChangeOutOfSwimmingClothes();
        }

        public override void EndUse(Character targetCharacter)
        {

        }
    }

}