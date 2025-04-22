#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class UserStatsUnloadedEvent : UnityEvent<UserStatsUnloaded_t> { }
}
#endif