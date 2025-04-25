#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct LobbyGameServer
    {
        public CSteamID id;
        public string IpAddress
        {
            get => API.Utilities.IPUintToString(ipAddress);
            set => ipAddress = API.Utilities.IPStringToUint(value);
        }
        public uint ipAddress;
        public ushort port;
    }
    //*/

}
#endif