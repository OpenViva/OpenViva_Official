#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;
using UnityEditor;
using UnityEngine;

namespace Heathen.SteamworksIntegration.Editors
{
    public class LobbyIMGUITool
    {
        public Texture icon;
        public Texture2D userBackground;

        private int lobbyIndex = 0;
        private Vector2 scrollPos;

        public void OnGUI()
        {
            if (Application.isPlaying)
            {
                if (API.Matchmaking.Client.memberOfLobbies != null)
                {
                    if (API.Matchmaking.Client.memberOfLobbies.Count > 0)
                    {
                        DrawWindow();
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Steamworks Lobby Tools is initialized!\n No lobbies currently connected.", MessageType.Info);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Steamworks Lobby Tools is initialized however the lobbies collection is null, this indicates an issue with the initalizaiton process, please contact Heathen Support on Discord.", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("The lobby inspector only works in play mode and when the Steamworks Lobby Tools system has been initialized.", MessageType.Info);
            }
        }

        private void DrawWindow()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            ListLobbies();
            EditorGUILayout.EndHorizontal();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            LobbyDetails(API.Matchmaking.Client.memberOfLobbies[lobbyIndex]);
            EditorGUILayout.EndScrollView();
        }

        private void ListLobbies()
        {
            for (int i = 0; i < API.Matchmaking.Client.memberOfLobbies.Count; i++)
            {
                var lobby = API.Matchmaking.Client.memberOfLobbies[i];

                if (DrawLobbyEntry(lobby, lobbyIndex == i))
                    lobbyIndex = i;
            }
        }

        private void LobbyDetails(LobbyData lobby)
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Lobby Name: ");
            EditorGUILayout.SelectableLabel(lobby.Name, GUILayout.Height(15));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Lobby ID: ");
            EditorGUILayout.SelectableLabel(lobby.ToString(), GUILayout.Height(18));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            var entries = lobby.GetMetadata();
            EditorGUILayout.LabelField("Metadata: (" + entries.Count + ")");

            EditorGUI.indentLevel++;
            
            foreach (var data in entries)
            {
                EditorGUILayout.SelectableLabel(data.Key + " : " + data.Value, GUILayout.Height(18));
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            var members = lobby.Members;
            EditorGUILayout.LabelField("Members: (" + members.Length + ")");

            EditorGUI.indentLevel++;

            foreach (var member in members)
            {
                ListUserDetails(member);
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();

        }

        private void ListUserDetails(LobbyMemberData member)
        {
            EditorGUILayout.BeginHorizontal();
            member.user.LoadAvatar(Avatar =>
            {
                GUILayout.Box(Avatar, GUILayout.Width(64), GUILayout.Height(64));
            });

            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("User ID: ");
            EditorGUILayout.SelectableLabel(member.user.id.ToString(), GUILayout.Height(18));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Display Name: ");
            EditorGUILayout.SelectableLabel(member.user.Name, GUILayout.Height(18));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("User Level: ");
            EditorGUILayout.SelectableLabel(member.user.Level.ToString(), GUILayout.Height(18));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Is Ready: ");
            EditorGUILayout.SelectableLabel(member.IsReady.ToString(), GUILayout.Height(18));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Game Version: ");
            EditorGUILayout.SelectableLabel(member.GameVersion.ToString(), GUILayout.Height(18));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private bool DrawLobbyEntry(LobbyData lobby, bool selected)
        {
            var typeString = lobby[LobbyData.DataType];
            if (string.IsNullOrEmpty(typeString))
                typeString = "Unknown Type";

            return GUILayout.Toggle(selected, typeString + ": " + lobby.ToString(), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
        }
    }
}
#endif