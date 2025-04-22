#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct PartyBeaconDetails
    {
        public PartyBeaconID_t id;
        public UserData owner;
        public SteamPartyBeaconLocation_t location;
        public string metadata;
    }
    //*/

}
#endif