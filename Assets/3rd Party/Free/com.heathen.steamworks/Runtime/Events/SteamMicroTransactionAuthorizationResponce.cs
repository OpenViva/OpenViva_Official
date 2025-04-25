#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
#if STEAMWORKSNET
    [System.Serializable]
    public class SteamMicroTransactionAuthorizationResponce : UnityEvent<AppId_t, ulong, bool>
    { }
#elif FACEPUNCH
    [System.Serializable]
    public class SteamMicroTransactionAuthorizationResponce : UnityEvent<AppId, ulong, bool>
    { }
#endif
}
#endif