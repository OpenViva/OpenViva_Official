#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
#if STEAMWORKSNET
    [System.Serializable]
    public class SteamInventoryResultReadyEvent : UnityEvent<InventoryResult> { }
#elif FACEPUNCH
    [System.Serializable]
    public class SteamInventoryResultReadyEvent : UnityEvent<InventoryItem[]> { }
#endif
}
#endif