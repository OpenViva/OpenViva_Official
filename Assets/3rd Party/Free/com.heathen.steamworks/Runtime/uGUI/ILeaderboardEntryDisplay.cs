#if !DISABLESTEAMWORKS  && STEAMWORKSNET

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Used by the Leaderboard UI List when instantiating records. This can be inherited from to create custom Leaderboard Entry Display's similar to the Leaderboard Display Entry provided in the asset.
    /// </summary>
    public interface ILeaderboardEntryDisplay
    {
        /// <summary>
        /// This will be set by the Leaderboard UI List when instantiating a record and should update the UI elements of the entry accordingly.
        /// </summary>
        LeaderboardEntry Entry { get; set; }
    }
}
#endif