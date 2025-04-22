#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct InventoryResult
    {
        public ItemDetail[] items;
        public EResult result;
        public DateTime timestamp;
    }
}
#endif