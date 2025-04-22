#if !DISABLESTEAMWORKS  && STEAMWORKSNET

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Enumerator used in Game Server Browser searches
    /// </summary>
    public enum GameServerSearchType
    {
        Internet,
        Friends,
        Favorites,
        LAN,
        Spectator,
        History
    }
}
#endif