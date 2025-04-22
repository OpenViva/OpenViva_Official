#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct ItemProperty
    {
        public string key;
        public string value;
    }
}
#endif