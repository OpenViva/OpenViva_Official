#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// Displays a list of users that are in a specific Clan Chat
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/clan-chat-member-list")]
    public class ClanChatMemberList : MonoBehaviour
    {
        /// <summary>
        /// The ID of the clan to track
        /// </summary>
        [SerializeField]
        private ulong clanId;
        /// <summary>
        /// The GameObject that the UI elements representing each user should be spawned
        /// </summary>
        [SerializeField]
        private Transform content;
        /// <summary>
        /// The template that will be cloned for each user found
        /// </summary>
        [SerializeField]
        private GameObject template;
        /// <summary>
        /// The <see cref="ClanData"/> of the tracked clan
        /// </summary>
        public ClanData Clan
        {
            get => clanId;
            set => Apply(value);
        }

        private void Start()
        {
            if (clanId > 0)
                Apply(clanId);
        }
        /// <summary>
        /// Update the tracked clan
        /// </summary>
        /// <param name="clan">The clan to track</param>
        public void Apply(ClanData clan)
        {
            clanId = clan;
            Refresh();
        }
        /// <summary>
        /// Update the display of users
        /// </summary>
        public void Refresh()
        {
            foreach (GameObject obj in content)
                Destroy(obj);

            if (clanId > 0 && Clan.IsValid)
            {
                List<UserData> members = new List<UserData>();
                members.AddRange(API.Clans.Client.GetChatMembers(Clan));
                members.Sort((a, b) => { return a.Nickname.CompareTo(b.Nickname); });

                foreach(var user in members)
                {
                    var go = Instantiate(template, content);
                    var comp = go.GetComponent<IUserProfile>();
                    comp.Apply(user);
                }
            }
        }
    }
}
#endif