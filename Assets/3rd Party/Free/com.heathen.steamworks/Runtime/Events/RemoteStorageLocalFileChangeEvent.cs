#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class RemoteStorageLocalFileChangeEvent : UnityEvent<RemoteStorageLocalFileChange_t> { }
}
#endif