#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    public struct ExchangeEntry
    {
        public SteamItemInstanceID_t instance;
        public uint quantity;
    }
}
#endif