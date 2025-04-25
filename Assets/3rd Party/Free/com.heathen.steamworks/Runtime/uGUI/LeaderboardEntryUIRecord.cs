#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// A simple implementation of the <see cref="ILeaderboardEntryDisplay"/> interface.
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/leaderboard-entry-ui-record")]
    public class LeaderboardEntryUIRecord : MonoBehaviour, ILeaderboardEntryDisplay
    {
        [SerializeField]
        private SetUserAvatar avatar;
        [SerializeField]
        private SetUserName userName;
        [SerializeField]
        private TMPro.TextMeshProUGUI score;
        [SerializeField]
        private TMPro.TextMeshProUGUI rank;

        private LeaderboardEntry _entry;
        /// <summary>
        /// Read or write the <see cref="LeaderboardEntry"/> for this object
        /// </summary>
        public LeaderboardEntry Entry 
        { 
            get => _entry;
            set => SetEntry(value);
        }

        private void SetEntry(LeaderboardEntry entry)
        {
            avatar.UserData = entry.User;
            userName.UserData = entry.User;
            score.text = entry.Score.ToString();
            rank.text = entry.Rank.ToString();

            _entry = entry;
        }
    }
}
#endif