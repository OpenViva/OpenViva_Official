using UnityEngine;

public class ItemIndexes
{

    public int GetItemIndex(string name)
    {
        return name switch
        {
            "rubberDucky" => 1,
            "peach" => 2,
            "strawberry" => 3,
            "cantaloupe" => 4,
            "blueberry" => 5,
            "wheatSpike" => 6,
            "flashlight" => 7,
            "egg" => 8,
            "flourJar" => 9,
            "knife" => 10,
            "lantern" => 11,
            "milkCanisterHanger" => 12,
            "mixingBowl" => 13,
            "mixingSpoon" => 14,
            "mortar" => 15,
            "pestle" => 16,
            "pot" => 17,
            "soap" => 18,
            "towel" => 19,
            "dough" => 20,
            "bread" => 21,
            "toast" => 22,
            "frenchToast" => 23,
            "burntBread" => 24,
            _ => -1,
        };
    }

    public string GetItemName(int index)
    {
        return index switch
        {
            1 => "Rubber Ducky",
            2 => "Peach",
            3 => "Strawberry",
            4 => "Cantaloupe",
            5 => "Blueberry",
            6 => "WheatIntoMortar",
            7 => "Flashlight",
            8 => "Egg",
            9 => "Flour Jar",
            10 => "Knife",
            11 => "Lantern",
            12 => "Milk Canister",
            13 => "Mixing Bowl",
            14 => "Mixing Spoon",
            15 => "MortarLogic",
            16 => "Pestle",
            17 => "Pot",
            18 => "Soap",
            19 => "Towel",
            20 => "Dough",
            21 => "Bread",
            22 => "Toast",
            23 => "French Toast",
            24 => "Burnt Bread",
            _ => "Error",
        };
    }
}
