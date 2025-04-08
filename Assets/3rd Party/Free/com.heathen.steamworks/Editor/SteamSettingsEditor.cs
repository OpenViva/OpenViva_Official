#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Heathen.SteamworksIntegration.Editors
{
    [CustomEditor(typeof(SteamSettings))]
    public class SteamSettingsEditor : Editor
    {
        private SteamSettings settings;
        private bool sgsFoldout = false;
        bool needRefresh = false;
        private static bool achievements = false;
        private static bool inventoryFoldout = false;
        private static bool inputFoldout = false;
        private static bool inputActionFoldout = false;
        private static bool inputActionSetFoldout = false;
        private static bool inputActionSetLayerFoldout = false;
        private static bool itemsFoldout = false;
        private static bool bundlesFoldout = false;
        private static bool generatorFoldout = false;
        private static bool playtimegeneratorFoldout = false;
        private static bool taggeneratorFoldout = false;
        private static bool dlcFoldout = false;
        private static bool artifactFoldout = false;
        private static bool statsFoldout = false;
        private static bool leaderboardFoldout = false;
        private static string tempAchName = "";

        public override void OnInspectorGUI()
        {
            settings = target as SteamSettings;

            if (needRefresh)
            {
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings), ImportAssetOptions.ForceUpdate);
                needRefresh = false;
            }

            ValidationChecks();
            DrawCommonSettings();
            DrawServerSettings();

        }

        private void ValidationChecks()
        {
            if (settings.server == null)
            {
                settings.server = new SteamSettings.GameServer();
                EditorUtility.SetDirty(settings);
            }

            if (settings.client == null)
            {
                settings.client = new SteamSettings.GameClient();
                EditorUtility.SetDirty(settings);
            }
        }

        private void DrawCommonSettings()
        {
            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Steamworks Portal"))
            {
                Application.OpenURL("https://partner.steamgames.com/apps/landing/" + settings.applicationId.ToString());
            }
            if (EditorGUILayout.LinkButton("Knowledge Base"))
            {
                Application.OpenURL("https://kb.heathen.group/toolkit-for-steamworks/steamworks");
            }
            if (EditorGUILayout.LinkButton("Support"))
            {
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            }
            if (EditorGUILayout.LinkButton("Leave a Review"))
            {
                Application.OpenURL("https://assetstore.unity.com/publishers/5836");
            }
            EditorGUILayout.EndHorizontal();
            
            var debug = GUILayout.Toggle(settings.isDebugging, "Enable Debug Messages");
            if(settings.isDebugging != debug)
            {
                Undo.RecordObject(target, "editor");
                settings.isDebugging = debug;
                UnityEditor.EditorUtility.SetDirty(target);
            }

            var id = EditorGUILayout.TextField("Application Id", settings.applicationId.ToString());
            uint buffer = 0;
            if (uint.TryParse(id, out buffer))
            {
                if (buffer != settings.applicationId)
                {
                    Undo.RecordObject(target, "editor");
                    settings.applicationId = buffer;
                    EditorUtility.SetDirty(target);
                }
            }

            artifactFoldout = EditorGUILayout.Foldout(artifactFoldout, "Artifacts");

            if (artifactFoldout)
            {
                int il = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                DrawInputArea();
                DrawStatsList();
                DrawLeaderboardList();
                DrawAchievementList();
                DrawDLCList();
                DrawInventoryArea();

                EditorGUI.indentLevel = il;
            }
        }

        private void DrawServerSettings()
        {
            sgsFoldout = EditorGUILayout.Foldout(sgsFoldout, "Steam Game Server Configuration");

            if (sgsFoldout)
            {
                EditorGUILayout.Space();
                DrawServerToggleSettings();
                EditorGUILayout.Space();
                DrawConnectionSettings();
                EditorGUILayout.Space();
                DrawServerGeneralSettings();
            }
        }

        private void DrawServerGeneralSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("General", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            if (!settings.server.anonymousServerLogin)
            {
                EditorGUILayout.HelpBox("If anonymous server login is not enabled then you must provide a game server token.", MessageType.Info);

                var token = EditorGUILayout.TextField("Token", settings.server.gameServerToken);

                if (token != settings.server.gameServerToken)
                {
                    Undo.RecordObject(target, "editor");
                    settings.server.gameServerToken = token;
                }
            }

            var serverName = EditorGUILayout.TextField("Server Name", settings.server.serverName);

            if (serverName != settings.server.serverName)
            {
                Undo.RecordObject(target, "editor");
                settings.server.serverName = serverName;
            }

            if (settings.server.supportSpectators)
            {
                serverName = EditorGUILayout.TextField("Spectator Name", settings.server.spectatorServerName);

                if (serverName != settings.server.spectatorServerName)
                {
                    Undo.RecordObject(target, "editor");
                    settings.server.spectatorServerName = serverName;
                }
            }

            serverName = EditorGUILayout.TextField("Description", settings.server.gameDescription);

            if (serverName != settings.server.gameDescription)
            {
                Undo.RecordObject(target, "editor");
                settings.server.gameDescription = serverName;
            }

            serverName = EditorGUILayout.TextField("Directory", settings.server.gameDirectory);

            if (serverName != settings.server.gameDirectory)
            {
                Undo.RecordObject(target, "editor");
                settings.server.gameDirectory = serverName;
            }

            serverName = EditorGUILayout.TextField("Map Name", settings.server.mapName);

            if (serverName != settings.server.mapName)
            {
                Undo.RecordObject(target, "editor");
                settings.server.mapName = serverName;
            }

            serverName = EditorGUILayout.TextField("Game Metadata", settings.server.gameData);

            if (serverName != settings.server.gameData)
            {
                Undo.RecordObject(target, "editor");
                settings.server.gameData = serverName;
            }

            var count = EditorGUILayout.TextField("Max Player Count", settings.server.maxPlayerCount.ToString());
            int buffer;
            if (int.TryParse(count, out buffer) && buffer != settings.server.maxPlayerCount)
            {
                Undo.RecordObject(target, "editor");
                settings.server.maxPlayerCount = buffer;
            }

            count = EditorGUILayout.TextField("Bot Player Count", settings.server.botPlayerCount.ToString());

            if (int.TryParse(count, out buffer) && buffer != settings.server.botPlayerCount)
            {
                Undo.RecordObject(target, "editor");
                settings.server.botPlayerCount = buffer;
            }
        }

        private void DrawConnectionSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Connection", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            var address = API.Utilities.IPUintToString(settings.server.ip);
            var nAddress = EditorGUILayout.TextField("IP Address", address);
            
            if(address != nAddress)
            {
                try
                {
                    var nip = API.Utilities.IPStringToUint(nAddress);
                    if (nip != settings.server.ip)
                    {
                        Undo.RecordObject(target, "editor");
                        settings.server.ip = nip;
                        EditorUtility.SetDirty(target);
                    }
                }
                catch { }
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Ports ");
            EditorGUILayout.EndHorizontal();

            var port = EditorGUILayout.TextField(new GUIContent("Game", "The port that clients will connect to for gameplay.  You will usually open up your own socket bound to this port."), settings.server.gamePort.ToString());
            ushort nPort;

            if (ushort.TryParse(port, out nPort) && nPort != settings.server.gamePort)
            {
                Undo.RecordObject(target, "editor");
                settings.server.gamePort = nPort;
                EditorUtility.SetDirty(target);
            }

            port = EditorGUILayout.TextField(new GUIContent("Query", "The port that will manage server browser related duties and info pings from clients.\nIf you pass MASTERSERVERUPDATERPORT_USEGAMESOCKETSHARE (65535) for QueryPort, then it will use 'GameSocketShare' mode, which means that the game is responsible for sending and receiving UDP packets for the master server updater. See references to GameSocketShare in isteamgameserver.hn"), settings.server.queryPort.ToString());
            
            if(ushort.TryParse(port, out nPort) && nPort != settings.server.queryPort)
            {
                Undo.RecordObject(target, "editor");
                settings.server.queryPort = nPort;
                EditorUtility.SetDirty(target);
            }

            port = EditorGUILayout.TextField("Spectator", settings.server.spectatorPort.ToString());

            if (ushort.TryParse(port, out nPort) && nPort != settings.server.spectatorPort)
            {
                Undo.RecordObject(target, "editor");
                settings.server.spectatorPort = nPort;
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawServerToggleSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Features", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var autoInt = GUILayout.Toggle(settings.server.autoInitialize, (settings.server.autoInitialize ? "Disable" : "Enable") + " Auto-Initialize", EditorStyles.toolbarButton);
            var autoLog = GUILayout.Toggle(settings.server.autoLogon, (settings.server.autoLogon ? "Disable" : "Enable") + " Auto-Logon", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var heart = GUILayout.Toggle(settings.server.enableHeartbeats, (settings.server.enableHeartbeats ? "Disable" : "Enable") + " Server Heartbeat", EditorStyles.toolbarButton);
            var anon = GUILayout.Toggle(settings.server.anonymousServerLogin, (settings.server.anonymousServerLogin ? "Disable" : "Enable") + " Anonymous Server Login", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();            
            var gsAuth = GUILayout.Toggle(settings.server.usingGameServerAuthApi, (settings.server.usingGameServerAuthApi ? "Disable" : "Enable") + " Game Server Auth API", EditorStyles.toolbarButton);
            var pass = GUILayout.Toggle(settings.server.isPasswordProtected, (settings.server.isPasswordProtected ? "Disable" : "Enable") + " Password Protected", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var dedicated = GUILayout.Toggle(settings.server.isDedicated, (settings.server.isDedicated ? "Disable" : "Enable") + " Dedicated Server", EditorStyles.toolbarButton);
            var spec = GUILayout.Toggle(settings.server.supportSpectators, (settings.server.supportSpectators ? "Disable" : "Enable") + " Spectator Support", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            //var mirror = GUILayout.Toggle(settings.server.enableMirror, (settings.server.enableMirror ? "Disable" : "Enable") + " Mirror Support", EditorStyles.toolbarButton);

            if (autoInt != settings.server.autoInitialize)
            {
                Undo.RecordObject(target, "editor");
                settings.server.autoInitialize = autoInt;
                EditorUtility.SetDirty(target);
            }

            if (heart != settings.server.enableHeartbeats)
            {
                Undo.RecordObject(target, "editor");
                settings.server.enableHeartbeats = heart;
                EditorUtility.SetDirty(target);
            }

            if (spec != settings.server.supportSpectators)
            {
                Undo.RecordObject(target, "editor");
                settings.server.supportSpectators = spec;
                EditorUtility.SetDirty(target);
            }

            if (anon != settings.server.anonymousServerLogin)
            {
                Undo.RecordObject(target, "editor");
                settings.server.anonymousServerLogin = anon;
                EditorUtility.SetDirty(target);
            }

            if (gsAuth != settings.server.usingGameServerAuthApi)
            {
                Undo.RecordObject(target, "editor");
                settings.server.usingGameServerAuthApi = gsAuth;
                EditorUtility.SetDirty(target);
            }

            if (pass != settings.server.isPasswordProtected)
            {
                Undo.RecordObject(target, "editor");
                settings.server.isPasswordProtected = pass;
                EditorUtility.SetDirty(target);
            }

            if (dedicated != settings.server.isDedicated)
            {
                Undo.RecordObject(target, "editor");
                settings.server.isDedicated = dedicated;
                EditorUtility.SetDirty(target);
            }

            if (autoLog != settings.server.autoLogon)
            {
                Undo.RecordObject(target, "editor");
                settings.server.autoLogon = autoLog;
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawStatsList()
        {

            statsFoldout = EditorGUILayout.Foldout(statsFoldout, "Stats: " + settings.stats.Count);

            if (statsFoldout)
            {
                int mil = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                var color = GUI.contentColor;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                GUI.contentColor = SteamSettings.Colors.BrightGreen;
                if (GUILayout.Button("+ Int", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    GUI.FocusControl(null);

                    IntStatObject nStat = ScriptableObject.CreateInstance<IntStatObject>();
                    nStat.name = "[Stat Int] New Int Stat";
                    nStat.data = "New Int Stat";
                    AssetDatabase.AddObjectToAsset(nStat, settings);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                    settings.stats.Add(nStat);
                    EditorUtility.SetDirty(target);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                    EditorGUIUtility.PingObject(nStat);
                }
                if (GUILayout.Button("+ Float", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    GUI.FocusControl(null);

                    FloatStatObject nStat = ScriptableObject.CreateInstance<FloatStatObject>();
                    nStat.name = "[Stat Float] New Float Stat";
                    nStat.data = "New Float Stat";
                    AssetDatabase.AddObjectToAsset(nStat, settings);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                    settings.stats.Add(nStat);
                    EditorUtility.SetDirty(target);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                    EditorGUIUtility.PingObject(nStat);
                }
                if (GUILayout.Button("+ Avg Rate", EditorStyles.toolbarButton, GUILayout.Width(75)))
                {
                    GUI.FocusControl(null);

                    AvgRateStatObject nStat = ScriptableObject.CreateInstance<AvgRateStatObject>();
                    nStat.name = "[Stat AvgRate] New Average Rate Stat";
                    nStat.data = "New Average Rate Stat";
                    AssetDatabase.AddObjectToAsset(nStat, settings);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                    settings.stats.Add(nStat);
                    EditorUtility.SetDirty(target);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                    EditorGUIUtility.PingObject(nStat);
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                int il = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                settings.stats.RemoveAll(p => p == null);

                for (int i = 0; i < settings.stats.Count; i++)
                {
                    var target = settings.stats[i];
                    if (target == null)
                        continue;

                    Color sC = GUI.backgroundColor;

                    GUI.backgroundColor = sC;
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                    {
                        GUI.FocusControl(null);
                        EditorGUIUtility.PingObject(target);
                    }

                    var newName = EditorGUILayout.TextField(target.data);
                    if (!string.IsNullOrEmpty(newName) && newName != target.data)
                    {
                        Undo.RecordObject(target, "name change");
                        target.data = newName;
                        switch (target.Type)
                        {
                            case StatObject.DataType.Int:
                                target.name = "[Stat Int] " + newName;
                                break;
                            case StatObject.DataType.Float:
                                target.name = "[Stat Float] " + newName;
                                break;
                            case StatObject.DataType.AvgRate:
                                target.name = "[Stat AvgRate] " + newName;
                                break;
                        }
                        EditorUtility.SetDirty(target);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                        EditorGUIUtility.PingObject(target);
                    }


                    var terminate = false;
                    GUI.contentColor = SteamSettings.Colors.ErrorRed;
                    if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                    {
                        GUI.FocusControl(null);
                        if (AssetDatabase.GetAssetPath(settings.stats[i]) == AssetDatabase.GetAssetPath(settings))
                        {
                            AssetDatabase.RemoveObjectFromAsset(settings.stats[i]);
                            needRefresh = true;
                        }

                        settings.stats.RemoveAt(i);
                        terminate = true;
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();

                    if (terminate)
                    {
                        EditorUtility.SetDirty(this.target);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this.target));
                        EditorGUIUtility.PingObject(this.target);
                        break;
                    }
                }
                EditorGUI.indentLevel = il;

                EditorGUI.indentLevel = mil;
            }
        }

        private void DrawAchievementList()
        {
            settings.achievements.RemoveAll(p => p == null);
            achievements = EditorGUILayout.Foldout(achievements, "Achievements: " + settings.achievements.Count);

            if (achievements)
            {
                var color = GUI.contentColor;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                GUI.contentColor = color;
                if (GUILayout.Button("Import", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    if(!API.App.Initialized)
                    {
                        Debug.Log("You cannot import data before initializing Steam API.");
                        return;
                    }

                    if (!Steamworks.SteamUser.BLoggedOn())
                    {
                        Debug.Log("You cannot import data when the Steam client is in offline mode.");
                        return;
                    }

                    try
                    {
                        GUI.FocusControl(null);

                        var names = API.StatsAndAchievements.Client.GetAchievementNames();

                        List<AchievementObject> toRemove = new List<AchievementObject>();
                        for (int i = 0; i < settings.achievements.Count; i++)
                        {
                            var achievement = settings.achievements[i];
                            if (!names.Contains(achievement.Id))
                            {
                                toRemove.Add(achievement);
                            }
                        }

                        while (toRemove.Count > 0)
                        {
                            var target = toRemove[0];
                            toRemove.Remove(target);
                            GUI.FocusControl(null);
                            if (AssetDatabase.GetAssetPath(target) == AssetDatabase.GetAssetPath(settings))
                            {
                                AssetDatabase.RemoveObjectFromAsset(target);
                                needRefresh = true;
                            }
                            settings.achievements.Remove(target);
                            EditorUtility.SetDirty(target);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                            EditorGUIUtility.PingObject(target);
                        }

                        for (int i = 0; i < names.Length; i++)
                        {
                            var achName = names[i];

                            var achObj = settings.achievements.FirstOrDefault(p => p.Id == achName);

                            bool created = false;
                            if (achObj == null)
                            {
                                achObj = ScriptableObject.CreateInstance<AchievementObject>();
                                created = true;
                            }

                            achObj.name = "[Ach] " + achName;
                            achObj.Id = achName;

                            if (created)
                            {
                                UnityEditor.AssetDatabase.AddObjectToAsset(achObj, SteamSettings.current);
                                settings.achievements.Add(achObj);
                            }

                            UnityEditor.EditorUtility.SetDirty(achObj);
                        }

                        UnityEditor.EditorUtility.SetDirty(target);
                        UnityEditor.EditorUtility.SetDirty(SteamSettings.current);
                        UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(SteamSettings.current));
                        UnityEditor.EditorUtility.SetDirty(target);

                        //settings.achievements.Add(nStat);
                        EditorUtility.SetDirty(target);
                        EditorGUIUtility.PingObject(target);
                    }
                    catch
                    {
                        Debug.LogWarning("Achievements can only be imported while the simulation is running, press play and try again to import.");
                    }
                }
                GUILayout.Space(10);
                tempAchName = GUILayout.TextField(tempAchName);
                if (!string.IsNullOrEmpty(tempAchName) && GUILayout.Button("Add", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    var achObj = ScriptableObject.CreateInstance<AchievementObject>();
                    achObj.name = "[Ach] " + tempAchName;
                    achObj.Id = tempAchName;
                    tempAchName = string.Empty;
                    UnityEditor.AssetDatabase.AddObjectToAsset(achObj, (SteamSettings)target);
                    settings.achievements.Add(achObj);
                    UnityEditor.EditorUtility.SetDirty(achObj);
                    UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath((SteamSettings)target));
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                int il = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                for (int i = 0; i < settings.achievements.Count; i++)
                {
                    var target = settings.achievements[i];

                    if (target == null)
                        continue;

                    Color sC = GUI.backgroundColor;

                    GUI.backgroundColor = sC;
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                    {
                        GUI.FocusControl(null);
                        EditorGUIUtility.PingObject(target);
                    }

                    EditorGUILayout.LabelField(target.Id);
                    if (UnityEngine.Application.isPlaying && API.App.Initialized)
                        EditorGUILayout.LabelField(target.IsAchieved ? "Unlocked" : "Locked");

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel = il;
            }
        }

        private void DrawLeaderboardList()
        {
#region Steam Complete
            if (settings.leaderboards == null)
                settings.leaderboards = new List<LeaderboardObject>();

            settings.leaderboards.RemoveAll(p => p == null);
            if (settings.leaderboards == null)
                settings.leaderboards = new List<LeaderboardObject>();

            leaderboardFoldout = EditorGUILayout.Foldout(leaderboardFoldout, "Leaderboards: " + settings.leaderboards.Count);

            if (leaderboardFoldout)
            {
                //int mil = EditorGUI.indentLevel;
                //EditorGUI.indentLevel++;

                var color = GUI.contentColor;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                GUI.contentColor = SteamSettings.Colors.BrightGreen;
                if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    GUI.FocusControl(null);

                    LeaderboardObject nStat = ScriptableObject.CreateInstance<LeaderboardObject>();
                    nStat.name = "[LdrBrd] New Leaderboard";
                    nStat.apiName = "New Leaderboard";
                    AssetDatabase.AddObjectToAsset(nStat, settings);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                    settings.leaderboards.Add(nStat);
                    EditorUtility.SetDirty(target);
                    EditorGUIUtility.PingObject(nStat);
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                var bgColor = GUI.backgroundColor;
                //int il = EditorGUI.indentLevel;
                //EditorGUI.indentLevel++;

                settings.leaderboards.RemoveAll(p => p == null);

                for (int i = 0; i < settings.leaderboards.Count; i++)
                {
                    var item = settings.leaderboards[i];

                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button(new GUIContent("P", "Ping the object in the Unity Editor"), EditorStyles.toolbarButton, GUILayout.Width(20)))
                    {
                        GUI.FocusControl(null);
                        EditorGUIUtility.PingObject(item);
                    }

                    if (GUILayout.Button(new GUIContent(item.createIfMissing ? "✓" : "-", "Create if missing?"), EditorStyles.toolbarButton, GUILayout.Width(20)))
                    {
                        GUI.FocusControl(null);
                        item.createIfMissing = !item.createIfMissing;
                        EditorUtility.SetDirty(item);
                    }

                    var nVal = EditorGUILayout.TextField(item.apiName);
                    if (nVal != item.apiName)
                    {
                        item.apiName = nVal;
                        item.name = "[LdrBrd] " + nVal;
                        EditorUtility.SetDirty(target);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                        EditorGUIUtility.PingObject(item);
                    }

                    GUIContent detailsContent = new GUIContent("Details:", "This is the number of detail values that will be loaded for entries when querying the leaderboard. Details are an int array and can be used to assoceate general data with a given entry such as class, rank, level, map, etc.");
                    EditorGUILayout.LabelField(detailsContent, GUILayout.Width(65));
                    var nCount = EditorGUILayout.TextField(item.maxDetailEntries.ToString(), GUILayout.Width(75));
                    int nCountBuffer = 0;
                    if (int.TryParse(nCount, out nCountBuffer))
                    {
                        item.maxDetailEntries = nCountBuffer;
                    }

                    GUI.contentColor = SteamSettings.Colors.ErrorRed;
                    if (GUILayout.Button(new GUIContent("X", "Remove the object"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                    {
                        GUI.FocusControl(null);
                        if (AssetDatabase.GetAssetPath(settings.leaderboards[i]) == AssetDatabase.GetAssetPath(settings))
                        {
                            AssetDatabase.RemoveObjectFromAsset(settings.leaderboards[i]);
                            needRefresh = true;
                        }
                        settings.leaderboards.RemoveAt(i);
                        EditorUtility.SetDirty(target);
                        EditorGUIUtility.PingObject(target);
                        return;
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();
                }
                //EditorGUI.indentLevel = il;
                GUI.backgroundColor = bgColor;
                //EditorGUI.indentLevel = mil;
            }
#endregion
            
        }

        private void DrawDLCList()
        {
            if (settings.dlc == null)
                settings.dlc = new List<DownloadableContentObject>();

            settings.dlc.RemoveAll(p => p == null);
            if (settings.dlc == null)
                settings.dlc = new List<DownloadableContentObject>();

            dlcFoldout = EditorGUILayout.Foldout(dlcFoldout, "Downloadable Content: " + settings.dlc.Count);

            if (dlcFoldout)
            {
                var color = GUI.contentColor;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                //GUI.contentColor = SteamSettings.Colors.BrightGreen;
                if (GUILayout.Button("Import", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    if (!API.App.Initialized)
                    {
                        Debug.Log("You cannot import data before initalizing Steam API.");
                        return;
                    }

                    if (!Steamworks.SteamUser.BLoggedOn())
                    {
                        Debug.Log("You cannot import data when the Steam client is in offline mode.");
                        return;
                    }

                    GUI.FocusControl(null);
                    try
                    {
                        var dlc = API.App.Client.Dlc;
                        var toRemove = new List<DownloadableContentObject>();
                        foreach (var eDlc in settings.dlc)
                        {
                            if (eDlc.data.AppId.m_AppId != default || !dlc.Any(p => p == eDlc.data))
                                toRemove.Add(eDlc);
                        }

                        while (toRemove.Count > 0)
                        {
                            var target = toRemove[0];
                            toRemove.Remove(target);
                            GUI.FocusControl(null);
                            if (AssetDatabase.GetAssetPath(target) == AssetDatabase.GetAssetPath(settings))
                            {
                                AssetDatabase.RemoveObjectFromAsset(target);
                                needRefresh = true;
                                UnityEditor.EditorUtility.SetDirty(settings);
                            }
                            settings.dlc.Remove(target);
                        }

                        for (int i = 0; i < dlc.Length; i++)
                        {
                            var tDlc = dlc[i];

                            var dlcObj = settings.dlc.FirstOrDefault(p => p.data == tDlc);

                            bool created = false;
                            if (dlcObj == null)
                            {
                                dlcObj = ScriptableObject.CreateInstance<DownloadableContentObject>();
                                created = true;
                            }

                            dlcObj.name = "[DLC] " + tDlc.Name;
                            dlcObj.data = tDlc;

                            if (created)
                            {
                                UnityEditor.AssetDatabase.AddObjectToAsset(dlcObj, settings);
                                settings.dlc.Add(dlcObj);
                            }

                            UnityEditor.EditorUtility.SetDirty(dlcObj);
                            UnityEditor.EditorUtility.SetDirty(settings);
                        }

                        //Found we need to double tap set dirty here, not sure if this is a bug with Unity editor or something missing in our process
                        UnityEditor.EditorUtility.SetDirty(target);
                        UnityEditor.EditorUtility.SetDirty(settings);
                        UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(target));
                        UnityEditor.EditorUtility.SetDirty(settings);

                        EditorUtility.SetDirty(target);
                        EditorGUIUtility.PingObject(target);
                    }
                    catch
                    {
                        Debug.LogWarning("DLC can only be imported while the simulation is running, press play and try again to import.");
                    }
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                var bgColor = GUI.backgroundColor;
                
                settings.dlc.RemoveAll(p => p == null);

                for (int i = 0; i < settings.dlc.Count; i++)
                {
                    var item = settings.dlc[i];

                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                    {
                        GUI.FocusControl(null);
                        EditorGUIUtility.PingObject(item);
                    }

                    EditorGUILayout.LabelField(item.name.Replace("[DLC] ", ""));

                    EditorGUILayout.EndHorizontal();
                }
                
                GUI.backgroundColor = bgColor;
            }
        }

        private void DrawInventoryArea()
        {

            settings.client.inventory.items.RemoveAll(p => p == null);

            foreach(var item in settings.client.inventory.items)
            {
                var objName = "[Inv] " + item.Id.ToString() + " " + item.Name;
                if (item.name != objName)
                {
                    item.name = objName;
                    EditorUtility.SetDirty(item);
                    EditorUtility.SetDirty(settings);

                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                    EditorGUIUtility.PingObject(item);
                }
            }

            inventoryFoldout = EditorGUILayout.Foldout(inventoryFoldout, "Inventory: " + settings.client.inventory.items.Count);

            if (inventoryFoldout)
            {
                var runLoad = settings.client.inventory.runTimeUpdateItemDefinitions;
                runLoad = EditorGUILayout.Toggle(new GUIContent("Runtime Update", "Should the item definitions be updated at run time in the event of item definition change notification from the Valve client."), runLoad);
                if(settings.client.inventory.runTimeUpdateItemDefinitions != runLoad)
                {
                    settings.client.inventory.runTimeUpdateItemDefinitions = runLoad;
                    EditorUtility.SetDirty(target);
                }

                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                var color = GUI.contentColor;
                GUI.contentColor = SteamSettings.Colors.BrightGreen;
                if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    GUI.FocusControl(null);

                    var nItem = ScriptableObject.CreateInstance<ItemDefinitionObject>();
                    nItem.Id = new SteamItemDef_t(GetNextAvailableItemNumber());
                    nItem.Name = "New Item";
                    nItem.name = "[Inv] " + nItem.Id.m_SteamItemDef + " New Item";

                    AssetDatabase.AddObjectToAsset(nItem, settings);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                    settings.client.inventory.items.Add(nItem);
                    EditorUtility.SetDirty(target);

                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(nItem);
                }
                GUI.contentColor = color;
                if (GUILayout.Button("Import", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    if (!API.App.Initialized)
                    {
                        Debug.Log("You cannot import data before initalizing Steam API.");
                        return;
                    }

                    if (!Steamworks.SteamUser.BLoggedOn())
                    {
                        Debug.Log("You cannot import data when the Steam client is in offline mode.");
                        return;
                    }

                    GUI.FocusControl(null);

                    try
                    {
                        if (settings.isDebugging)
                            Debug.Log("Processing inventory item definition cashe!");
                        settings.client.inventory.UpdateItemDefinitions(settings.isDebugging);

                        if (settings.isDebugging)
                            Debug.Log("Requesting Refresh of Steam Inventory Item Definitions");

                        API.Inventory.Client.EventSteamInventoryDefinitionUpdate.RemoveListener(HandleDefinitionUpdate);
                        API.Inventory.Client.EventSteamInventoryDefinitionUpdate.AddListener(HandleDefinitionUpdate);
                        API.Inventory.Client.LoadItemDefinitions();
                        
                    }
                    catch
                    {
                        Debug.LogWarning("Failed to import data from Steam, make sure you have simulated/ran at least once in order to engage the Steam API.");
                    }

                    EditorUtility.SetDirty(target);
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(target);
                }
                GUI.contentColor = color;
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    GUI.FocusControl(null);
                    var targets = settings.client.inventory.items.ToArray();
                    foreach (var item in targets)
                    {
                        if (AssetDatabase.GetAssetPath(item) == AssetDatabase.GetAssetPath(settings))
                        {
                            AssetDatabase.RemoveObjectFromAsset(item);
                            needRefresh = true;
                        }
                        settings.client.inventory.items.Remove(item);
                        EditorUtility.SetDirty(target);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                        EditorGUIUtility.PingObject(target);
                    }
                    EditorUtility.SetDirty(target);
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(target);
                }
                GUI.contentColor = color;

                if (GUILayout.Button("Copy JSON", EditorStyles.toolbarButton, GUILayout.Width(100)))
                {
                    GUI.FocusControl(null);

                    StringBuilder sb = new StringBuilder();
                    sb.Append("{");
                    sb.Append("\t\"appid\": " + settings.applicationId.ToString());
                    sb.Append(",\n\t\"items\": [");
                    sb.Append("\n" + settings.client.inventory.items[0].ToJson());

                    for (int i = 1; i < settings.client.inventory.items.Count; i++)
                    {
                        sb.Append(",\n" + settings.client.inventory.items[i].ToJson());
                    }

                    sb.Append("\n\t]\n}");

                    var n = new TextEditor();
                    n.text = sb.ToString();
                    n.SelectAll();
                    n.Copy();

                    Debug.Log("Output copied to clipboard:\n\n" + sb.ToString());
                }
                GUI.contentColor = color;


                EditorGUILayout.EndHorizontal();

                itemsFoldout = EditorGUILayout.Foldout(itemsFoldout, "Items: " + settings.client.inventory.items.Where(p => p.Type == InventoryItemType.item).Count());

                if (itemsFoldout)
                {
                    settings.client.inventory.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < settings.client.inventory.items.Count; i++)
                    {
                        var item = settings.client.inventory.items[i];

                        if (item.Type == InventoryItemType.item)
                        {
                            if (DrawItem(item))
                                break;
                        }
                    }
                }

                bundlesFoldout = EditorGUILayout.Foldout(bundlesFoldout, "Bundles: " + settings.client.inventory.items.Where(p => p.Type == InventoryItemType.bundle).Count());

                if (bundlesFoldout)
                {
                    settings.client.inventory.items.RemoveAll(p => p == null);
                    settings.client.inventory.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < settings.client.inventory.items.Count; i++)
                    {
                        var item = settings.client.inventory.items[i];

                        if (item.Type == InventoryItemType.bundle)
                        {
                            if (DrawItem(item))
                                break;
                        }
                    }
                }

                generatorFoldout = EditorGUILayout.Foldout(generatorFoldout, "Generators: " + settings.client.inventory.items.Where(p => p.Type == InventoryItemType.generator).Count());

                if (generatorFoldout)
                {
                    settings.client.inventory.items.RemoveAll(p => p == null);
                    settings.client.inventory.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < settings.client.inventory.items.Count; i++)
                    {
                        var item = settings.client.inventory.items[i];

                        if (item.Type == InventoryItemType.generator)
                        {
                            if (DrawItem(item))
                                break;
                        }
                    }
                }

                playtimegeneratorFoldout = EditorGUILayout.Foldout(playtimegeneratorFoldout, "Playtime Generators: " + settings.client.inventory.items.Where(p => p.Type == InventoryItemType.playtimegenerator).Count());

                if (playtimegeneratorFoldout)
                {
                    settings.client.inventory.items.RemoveAll(p => p == null);
                    settings.client.inventory.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < settings.client.inventory.items.Count; i++)
                    {
                        var item = settings.client.inventory.items[i];

                        if (item.Type == InventoryItemType.playtimegenerator)
                        {
                            if (DrawItem(item))
                                break;
                        }
                    }
                }

                taggeneratorFoldout = EditorGUILayout.Foldout(taggeneratorFoldout, "Tag Generators: " + settings.client.inventory.items.Where(p => p.Type == InventoryItemType.tag_generator).Count());

                if (taggeneratorFoldout)
                {
                    settings.client.inventory.items.RemoveAll(p => p == null);
                    settings.client.inventory.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < settings.client.inventory.items.Count; i++)
                    {
                        var item = settings.client.inventory.items[i];

                        if (item.Type == InventoryItemType.tag_generator)
                        {
                            if (DrawItem(item))
                                break;
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        private void HandleDefinitionUpdate()
        {
            Debug.Log("Processing inventory item definition update!");
            settings.client.inventory.UpdateItemDefinitions(settings.isDebugging);
            API.Inventory.Client.EventSteamInventoryDefinitionUpdate.RemoveListener(HandleDefinitionUpdate);
        }

        private void DrawInputArea()
        {
            //inputFoldout
            inputFoldout = EditorGUILayout.Foldout(inputFoldout, "Input: " + (settings.client.actions.Count + settings.client.actionSets.Count + settings.client.actionSetLayers.Count).ToString());

            if(inputFoldout)
            {

                EditorGUI.indentLevel++;

                inputActionSetFoldout = EditorGUILayout.Foldout(inputActionSetFoldout, "Action Sets: " + settings.client.actionSets.Count.ToString());

                if(inputActionSetFoldout)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    var color = GUI.contentColor;
                    GUI.contentColor = SteamSettings.Colors.BrightGreen;
                    if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                    {
                        GUI.FocusControl(null);

                        var nItem = ScriptableObject.CreateInstance<InputActionSet>();
                        nItem.setName = "action_set";
                        nItem.name = "[Input-Set] " + nItem.setName;

                        AssetDatabase.AddObjectToAsset(nItem, settings);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                        settings.client.actionSets.Add(nItem);
                        EditorUtility.SetDirty(target);

                        GUI.FocusControl(null);
                        EditorGUIUtility.PingObject(nItem);
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < settings.client.actionSets.Count; i++)
                    {
                        var item = settings.client.actionSets[i];

                        if (DrawItem(item))
                            break;
                    }
                }

                inputActionSetLayerFoldout = EditorGUILayout.Foldout(inputActionSetLayerFoldout, "Action Set Layerss: " + settings.client.actionSetLayers.Count.ToString());

                if (inputActionSetLayerFoldout)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    var color = GUI.contentColor;
                    GUI.contentColor = SteamSettings.Colors.BrightGreen;
                    if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                    {
                        GUI.FocusControl(null);

                        var nItem = ScriptableObject.CreateInstance<InputActionSetLayer>();
                        nItem.layerName = "action_set_layer";
                        nItem.name = "[Input-SetLayer] " + nItem.layerName;

                        AssetDatabase.AddObjectToAsset(nItem, settings);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                        settings.client.actionSetLayers.Add(nItem);
                        EditorUtility.SetDirty(target);

                        GUI.FocusControl(null);
                        EditorGUIUtility.PingObject(nItem);
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < settings.client.actionSetLayers.Count; i++)
                    {
                        var item = settings.client.actionSetLayers[i];

                        if (DrawItem(item))
                            break;
                    }
                }

                inputActionFoldout = EditorGUILayout.Foldout(inputActionFoldout, "Actions: " + settings.client.actions.Count.ToString());

                if (inputActionFoldout)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    var color = GUI.contentColor;
                    GUI.contentColor = SteamSettings.Colors.BrightGreen;
                    if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                    {
                        GUI.FocusControl(null);

                        var nItem = ScriptableObject.CreateInstance<InputAction>();
                        nItem.ActionName = "action";
                        nItem.name = "[Input-Action] " + nItem.ActionName;

                        AssetDatabase.AddObjectToAsset(nItem, settings);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                        settings.client.actions.Add(nItem);
                        EditorUtility.SetDirty(target);

                        GUI.FocusControl(null);
                        EditorGUIUtility.PingObject(nItem);
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < settings.client.actions.Count; i++)
                    {
                        var item = settings.client.actions[i];

                        if (DrawItem(item))
                            break;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        private bool DrawItem(ItemDefinitionObject item)
        {
            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                GUI.FocusControl(null);
                EditorGUIUtility.PingObject(item);
            }

            EditorGUILayout.LabelField(item.name);

            GUI.contentColor = SteamSettings.Colors.ErrorRed;
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                GUI.FocusControl(null);
                if (AssetDatabase.GetAssetPath(item) == AssetDatabase.GetAssetPath(settings))
                {
                    AssetDatabase.RemoveObjectFromAsset(item);
                    needRefresh = true;
                }
                settings.client.inventory.items.Remove(item);
                EditorUtility.SetDirty(target);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                EditorGUIUtility.PingObject(target);
                return true;
            }
            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();
            return false;
        }

        private bool DrawItem(InputActionSet item)
        {
            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                GUI.FocusControl(null);
                EditorGUIUtility.PingObject(item);
            }

            var result = EditorGUILayout.TextField(item.setName);

            if(result != item.setName)
            {
                item.setName = result;
                item.name = "[Input-Set] " + item.setName;

                EditorUtility.SetDirty(item);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                EditorGUIUtility.PingObject(item);
            }

            GUI.contentColor = SteamSettings.Colors.ErrorRed;
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                GUI.FocusControl(null);
                if (AssetDatabase.GetAssetPath(item) == AssetDatabase.GetAssetPath(settings))
                {
                    AssetDatabase.RemoveObjectFromAsset(item);
                    needRefresh = true;
                }
                settings.client.actionSets.Remove(item);
                EditorUtility.SetDirty(target);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                EditorGUIUtility.PingObject(target);
                return true;
            }
            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();
            return false;
        }

        private bool DrawItem(InputActionSetLayer item)
        {
            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                GUI.FocusControl(null);
                EditorGUIUtility.PingObject(item);
            }

            var result = EditorGUILayout.TextField(item.layerName);

            if (result != item.layerName)
            {
                item.layerName = result;
                item.name = "[Input-SetLayer] " + item.layerName;

                EditorUtility.SetDirty(item);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                EditorGUIUtility.PingObject(item);
            }

            GUI.contentColor = SteamSettings.Colors.ErrorRed;
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                GUI.FocusControl(null);
                if (AssetDatabase.GetAssetPath(item) == AssetDatabase.GetAssetPath(settings))
                {
                    AssetDatabase.RemoveObjectFromAsset(item);
                    needRefresh = true;
                }
                settings.client.actionSetLayers.Remove(item);
                EditorUtility.SetDirty(target);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                EditorGUIUtility.PingObject(target);
                return true;
            }
            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();
            return false;
        }

        private bool DrawItem(InputAction item)
        {
            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                GUI.FocusControl(null);
                EditorGUIUtility.PingObject(item);
            }

            if (item.Type == InputActionType.Digital)
            {
                if (GUILayout.Button(new GUIContent("DI", "Click to make this an analog action."), EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    item.Type = InputActionType.Analog;

                    GUI.FocusControl(null);
                    EditorUtility.SetDirty(item);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                    EditorGUIUtility.PingObject(item);
                }
            }
            else
            {
                if (GUILayout.Button(new GUIContent("AI", "Click to make this a digital action."), EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    item.Type = InputActionType.Digital;

                    GUI.FocusControl(null);
                    EditorUtility.SetDirty(item);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                    EditorGUIUtility.PingObject(item);
                }
            }

            var result = EditorGUILayout.TextField(item.ActionName);

            if (result != item.ActionName)
            {
                item.ActionName = result;
                item.name = "[Input-Action] " + item.ActionName;

                EditorUtility.SetDirty(item);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                EditorGUIUtility.PingObject(item);
            }

            GUI.contentColor = SteamSettings.Colors.ErrorRed;
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                GUI.FocusControl(null);
                if (AssetDatabase.GetAssetPath(item) == AssetDatabase.GetAssetPath(settings))
                {
                    AssetDatabase.RemoveObjectFromAsset(item);
                    needRefresh = true;
                }
                settings.client.actions.Remove(item);
                EditorUtility.SetDirty(target);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                EditorGUIUtility.PingObject(target);
                return true;
            }
            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();
            return false;
        }

        private int GetNextAvailableItemNumber()
        {
            int id = 1;
            while (settings.client.inventory.items.Any(p => p.Id.m_SteamItemDef == id) && id < 999999)
                id++;

            return id;
        }
    }
}
#endif