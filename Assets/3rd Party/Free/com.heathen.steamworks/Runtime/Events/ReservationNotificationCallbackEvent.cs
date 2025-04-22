#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class ReservationNotificationCallbackEvent : UnityEvent<ReservationNotificationCallback_t> { }
}
#endif