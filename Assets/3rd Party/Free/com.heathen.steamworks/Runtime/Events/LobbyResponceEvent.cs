#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class LobbyResponceEvent : UnityEvent<Steamworks.EChatRoomEnterResponse> { }

    [System.Serializable]
    public class EResultEvent : UnityEvent<Steamworks.EResult> { }
}
#endif