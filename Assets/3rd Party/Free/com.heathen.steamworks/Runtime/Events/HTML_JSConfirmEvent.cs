#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class HTML_JSConfirmEvent : UnityEvent<HTML_JSConfirm_t> { };
}
#endif