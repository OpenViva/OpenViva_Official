using UnityEngine;

public class CropIndexes : MonoBehaviour
{
    // Itms indexes for instantiating crops.
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
            default: return "Error";
        }
    }
}
