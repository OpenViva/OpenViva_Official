#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System.Collections;
using UnityEngine;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// Can be connected to a GameObject along with the Clan Chat Director to cause the system to automatically join the clan chat on start up.
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/chat-auto-join")]
    [RequireComponent(typeof(ClanChatDirector))]
    public class ChatAutoJoin : MonoBehaviour
    {
        /// <summary>
        /// The ID of the clan that should be joined, this is a ulong value that can usually be found as part of the URL of a clan or similar. You could optionally search for clans using the Clan API and use the ID found there.
        /// This field is private but can be set in the Unity Editor.
        /// </summary>
        [SerializeField]
        private ulong clanId;

        private void Start()
        {
            if (API.App.Initialized)
            {
                var director = GetComponent<ClanChatDirector>();
                director.Join(clanId);
            }
            else
            {
                API.App.evtSteamInitialized.AddListener(DelayUpdate);
            }
        }

        private void DelayUpdate()
        {
            var director = GetComponent<ClanChatDirector>();
            director.Join(clanId);

            API.App.evtSteamInitialized.RemoveListener(DelayUpdate);
        }
    }
}
#endif