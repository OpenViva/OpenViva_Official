#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class ScreenshotRequestedEvent : UnityEvent<ScreenshotRequested_t> { }
}
#endif