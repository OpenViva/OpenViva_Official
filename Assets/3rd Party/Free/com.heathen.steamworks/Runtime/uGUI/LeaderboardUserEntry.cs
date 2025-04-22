#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// Displays the local user's <see cref="LeaderboardEntry"/> for the given <see cref="LeaderboardObject"/>
    /// </summary>
    public class LeaderboardUserEntry : MonoBehaviour
    {
        /// <summary>
        /// Which board to display for
        /// </summary>
        public LeaderboardObject leaderboard;
        /// <summary>
        /// Label to display the score
        /// </summary>
        public TMPro.TextMeshProUGUI score;
        /// <summary>
        /// Label to display the rank
        /// </summary>
        public TMPro.TextMeshProUGUI rank;

        /// <summary>
        /// Get the entry from this board for this user
        /// </summary>
        public LeaderboardEntry Entry { get; private set; }

        private void Start()
        {
            leaderboard.UserEntryUpdated.AddListener(Refresh);
            //Refresh on a delay we do this to insure everything is loaded even if this is in the same scene as Steam Init
            Invoke(nameof(Refresh), 1.5f);
        }

        /// <summary>
        /// Refresh the local user's current entry for the confiured board
        /// </summary>
        public void Refresh()
        {
            leaderboard.GetUserEntry((entry, error) =>
            {
                if (!error && entry != null)
                {
                    Refresh(entry);
                }
            });
        }

        /// <summary>
        /// Can be used to set a specific entry to the display
        /// </summary>
        /// <param name="entry"></param>
        public void Refresh(LeaderboardEntry entry)
        {
            if (entry == null)
                return;

            Entry = entry;
            score.text = entry.Score.ToString();
            rank.text = entry.Rank.ToString();
        }
    }
}
#endif