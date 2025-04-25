#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// Displays a list of the clans the user sees e.g. is a member of or otherwise has a relationship with.
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/clan-list")]
    public class ClanList : MonoBehaviour
    {
        /// <summary>
        /// The types of clan list filters available
        /// </summary>
        public enum Filter
        {
            /// <summary>
            /// Any clan
            /// </summary>
            Any,
            /// <summary>
            /// Only official groups such as game groups
            /// </summary>
            OfficialGroups,
            /// <summary>
            /// Only publicly visible groups
            /// </summary>
            PublicGroups,
            /// <summary>
            /// Only non-official groups, so this will not show any game groups
            /// </summary>
            NonOfficialGroups,
            /// <summary>
            /// Only private groups
            /// </summary>
            PrivateGroups, 
            /// <summary>
            /// Only those that are followed by this user
            /// </summary>
            Followed
        }

        [SerializeField]
        private Filter filter = Filter.Any;
        [SerializeField]
        private Transform content;
        [SerializeField]
        private GameObject recordTemplate;
        /// <summary>
        /// Get or set the current filter being used
        /// </summary>
        public Filter ActiveFilter
        {
            get => filter;
            set
            {
                filter = value;
                UpdateDisplay();
            }
        }

        private Dictionary<ClanData, ClanProfile> records = new Dictionary<ClanData, ClanProfile>();

        private void OnEnable()
        {
            if (API.App.Initialized)
            {
                UpdateDisplay();
            }
            else
            {
                API.App.evtSteamInitialized.AddListener(DelayUpdate);
            }
        }

        private void OnDisable()
        {
            Clear();
        }

        private void DelayUpdate()
        {
            UpdateDisplay();
            API.App.evtSteamInitialized.RemoveListener(DelayUpdate);
        }

        private void Remove(ClanData clan)
        {
            if (records.ContainsKey(clan))
            {
                var target = records[clan];
                records.Remove(clan);
                Destroy(target.gameObject);
            }
        }

        private void Add(ClanData clan)
        {
            //Add the user and then resort the display
            if (!records.ContainsKey(clan))
            {
                AddNewRecord(clan);
                SortRecords();
            }
            else
                records[clan].Clan = clan;
        }

        private void AddNewRecord(ClanData clan)
        {
            var go = Instantiate(recordTemplate, content);
            var comp = go.GetComponent<ClanProfile>();
            comp.Clan = clan;
            records.Add(clan, comp);
        }

        private void SortRecords()
        {
            var keys = records.Keys.ToList();
            keys.Sort((a, b) => { return a.Name.CompareTo(b.Name); });

            foreach (var key in keys)
            {
                records[key].transform.SetAsLastSibling();
            }
        }
        /// <summary>
        /// Clear the list of clans
        /// </summary>
        public void Clear()
        {
            if (content.childCount > 0)
                try
                {
                    foreach (GameObject go in content)
                        try
                        {
                            Destroy(go);
                        }
                        catch { }
                }
                catch { }
        }
        /// <summary>
        /// Update the list of clans, this will clear any existing clans and rebuild the list from scratch
        /// </summary>
        public void UpdateDisplay()
        {
            Clear();


            List<ClanData> filtered = new List<ClanData>();
            var clans = new List<ClanData>(API.Clans.Client.GetClans());
            var followed = new List<ClanData>();

            API.Friends.Client.GetFollowed((r) =>
            {
                if(r != null && r.Length > 0)
                {
                    var subset = r.Where(p => p.GetEAccountType() == Steamworks.EAccountType.k_EAccountTypeClan);
                    if (subset.Count() > 0)
                    {
                        foreach (var id in subset)
                        {
                            clans.Add(id);
                            followed.Add(id);
                        }
                    }
                }

                if (filter == Filter.Followed)
                {
                    foreach (var clan in followed)
                        if (!records.ContainsKey(clan))
                            AddNewRecord(clan);
                }
                else
                {
                    foreach (var clan in clans)
                    {
                        if (MatchFilter(clan))
                            filtered.Add(clan);
                    }

                    foreach (var clan in filtered)
                        if (!records.ContainsKey(clan))
                            AddNewRecord(clan);
                }

                SortRecords();
            });
        }
        /// <summary>
        /// Compare a clan to the current filter
        /// </summary>
        /// <param name="clan"></param>
        /// <returns></returns>
        public bool MatchFilter(ClanData clan)
        {
            switch(filter)
            {
                case Filter.Any:
                    return true;
                case Filter.NonOfficialGroups:
                    return !clan.IsOfficialGameGroup;
                case Filter.OfficialGroups:
                    return clan.IsOfficialGameGroup;
                case Filter.PrivateGroups:
                    return !clan.IsPublic && !clan.IsOfficialGameGroup;
                case Filter.PublicGroups:
                    return clan.IsPublic;
                default:
                    return false;
            }
        }
    }
}
#endif