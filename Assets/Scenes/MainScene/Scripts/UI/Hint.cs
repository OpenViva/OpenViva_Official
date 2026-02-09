public class Hint
{
    public enum HintType
    {
        GrabHint,
        LeftReleaseHint,
        RightReleaseHint,
        OpenBagHint,
        CloseBagHint,
        SelectItemsHint,
        TakeItemHint,
        LeftPlaceItemHint,
        RightPlaceItemHint,
        InteractHint,
        FlashlightHint
    };

    public HintType Type { get; set; }
    public string hintText;
}
