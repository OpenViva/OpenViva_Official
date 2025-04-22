#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using Friends = Heathen.SteamworksIntegration.API.Friends;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// Displays the image, name and tag of a clan
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/clan-profile")]
    public class ClanProfile : MonoBehaviour
    {
        /// <summary>
        /// Displays the Steam Avatar/Icon related to this clan
        /// </summary>
        [SerializeField]
        private RawImage icon;
        /// <summary>
        /// Displays the name of the clan
        /// </summary>
        [SerializeField]
        private TMPro.TextMeshProUGUI displayName;
        /// <summary>
        /// Displays teh tag for the clan
        /// </summary>
        [SerializeField]
        private TMPro.TextMeshProUGUI clanTag;

        /// <summary>
        /// The clan this is displaying
        /// </summary>
        public ClanData Clan
        {
            get
            {
                return currentClan;
            }
            set
            {
                Apply(value);
            }
        }

        private ClanData currentClan;

        private void OnEnable()
        {
            Friends.Client.EventPersonaStateChange.AddListener(HandlePersonaStateChange);
        }

        private void OnDisable()
        {
            Friends.Client.EventPersonaStateChange.RemoveListener(HandlePersonaStateChange);
        }

        private void HandlePersonaStateChange(PersonaStateChange arg)
        {
            if (Friends.Client.PersonaChangeHasFlag(arg.Flags, EPersonaChange.k_EPersonaChangeAvatar)
                && arg.SubjectId == currentClan)
            {
                Apply(currentClan);
            }
        }
        /// <summary>
        /// Set the clan to be displayed
        /// </summary>
        /// <param name="clan"></param>
        public void Apply(ClanData clan)
        {
            currentClan = clan;

            if (displayName != null)
                displayName.text = clan.Name;

            if (clanTag != null)
                clanTag.text = clan.Tag;

            if (icon != null)
                clan.LoadIcon(r => icon.texture = r);
        }
    }
}
#endif