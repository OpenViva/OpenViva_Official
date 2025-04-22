#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct LeaderboardScores
    {
        public bool bIOFailure;
        public bool playerIncluded;
        public List<LeaderboardEntry> scoreData;
    }
}
#endif
