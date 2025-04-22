#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class LobbyAuthenticaitonSessionEvent : UnityEvent<AuthenticationSession, byte[]> { }
}
#endif