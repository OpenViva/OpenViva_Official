#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct LeaderboardScoreUploaded
    {
        public Steamworks.LeaderboardScoreUploaded_t data;
        public readonly bool Success => data.m_bSuccess != 0;
        public readonly bool ScoreChanged => data.m_bScoreChanged != 0;
        public readonly LeaderboardData Leaderboard => data.m_hSteamLeaderboard;
        public readonly int Score => data.m_nScore;
        public readonly int GlobalRankNew => data.m_nGlobalRankNew;
        public readonly int GlobalRankPrevious => data.m_nGlobalRankPrevious;

        public static implicit operator LeaderboardScoreUploaded(LeaderboardScoreUploaded_t native) => new LeaderboardScoreUploaded { data = native };
        public static implicit operator LeaderboardScoreUploaded_t(LeaderboardScoreUploaded heathen) => heathen.data;
    }
}
#endif