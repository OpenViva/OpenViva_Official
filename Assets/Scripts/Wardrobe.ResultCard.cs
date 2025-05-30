using UnityEngine;
using UnityEngine.UI;

namespace Viva
{


    public partial class Wardrobe : UITabMenu
    {

        [Header("Result Card UI")]

        [SerializeField]
        private GameObject resultTab;
        [SerializeField]
        private Text resultProgressText;
        [SerializeField]
        private Image resultCardImage;


        private Outfit CreatePhotoshootOutfit(Companion companion, ClothingPreset targetClothing, Outfit.ClothingOverride clothingOverride)
        {

            if (targetClothing == null)
            {
                Debug.LogError("ERROR targetClothing cannot be null!");
                return null;
            }
            if (clothingOverride == null)
            {
                Debug.LogError("ERROR material texture override is null for photoshoot!");
            }
            Outfit outfit = Outfit.Create(
                null,
                false
            );

            if (outfit == null)
            {
                return null;
            }
            switch (targetClothing.wearType)
            {
                case ClothingPreset.WearType.FULL_BODY:
                    outfit.RemoveAllClothingPieces(ClothingPreset.WearType.SKIRT);
                    break;
                case ClothingPreset.WearType.GROIN:
                    outfit.RemoveAllClothingPieces(ClothingPreset.WearType.SKIRT);
                    break;
            }

            outfit.WearClothingPiece(companion, targetClothing, clothingOverride, 5);

            return outfit;
        }
    }

}