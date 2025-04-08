#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class FavoritesListChangedEvent : UnityEvent<FavoritesListChanged_t> { }
}
#endif