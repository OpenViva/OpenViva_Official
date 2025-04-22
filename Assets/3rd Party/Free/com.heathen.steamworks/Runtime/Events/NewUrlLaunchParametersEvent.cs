#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
#if STEAMWORKSNET
    [System.Serializable]
    public class NewUrlLaunchParametersEvent : UnityEvent<NewUrlLaunchParameters_t> { }
#elif FACEPUNCH
    [System.Serializable]
    public class NewUrlLaunchParametersEvent : UnityEvent { }
#endif
}
#endif