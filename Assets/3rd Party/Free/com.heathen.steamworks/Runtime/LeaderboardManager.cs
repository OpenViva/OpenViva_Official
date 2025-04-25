#if !DISABLESTEAMWORKS  && STEAMWORKSNET
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    public class LeaderboardManager : MonoBehaviour
    {
        public enum ManagedEvents
        {
            QueryError,
            QueryCompleted,
            UploadError,
            UserEntryUpdated,
        }

        [SerializeField]
        private List<ManagedEvents> m_Delegates;

        public LeaderboardObject leaderboard;

        private LeaderboardEntry _lastKnownUserEntry;
        public LeaderboardEntry LastKnownUserEntry
        {
            get => _lastKnownUserEntry;
            private set
            {
                _lastKnownUserEntry = value;
                evtUserEntryUpdated.Invoke(value);
            }
        }

        public UserEntryEvent evtUserEntryUpdated = new UserEntryEvent();
        public EntryResultsEvent evtQueryCompleted = new EntryResultsEvent();
        public UnityEvent evtQueryError = new UnityEvent();
        public UnityEvent evtUploadError = new UnityEvent();

        public void RefreshUserEntry()
        {
            leaderboard.GetUserEntry((r, e) =>
            {
                if (!e)
                    LastKnownUserEntry = r;
                else
                    evtQueryError.Invoke();
            });
        }

        public void GetTopEntries(int count)
        {
            leaderboard.GetEntries(Steamworks.ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, count, (r, e) =>
            {
                if (!e)
                {
                    var user = r.FirstOrDefault(p => p.entry.m_steamIDUser == Steamworks.SteamUser.GetSteamID());
                    if (user != default)
                        LastKnownUserEntry = user;

                    evtQueryCompleted.Invoke(r);
                }
                else
                    evtQueryError?.Invoke();
            });
        }

        public void GetNearbyEntries(int beforeUser, int afterUser)
        {
            leaderboard.GetEntries(Steamworks.ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, -beforeUser, afterUser, (r, e) =>
            {
                if (!e)
                {
                    var user = r.FirstOrDefault(p => p.entry.m_steamIDUser == Steamworks.SteamUser.GetSteamID());
                    if (user != default)
                        LastKnownUserEntry = user;

                    evtQueryCompleted.Invoke(r);
                }
                else
                    evtQueryError?.Invoke();
            });
        }

        public void GetNearbyEntries(int aroundUser)
        {
            GetNearbyEntries(aroundUser, aroundUser);
        }

        public void GetAllFriendsEntries()
        {
            leaderboard.GetEntries(Steamworks.ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends, 0, 0, (r, e) =>
            {
                if (!e)
                {
                    var user = r.FirstOrDefault(p => p.entry.m_steamIDUser == Steamworks.SteamUser.GetSteamID());
                    if (user != default)
                        LastKnownUserEntry = user;

                    evtQueryCompleted.Invoke(r);
                }
                else
                    evtQueryError?.Invoke();
            });
        }

        public void GetUserEntries(IEnumerable<UserData> users)
        {
            leaderboard.GetEntries(users.ToArray(), (r, e) =>
            {
                if (!e)
                {
                    var user = r.FirstOrDefault(p => p.entry.m_steamIDUser == Steamworks.SteamUser.GetSteamID());
                    if (user != default)
                        LastKnownUserEntry = user;

                    evtQueryCompleted.Invoke(r);
                }
                else
                    evtQueryError?.Invoke();
            });
        }

        public void UploadScore(int score)
        {
            leaderboard.UploadScore(score, Steamworks.ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, (r, e) =>
            {
                if (!e)
                {
                    RefreshUserEntry();
                }
                else
                    evtUploadError.Invoke();
            });
        }

        public void UploadScore(int score, int[] details)
        {
            leaderboard.UploadScore(score, details, Steamworks.ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, (r, e) =>
            {
                if (!e)
                {
                    RefreshUserEntry();
                }
                else
                    evtUploadError.Invoke();
            });
        }

        public void ForceScore(int score)
        {
            leaderboard.UploadScore(score, Steamworks.ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, (r, e) =>
            {
                if (!e)
                {
                    RefreshUserEntry();
                }
                else
                    evtUploadError.Invoke();
            });
        }

        public void ForceScore(int score, int[] details)
        {
            leaderboard.UploadScore(score, details, Steamworks.ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, (r, e) =>
            {
                if (!e)
                {
                    RefreshUserEntry();
                }
                else
                    evtUploadError.Invoke();
            });
        }

        [Serializable]
        public class UserEntryEvent : UnityEvent<LeaderboardEntry> { }
        [Serializable]
        public class EntryResultsEvent : UnityEvent<LeaderboardEntry[]> { }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LeaderboardManager), true)]
    public class LeaderboardManagerEditor : Editor
    {
        SerializedProperty m_DelegatesProperty;
        SerializedProperty m_leaderboard;

        GUIContent m_IconToolbarMinus;
        GUIContent m_EventIDName;
        GUIContent[] m_EventTypes;
        GUIContent m_AddButtonContent;

        protected virtual void OnEnable()
        {
            m_DelegatesProperty = serializedObject.FindProperty("m_Delegates");
            m_leaderboard = serializedObject.FindProperty("leaderboard");
            m_AddButtonContent = new GUIContent("Add New Event Type");
            m_EventIDName = new GUIContent("");
            // Have to create a copy since otherwise the tooltip will be overwritten.
            m_IconToolbarMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"));
            m_IconToolbarMinus.tooltip = "Remove all events in this list.";

            string[] eventNames = Enum.GetNames(typeof(LeaderboardManager.ManagedEvents));
            m_EventTypes = new GUIContent[eventNames.Length];
            for (int i = 0; i < eventNames.Length; ++i)
            {
                m_EventTypes[i] = new GUIContent(eventNames[i]);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_leaderboard, true);

            int toBeRemovedEntry = -1;

            EditorGUILayout.Space();

            Vector2 removeButtonSize = GUIStyle.none.CalcSize(m_IconToolbarMinus);

            for (int i = 0; i < m_DelegatesProperty.arraySize; ++i)
            {
                SerializedProperty delegateProperty = m_DelegatesProperty.GetArrayElementAtIndex(i);
                m_EventIDName.text = delegateProperty.enumDisplayNames[delegateProperty.enumValueIndex];

                switch ((LeaderboardManager.ManagedEvents)delegateProperty.enumValueIndex)
                {
                    case LeaderboardManager.ManagedEvents.QueryCompleted:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LeaderboardManager.evtQueryCompleted)), m_EventIDName);
                        break;
                    case LeaderboardManager.ManagedEvents.QueryError:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LeaderboardManager.evtQueryError)), m_EventIDName);
                        break;
                    case LeaderboardManager.ManagedEvents.UploadError:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LeaderboardManager.evtUploadError)), m_EventIDName);
                        break;
                    case LeaderboardManager.ManagedEvents.UserEntryUpdated:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LeaderboardManager.evtUserEntryUpdated)), m_EventIDName);
                        break;
                }

                Rect callbackRect = GUILayoutUtility.GetLastRect();

                Rect removeButtonPos = new Rect(callbackRect.xMax - removeButtonSize.x - 8, callbackRect.y + 1, removeButtonSize.x, removeButtonSize.y);
                if (GUI.Button(removeButtonPos, m_IconToolbarMinus, GUIStyle.none))
                {
                    toBeRemovedEntry = i;
                }

                EditorGUILayout.Space();
            }

            if (toBeRemovedEntry > -1)
            {
                RemoveEntry(toBeRemovedEntry);
            }

            Rect btPosition = GUILayoutUtility.GetRect(m_AddButtonContent, GUI.skin.button);
            const float addButtonWidth = 200f;
            btPosition.x = btPosition.x + (btPosition.width - addButtonWidth) / 2;
            btPosition.width = addButtonWidth;
            if (GUI.Button(btPosition, m_AddButtonContent))
            {
                ShowAddTriggerMenu();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveEntry(int toBeRemovedEntry)
        {
            m_DelegatesProperty.DeleteArrayElementAtIndex(toBeRemovedEntry);
        }

        void ShowAddTriggerMenu()
        {
            // Now create the menu, add items and show it
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < m_EventTypes.Length; ++i)
            {
                bool active = true;

                // Check if we already have a Entry for the current eventType, if so, disable it
                for (int p = 0; p < m_DelegatesProperty.arraySize; ++p)
                {
                    SerializedProperty delegateEntry = m_DelegatesProperty.GetArrayElementAtIndex(p);
                    if (delegateEntry.enumValueIndex == i)
                    {
                        active = false;
                    }
                }
                if (active)
                    menu.AddItem(m_EventTypes[i], false, OnAddNewSelected, i);
                else
                    menu.AddDisabledItem(m_EventTypes[i]);
            }
            menu.ShowAsContext();
            Event.current.Use();
        }

        private void OnAddNewSelected(object index)
        {
            int selected = (int)index;

            m_DelegatesProperty.arraySize += 1;
            SerializedProperty delegateEntry = m_DelegatesProperty.GetArrayElementAtIndex(m_DelegatesProperty.arraySize - 1);
            delegateEntry.enumValueIndex = selected;
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif