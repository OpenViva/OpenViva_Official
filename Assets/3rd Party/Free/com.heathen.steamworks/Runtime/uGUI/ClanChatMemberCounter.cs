#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine;
using Steamworks;
using System.Collections;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// Maintains a display of the number of users in a clan chat.
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/clan-chat-member-counter")]
    [RequireComponent(typeof(TMPro.TextMeshProUGUI))]
    public class ClanChatMemberCounter : MonoBehaviour
    {
        [SerializeField]
        private ulong clanId;
        [SerializeField]
        internal string prefix;
        [SerializeField]
        internal string suffix;
        /// <summary>
        /// Exposes the ClanData that this object is linked to and can be assigned to update which clan is monitored
        /// </summary>
        public ClanData Clan
        {
            get => clanId;
            set => Apply(value);
        }

        private TMPro.TextMeshProUGUI label;

        private void OnEnable()
        {
            label = GetComponent<TMPro.TextMeshProUGUI>();
            API.Clans.Client.EventGameConnectedChatJoin.AddListener(HandleJoin);
            API.Clans.Client.EventGameConnectedChatLeave.AddListener(HandleLeve);
        }

        private void Start()
        {
            if (API.App.Initialized)
            {
                if (clanId > 0)
                    Refresh();
            }
            else
            {
                API.App.evtSteamInitialized.AddListener(DelayUpdate);
            }
        }

        private void DelayUpdate()
        {
            if (clanId > 0)
                Refresh();

            API.App.evtSteamInitialized.RemoveListener(DelayUpdate);
        }

        private void HandleLeve(UserLeaveData data)
        {
            if (data.room.clan == clanId)
                Refresh();
        }

        private void HandleJoin(ChatRoom room, UserData user)
        {
            if (room.clan == clanId)
                Refresh();
        }
        /// <summary>
        /// Apply the clan, this is done for you when you assign the Clan field
        /// </summary>
        /// <param name="clan"></param>
        public void Apply(ClanData clan)
        {
            clanId = clan;
            Refresh();
        }
        /// <summary>
        /// Refresh the value, this updates the text label, this is in general called for you
        /// </summary>
        public void Refresh()
        {
            if (clanId > 0)
                label.text = prefix + API.Clans.Client.GetChatMemberCount(Clan).ToString() + suffix;
        }
    }
}
#endif