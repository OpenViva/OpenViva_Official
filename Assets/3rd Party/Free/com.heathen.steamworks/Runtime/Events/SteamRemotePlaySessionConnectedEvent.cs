#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class SteamRemotePlaySessionConnectedEvent : UnityEvent<SteamRemotePlaySessionConnected_t> { }
}
#endif