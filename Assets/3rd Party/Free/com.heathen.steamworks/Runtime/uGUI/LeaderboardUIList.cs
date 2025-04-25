#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// The Leaderboard UI List is a simple tool that can be used to display LeaderboardEntry records. The Leaderboard UI List can be used in conjunction with the Leaderboard Manager to display Leaderboards in your UI with a zero code setup as is demonstrated in the Leaderboard Sample Scene.
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/leaderboard-ui-list")]
    public class LeaderboardUIList : MonoBehaviour
    {
        /// <summary>
        /// This will be the parent of any records instantiated by the tool. Most often you would set this to be the Content GameObject of a ScrollRect or similar.
        /// </summary>
        public Transform collection;
        /// <summary>
        /// This is the "template" that will instantiated for each record the list displays. This template should implement a component that inherits from <see cref="ILeaderboardEntryDisplay"/>. You can either create your own UI Control script and implement the <see cref="ILeaderboardEntryDisplay"/> interface or you can use the <see cref="LeaderboardEntryUIRecord"/> we provide which has a basic implementation already done.
        /// </summary>
        public GameObject template;

        /// <summary>
        /// This is called when the list is enabled and can be useful when you want to refresh a query when enabling (showing) the UI element.
        /// </summary>
        [Header("Events")]
        public UnityEvent Enabled;

        private List<GameObject> createdRecords = new List<GameObject>();

        private void OnEnable()
        {
            Enabled.Invoke();
        }

        /// <summary>
        /// <para>
        /// Calling this method will cause the Leaderboard UI List to clear any currently displayed records and to instantiate the "Template" for each entry passed in. It will attempt to get the <see cref="ILeaderboardEntryDisplay"/> component on the Template and set it's Entry field.
        /// </para>
        /// <para>
        /// You can connect this method to the <see cref="LeaderboardManager.evtQueryCompleted"/> to automatically display the results of any query ran on the manager. Doing this will give you a "code free" solution for displaying leaderboard entries.
        /// </para>
        /// </summary>
        /// <param name="entries"></param>
        public void Display(LeaderboardEntry[] entries)
        {
            foreach(var entry in createdRecords)
            {
                Destroy(entry);
            }
            createdRecords.Clear();

            foreach(var entry in entries)
            {
                var go = Instantiate(template, collection);
                createdRecords.Add(go);

                var display = go.GetComponent<ILeaderboardEntryDisplay>();
                if (display != null)
                    display.Entry = entry;
            }
        }
    }
}
#endif