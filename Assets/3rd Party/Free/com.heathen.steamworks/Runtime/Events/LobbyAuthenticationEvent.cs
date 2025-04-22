#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class LobbyAuthenticationEvent : UnityEvent<LobbyData, UserData, byte[], byte[]> { }
}
#endif