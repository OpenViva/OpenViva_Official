#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
#if STEAMWORKSNET
    [System.Serializable]
    public class LobbyEnterEvent : UnityEvent<LobbyEnter_t> { }
#elif FACEPUNCH
    [System.Serializable]
    public class LobbyEnterEvent : UnityEvent<Lobby> { }
#endif
}
#endif