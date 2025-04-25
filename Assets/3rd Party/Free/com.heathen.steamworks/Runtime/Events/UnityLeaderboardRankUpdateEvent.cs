#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public class UnityLeaderboardRankUpdateEvent : UnityEvent<LeaderboardEntry>
    { }
}
#endif