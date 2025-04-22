#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct StringFilter
    {
        public string key;
        public string value;
        public ELobbyComparison comparison;
    }
}
#endif