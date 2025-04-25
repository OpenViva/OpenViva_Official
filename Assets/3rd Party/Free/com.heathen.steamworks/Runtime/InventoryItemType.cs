#if !DISABLESTEAMWORKS  && STEAMWORKSNET

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// The type applied to a Steam Inventory Item
    /// </summary>
    public enum InventoryItemType
    {
        item,
        bundle,
        generator,
        playtimegenerator,
        tag_generator,
    }
}
#endif