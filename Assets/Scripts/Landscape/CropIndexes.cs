using UnityEngine;

public class CropIndexes : MonoBehaviour
{
    // Items indexes for instantiating crops.
    public int GetItemIndex(string name)
    {
        switch (name)
        {
            case "rubberDucky": return 0;
            case "peach(Clone)": return 1;
            case "strawberry(Clone)": return 2;
            case "cantaloupe(Clone)": return 3;
            case "blueberry(Clone)": return 4;
            case "wheatSpike(Clone)": return 5;
            case "flashlight": return 6;
            case "flourJar": return 7;
            case "knife": return 8;
            case "milkCanisterHanger": return 9;
            case "mixingBowl": return 10;
            case "mixingSpoon": return 11;
            case "mortar": return 12;
            case "pestle": return 13;
            case "pot": return 14;
            case "soap": return 15;
            case "towel": return 16;
            case "egg": return 17;
            case "lantern": return 18;
            default: return -1;
        }
    }

    public string GetItemName(int index)
    {
        switch (index)
        {
            case 0: return "Rubber Ducky";
            case 1: return "Peach";
            case 2: return "Strawberry";
            case 3: return "Cantaloupe";
            case 4: return "Blueberry";
            case 5: return "Wheat";
            case 6: return "Flashlight";
            case 7: return "Flour Jar";
            case 8: return "Knife";
            case 9: return "Milk Canister";
            case 10: return "Mixing Bowl";
            case 11: return "Mixing Spoon";
            case 12: return "Mortar";
            case 13: return "Pestle";
            case 14: return "Pot";
            case 15: return "Soap";
            case 16: return "Towel";
            case 17: return "Egg";
            case 18: return "Lantern";
            default: return "Error";
        }
    }
}
