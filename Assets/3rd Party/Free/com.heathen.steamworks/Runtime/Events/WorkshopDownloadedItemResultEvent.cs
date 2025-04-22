#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public class WorkshopDownloadedItemResultEvent : UnityEvent<DownloadItemResult_t>
    { }
}
#endif
