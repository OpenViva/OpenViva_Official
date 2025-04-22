#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public class WorkshopItemInstalledEvent : UnityEvent<ItemInstalled_t>
    { }
}
#endif
