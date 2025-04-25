#if !DISABLESTEAMWORKS  && STEAMWORKSNET

namespace Heathen.SteamworksIntegration
{
    public struct ConsumeOrder
    {
        public Steamworks.SteamItemDetails_t detail;
        public uint quantity;
    }
}
#endif