#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Collections.Generic;
using UnityEditor.UIElements;
using System.Collections;

namespace Heathen.SteamworksIntegration.Editors
{
    public class SteamInspector_Code : EditorWindow
    {
        public static SteamInspector_Code instance;

        public Texture2D icon;
        public VisualTreeAsset headerTree;
        public VisualTreeAsset dlcEntry;
        public VisualTreeAsset statEntry;
        public VisualTreeAsset achievementEntry;
        public VisualTreeAsset leaderboardEntry;
        public VisualTreeAsset inventoryItemEntry;
        public StyleSheet styleSheet;

        public Texture2D avatarPlaceholderImage;

        private Label initializationField;
        private Label reportedAppId;
        private VisualElement avatarImage;
        private Label csteamId;
        private Label userName;
        private Label userLevel;
        private Label userPresence;
        private ToolbarToggle commonToggle;
        private ToolbarToggle lobbyToggle;
        private ToolbarToggle inventoryToggle;
        private ToolbarToggle statsToggle;
        private ToolbarToggle achToggle;
        private ToolbarToggle leaderboardToggle;
        private ToolbarToggle dlcToggle;
        private VisualElement dlcCollection;
        private VisualElement statCollection;
        private VisualElement achievementCollection;
        private VisualElement leaderboardCollection;
        private VisualElement commonContainer;
        private VisualElement lobbyContainer;
        private VisualElement inventoryContainer;
        private IMGUIContainer lobbyIMGUI;
        private LobbyIMGUITool lobbyTools;
        private VisualElement statsPage;
        private VisualElement achPage;
        private VisualElement leaderboardPage;
        private VisualElement dlcPage;
        private VisualElement invPage;
        private Button resetStatsButton;
        private Button refreshBoardsButton;

        private bool refreshingScores = false;
        private bool internalUpdate = false;
        private Dictionary<Steamworks.SteamLeaderboard_t, LeaderboardEntry> entryDictionary = new Dictionary<Steamworks.SteamLeaderboard_t, LeaderboardEntry>();

        [MenuItem("Window/Steamworks/Inspector")]
        public static void ShowExample()
        {
            var version = SessionState.GetString("com.heathen.steamworks-version", "4.0");
            instance = GetWindow<SteamInspector_Code>();
            instance.titleContent = new GUIContent($"Inspector v{version}", instance.icon);
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            lobbyTools = new LobbyIMGUITool();

            VisualElement labelFromUXML = headerTree.CloneTree();
            root.Add(labelFromUXML);

            initializationField = root.Q<Label>(name = "lblInit");
            reportedAppId = root.Q<Label>(name = "lblRptAppId");
            avatarImage = root.Q<VisualElement>(name = "imgAvatar");
            csteamId = root.Q<Label>(name = "lblCsteamId");
            userName = root.Q<Label>(name = "lblUserName");
            userLevel = root.Q<Label>(name = "lblUserLevel");
            userPresence = root.Q<Label>(name = "lblUserPresence");

            dlcCollection = root.Q<VisualElement>(name = "dlcContent");
            statCollection = root.Q<VisualElement>(name = "statContent");
            achievementCollection = root.Q<VisualElement>(name = "achContent");
            leaderboardCollection = root.Q<VisualElement>(name = "ldrBoardContent");
            inventoryContainer = root.Q<VisualElement>(name = "InvItemContent");
            lobbyIMGUI = root.Q<IMGUIContainer>(name = "lobbyContainer");

            commonToggle = root.Q<ToolbarToggle>(name = "tglCmn");
            lobbyToggle = root.Q<ToolbarToggle>(name = "tglLby");
            inventoryToggle = root.Q<ToolbarToggle>(name = "tglInventory");
            statsToggle = root.Q<ToolbarToggle>(name = "tglStats");
            dlcToggle = root.Q<ToolbarToggle>(name = "tglDlc");
            achToggle = root.Q<ToolbarToggle>(name = "tglAch");
            leaderboardToggle = root.Q<ToolbarToggle>(name = "tglLeaderboard");

            commonContainer = root.Q<ScrollView>(name = "pgCommon");
            lobbyContainer = root.Q<VisualElement>(name = "pgLobby");

            statsPage = root.Q<VisualElement>(name = "pgStats");
            achPage = root.Q<VisualElement>(name = "pgAchievements");
            leaderboardPage = root.Q<VisualElement>(name = "pgLeaderboard");
            dlcPage = root.Q<VisualElement>(name = "pgDlc");
            invPage = root.Q<VisualElement>(name = "pgInventory");

            resetStatsButton = root.Q<Button>(name = "btResetAll");
            refreshBoardsButton = root.Q<Button>(name = "btBoardRefresh");

            lobbyIMGUI.onGUIHandler = lobbyTools.OnGUI;


            commonToggle.RegisterValueChangedCallback(HandleCommonToggleChange);
            lobbyToggle.RegisterValueChangedCallback(HandleLobbyToggleChange);
            inventoryToggle.RegisterValueChangedCallback(HandleInventoryToggleChange);

            statsToggle.RegisterValueChangedCallback(HandleStatsToggleChange);
            achToggle.RegisterValueChangedCallback(HandleAchievementToggleChange);
            dlcToggle.RegisterValueChangedCallback(HandleDlcToggleChange);
            leaderboardToggle.RegisterValueChangedCallback(HandleLeaderboardToggleChange);

            RefreshAllLeaderboardEntries();
            resetStatsButton.clicked += () => { API.StatsAndAchievements.Client.ResetAllStats(false); };
            refreshBoardsButton.clicked += RefreshAllLeaderboardEntries;
        }

        private void RefreshAllLeaderboardEntries()
        {
            if (refreshingScores || !Application.isPlaying || SteamSettings.behaviour == null)
                return;

            SteamSettings.behaviour.StartCoroutine(RefreshScores());
        }

        private IEnumerator RefreshScores()
        {
            refreshingScores = true;

            entryDictionary.Clear();
            foreach (var board in SteamSettings.current.leaderboards)
            {
                bool waiting = true;
                float exitTime = Time.realtimeSinceStartup + 5f;
                var id = board.data.id;
                board.GetUserEntry((r, e) =>
                {
                    if (!e && !entryDictionary.ContainsKey(id))
                        entryDictionary.Add(id, r);

                    waiting = false;
                });

                yield return new WaitWhile(() => waiting && exitTime > Time.realtimeSinceStartup);
            }

            refreshingScores = false;
        }

        private void HandleStatsToggleChange(ChangeEvent<bool> evt)
        {
            if (!evt.newValue || internalUpdate)
                return;

            internalUpdate = true;

            if (statsToggle.value)
            {
                if (commonToggle.value)
                    commonToggle.value = false;
                //if (statsToggle.value)
                //    statsToggle.value = false;
                if (achToggle.value)
                    achToggle.value = false;
                if (leaderboardToggle.value)
                    leaderboardToggle.value = false;
                if (dlcToggle.value)
                    dlcToggle.value = false;
                if (inventoryToggle.value)
                    inventoryToggle.value = false;
                if (lobbyToggle.value)
                    lobbyToggle.value = false;


                commonContainer.style.display = DisplayStyle.None;
                statsPage.style.display = DisplayStyle.Flex;
                achPage.style.display = DisplayStyle.None;
                leaderboardPage.style.display = DisplayStyle.None;
                dlcPage.style.display = DisplayStyle.None;
                invPage.style.display = DisplayStyle.None;
                lobbyContainer.style.display = DisplayStyle.None;
            }

            internalUpdate = false;
        }

        private void HandleAchievementToggleChange(ChangeEvent<bool> evt)
        {
            if (!evt.newValue || internalUpdate)
                return;

            internalUpdate = true;

            if (achToggle.value)
            {
                if (commonToggle.value)
                    commonToggle.value = false;
                if (statsToggle.value)
                    statsToggle.value = false;
                //if (achToggle.value)
                //    achToggle.value = false;
                if (leaderboardToggle.value)
                    leaderboardToggle.value = false;
                if (dlcToggle.value)
                    dlcToggle.value = false;
                if (inventoryToggle.value)
                    inventoryToggle.value = false;
                if (lobbyToggle.value)
                    lobbyToggle.value = false;


                commonContainer.style.display = DisplayStyle.None;
                statsPage.style.display = DisplayStyle.None;
                achPage.style.display = DisplayStyle.Flex;
                leaderboardPage.style.display = DisplayStyle.None;
                dlcPage.style.display = DisplayStyle.None;
                invPage.style.display = DisplayStyle.None;
                lobbyContainer.style.display = DisplayStyle.None;
            }

            internalUpdate = false;
        }

        private void HandleDlcToggleChange(ChangeEvent<bool> evt)
        {
            if (!evt.newValue || internalUpdate)
                return;

            internalUpdate = true;

            if (dlcToggle.value)
            {
                if (commonToggle.value)
                    commonToggle.value = false;
                if (statsToggle.value)
                    statsToggle.value = false;
                if (achToggle.value)
                    achToggle.value = false;
                if (leaderboardToggle.value)
                    leaderboardToggle.value = false;
                //if (dlcToggle.value)
                //    dlcToggle.value = false;
                if (inventoryToggle.value)
                    inventoryToggle.value = false;
                if (lobbyToggle.value)
                    lobbyToggle.value = false;


                commonContainer.style.display = DisplayStyle.None;
                statsPage.style.display = DisplayStyle.None;
                achPage.style.display = DisplayStyle.None;
                leaderboardPage.style.display = DisplayStyle.None;
                dlcPage.style.display = DisplayStyle.Flex;
                invPage.style.display = DisplayStyle.None;
                lobbyContainer.style.display = DisplayStyle.None;
            }

            internalUpdate = false;
        }

        private void HandleLeaderboardToggleChange(ChangeEvent<bool> evt)
        {
            if (!evt.newValue || internalUpdate)
                return;

            internalUpdate = true;

            if (leaderboardToggle.value)
            {
                if (commonToggle.value)
                    commonToggle.value = false;
                if (statsToggle.value)
                    statsToggle.value = false;
                if (achToggle.value)
                    achToggle.value = false;
                //if (leaderboardToggle.value)
                //    leaderboardToggle.value = false;
                if (dlcToggle.value)
                    dlcToggle.value = false;
                if (inventoryToggle.value)
                    inventoryToggle.value = false;
                if (lobbyToggle.value)
                    lobbyToggle.value = false;


                commonContainer.style.display = DisplayStyle.None;
                statsPage.style.display = DisplayStyle.None;
                achPage.style.display = DisplayStyle.None;
                leaderboardPage.style.display = DisplayStyle.Flex;
                dlcPage.style.display = DisplayStyle.None;
                invPage.style.display = DisplayStyle.None;
                lobbyContainer.style.display = DisplayStyle.None;
            }

            internalUpdate = false;
        }

        private void HandleCommonToggleChange(ChangeEvent<bool> evt)
        {
            if (!evt.newValue || internalUpdate)
                return;

            internalUpdate = true;

            if (commonToggle.value)
            {
                //if (commonToggle.value)
                //    commonToggle.value = false;
                if (statsToggle.value)
                    statsToggle.value = false;
                if (achToggle.value)
                    achToggle.value = false;
                if (leaderboardToggle.value)
                    leaderboardToggle.value = false;
                if (dlcToggle.value)
                    dlcToggle.value = false;
                if (inventoryToggle.value)
                    inventoryToggle.value = false;
                if (lobbyToggle.value)
                    lobbyToggle.value = false;


                commonContainer.style.display = DisplayStyle.Flex;
                statsPage.style.display = DisplayStyle.None;
                achPage.style.display = DisplayStyle.None;
                leaderboardPage.style.display = DisplayStyle.None;
                dlcPage.style.display = DisplayStyle.None;
                invPage.style.display = DisplayStyle.None;
                lobbyContainer.style.display = DisplayStyle.None;
            }

            internalUpdate = false;
        }

        private void HandleLobbyToggleChange(ChangeEvent<bool> evt)
        {
            if (!evt.newValue || internalUpdate)
                return;

            internalUpdate = true;

            if (lobbyToggle.value)
            {
                if (commonToggle.value)
                    commonToggle.value = false;
                if (statsToggle.value)
                    statsToggle.value = false;
                if (achToggle.value)
                    achToggle.value = false;
                if (leaderboardToggle.value)
                    leaderboardToggle.value = false;
                if (dlcToggle.value)
                    dlcToggle.value = false;
                if (inventoryToggle.value)
                    inventoryToggle.value = false;
                //if (lobbyToggle.value)
                //    lobbyToggle.value = false;


                commonContainer.style.display = DisplayStyle.None;
                statsPage.style.display = DisplayStyle.None;
                achPage.style.display = DisplayStyle.None;
                leaderboardPage.style.display = DisplayStyle.None;
                dlcPage.style.display = DisplayStyle.None;
                invPage.style.display = DisplayStyle.None;
                lobbyContainer.style.display = DisplayStyle.Flex;
            }

            internalUpdate = false;
        }

        private void HandleInventoryToggleChange(ChangeEvent<bool> evt)
        {
            if (!evt.newValue || internalUpdate)
                return;

            internalUpdate = true;

            if (inventoryToggle.value)
            {
                if (commonToggle.value)
                    commonToggle.value = false;
                if (statsToggle.value)
                    statsToggle.value = false;
                if (achToggle.value)
                    achToggle.value = false;
                if (leaderboardToggle.value)
                    leaderboardToggle.value = false;
                if (dlcToggle.value)
                    dlcToggle.value = false;
                //if (inventoryToggle.value)
                //    inventoryToggle.value = false;
                if (lobbyToggle.value)
                    lobbyToggle.value = false;


                commonContainer.style.display = DisplayStyle.None;
                statsPage.style.display = DisplayStyle.None;
                achPage.style.display = DisplayStyle.None;
                leaderboardPage.style.display = DisplayStyle.None;
                dlcPage.style.display = DisplayStyle.None;
                invPage.style.display = DisplayStyle.Flex;
                lobbyContainer.style.display = DisplayStyle.None;
            }

            internalUpdate = false;
        }

        internal static string GetSteamAppIdText()
        {
            var appIdPath = new DirectoryInfo(Application.dataPath).Parent.FullName + "/steam_appid.txt";

            var appIdTxtExists = File.Exists(appIdPath);

            if (!appIdTxtExists)
            {
                File.WriteAllText(appIdPath, "480");
                return "480";                
            }
            else
            {
                return File.ReadAllText(appIdPath);
            }
        }

        private void OnGUI()
        {
            if (initializationField == null)
                return;

            if (Application.isPlaying)
            {
                if (API.App.Initialized)
                {
                    var SettingsCollection = SteamSettings.GetAllSettings();
                    var reportedId = API.App.Client.Id.m_AppId;

                    foreach (var setting in SettingsCollection)
                    {
                        if (setting.applicationId == reportedId)
                        {
                            SteamSettings.current = setting;
                            break;
                        }
                    }

                    var appIdPath = new DirectoryInfo(Application.dataPath).Parent.FullName + "/steam_appid.txt";

                    var appIdTxtExists = File.Exists(appIdPath);

                    if (appIdTxtExists)
                    {
                        var stringAppId = File.ReadAllText(appIdPath);
                        uint.TryParse(stringAppId, out var _appId);
                        if(_appId != reportedId)
                        {
                            Debug.LogWarning($"Steamworks SDK is initialized as App ID {reportedId}, Your steam_appid.txt however is set to {_appId} this suggests that you did NOT restart Unity and Visual Studio after changing your App ID.");
                        }
                    }

                    initializationField.text = "Initialized";
                    reportedAppId.text = API.App.Client.Id.m_AppId.ToString();
                    csteamId.text = API.User.Client.Id.ToString();
                    userName.text = API.User.Client.Id.Name;
                    userLevel.text = API.User.Client.Level.ToString();
                    var rp = API.User.Client.RichPresence;

                    if (rp.Length > 0)
                    {
                        userPresence.text = "";
                        for (int i = 0; i < rp.Length; i++)
                        {
                            if (userPresence.text.Length > 0)
                                userPresence.text += "\n";

                            var rpe = rp[i];
                            userPresence.text += rpe.key + ":" + rpe.value;
                        }
                    }
                    else
                    {
                        userPresence.text = "None set";
                    }

                }
                else if (API.App.HasInitializationError)
                {
                    initializationField.text = "Erred";
                    csteamId.text = "0";
                    userName.text = "unknown";
                    userLevel.text = "1";
                    userPresence.text = "unknown";
                }
                else
                {
                    initializationField.text = "Pending";
                    csteamId.text = "0";
                    userName.text = "unknown";
                    userLevel.text = "1";
                    userPresence.text = "unknown";
                }

                API.User.Client.Id.LoadAvatar((t) =>
                {
                    avatarImage.style.backgroundImage = new StyleBackground(t);
                });
                
                UpdateAchievements();
                UpdateDLC();
                UpdateStats();
                UpdateLeaderboard();
                UpdateInventory();
            }
            else
            {
                initializationField.text = "Idle";
                reportedAppId.text = "NA";

                avatarImage.style.backgroundImage = new StyleBackground(avatarPlaceholderImage);
                csteamId.text = "0";
                userName.text = "None";
                userLevel.text = "0";
                userPresence.text = "None";


                if (dlcCollection.childCount > 0)
                    dlcCollection.Clear();

                if (achievementCollection.childCount > 0)
                    achievementCollection.Clear();

                if (statCollection.childCount > 0)
                    statCollection.Clear();

                if (leaderboardCollection.childCount > 0)
                    leaderboardCollection.Clear();
            }
        }

        private void UpdateAchievements()
        {
            if(SteamSettings.current == null)
            {
                achievementCollection.Clear();
                return;
            }

            if (achievementCollection.childCount > SteamSettings.Achievements.Count)
                achievementCollection.Clear();

            while (achievementCollection.childCount < SteamSettings.Achievements.Count)
            {
                var nElement = achievementEntry.CloneTree();
                achievementCollection.Add(nElement);

                Button unlockButton = nElement.Query<Button>(name = "btnUnlock");
                var index = achievementCollection.childCount - 1;
                unlockButton.clicked += () =>
                {
                    var item = SteamSettings.Achievements[index];
                    item.Unlock();
                };
                Button resetButton = nElement.Query<Button>(name = "btnReset");
                resetButton.clicked += () =>
                {
                    var item = SteamSettings.Achievements[index];
                    item.ClearAchievement();
                };
            }

            var itemList = new List<VisualElement>(achievementCollection.Children());

            for (int i = 0; i < SteamSettings.Achievements.Count; i++)
            {
                var ach = SteamSettings.Achievements[i];
                Label lblName = itemList[i].Query<Label>(name = "lblAchName");
                lblName.text = ach.Name;

                Label lblId = itemList[i].Query<Label>(name = "lblAchId");
                lblId.text = ach.Id;

                Label lblStatus = itemList[i].Query<Label>(name = "lblStatus");
                lblStatus.text = ach.IsAchieved.ToString();

                Button unlockButton = itemList[i].Query<Button>(name = "btnUnlock");
                Button resetButton = itemList[i].Query<Button>(name = "btnReset");
                if (ach.IsAchieved)
                {
                    unlockButton.style.display = DisplayStyle.None;
                    resetButton.style.display = DisplayStyle.Flex;
                }
                else
                {
                    unlockButton.style.display = DisplayStyle.Flex;
                    resetButton.style.display = DisplayStyle.None;
                }
            }
        }

        private void UpdateStats()
        {
            if (SteamSettings.current == null)
            {
                statCollection.Clear();
                return;
            }

            if (statCollection.childCount > SteamSettings.Stats.Count)
                statCollection.Clear();

            while (statCollection.childCount < SteamSettings.Stats.Count)
            {
                var nElement = statEntry.CloneTree();
                statCollection.Add(nElement);

                Button setButton = nElement.Query<Button>(name = "btnSet");

                var index = statCollection.childCount - 1;
                setButton.clicked += () =>
                {
                    var item = SteamSettings.Stats[index];
                    var statsList = new List<VisualElement>(statCollection.Children());
                    TextField inputField = statsList[index].Query<TextField>(name = "inputValue");

                    if (item.Type == StatObject.DataType.Int)
                    {
                        if (int.TryParse(inputField.value, out int result))
                        {
                            item.SetIntStat(result);
                        }
                        else
                        {
                            Debug.LogWarning("Unable to set stat: " + item.data + " value must be a valid int.");
                            inputField.value = item.GetIntValue().ToString();
                        }
                    }
                    else if (item.Type == StatObject.DataType.Float)
                    {
                        if (float.TryParse(inputField.value, out float result))
                            item.SetFloatStat(result);
                        else
                        {
                            Debug.LogWarning("Unable to set stat: " + item.data + " value must be a valid float.");
                            inputField.value = item.GetFloatValue().ToString();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Cannot set average rate state values from the inspector as they require both a value and a duration");
                    }
                };
            }

            var statList = new List<VisualElement>(statCollection.Children());

            for (int i = 0; i < Heathen.SteamworksIntegration.SteamSettings.Stats.Count; i++)
            {
                var stat = Heathen.SteamworksIntegration.SteamSettings.Stats[i];
                Label statName = statList[i].Query<Label>(name = "lblStatName");
                statName.text = stat.name;

                Label statDataType = statList[i].Query<Label>(name = "lblDataType");
                statDataType.text = stat.Type.ToString();

                Label statValue = statList[i].Query<Label>(name = "lblValue");
                if (stat.Type == Heathen.SteamworksIntegration.StatObject.DataType.Float)
                    statValue.text = stat.GetFloatValue().ToString();
                else
                    statValue.text = stat.GetIntValue().ToString();
            }
        }

        private void UpdateDLC()
        {
            if (SteamSettings.current == null)
            {
                dlcCollection.Clear();
                return;
            }

            if (dlcCollection.childCount > SteamSettings.DLC.Count)
                dlcCollection.Clear();

            while (dlcCollection.childCount < SteamSettings.DLC.Count)
            {
                var nElement = dlcEntry.CloneTree();
                dlcCollection.Add(nElement);
            }

            var dlcList = new List<VisualElement>(dlcCollection.Children());

            for (int i = 0; i < SteamSettings.DLC.Count; i++)
            {
                var dlc = SteamSettings.DLC[i];
                Label dlcName = dlcList[i].Query<Label>(name = "lblDlcName");
                dlcName.text = dlc.name;

                Label dlcId = dlcList[i].Query<Label>(name = "lblAppId");
                dlcId.text = dlc.data.ToString();

                Label dlcSub = dlcList[i].Query<Label>(name = "lblSubscribed");
                dlcSub.text = dlc.IsSubscribed.ToString();
            }
        }

        private void UpdateLeaderboard()
        {
            if (SteamSettings.current == null)
            {
                leaderboardCollection.Clear();
                return;
            }

            if (leaderboardCollection.childCount > SteamSettings.Leaderboards.Count)
                leaderboardCollection.Clear();

            while (leaderboardCollection.childCount < SteamSettings.Leaderboards.Count)
            {
                var nElement = leaderboardEntry.CloneTree();
                leaderboardCollection.Add(nElement);
            }

            var itemList = new List<VisualElement>(leaderboardCollection.Children());

            if (entryDictionary == null)
                entryDictionary = new Dictionary<Steamworks.SteamLeaderboard_t, LeaderboardEntry>();

            for (int i = 0; i < SteamSettings.Leaderboards.Count; i++)
            {
                var leaderboard = SteamSettings.Leaderboards[i];
                Label lblName = itemList[i].Query<Label>(name = "lblName");
                lblName.text = leaderboard.apiName;

                Label lblId = itemList[i].Query<Label>(name = "lblId");
                lblId.text = leaderboard.data.id.ToString();

                Label lblScore = itemList[i].Query<Label>(name = "lblScore");
                Label lblRank = itemList[i].Query<Label>(name = "lblRank");

                if (entryDictionary.ContainsKey(leaderboard.data.id) && leaderboard.Valid && entryDictionary[leaderboard.data.id] != null)
                {
                    lblScore.text = entryDictionary[leaderboard.data.id].entry.m_nScore.ToString();
                    lblRank.text = entryDictionary[leaderboard.data.id].entry.m_nGlobalRank.ToString();
                }
                else
                {
                    lblScore.text = "Unknown";
                    lblRank.text = "Unknown";
                }
            }
        }

        private void UpdateInventory()
        {
            if (SteamSettings.current == null)
            {
                inventoryContainer.Clear();
                return;
            }

            if (inventoryContainer.childCount > SteamSettings.Client.inventory.items.Count)
                inventoryContainer.Clear();

            while (inventoryContainer.childCount < SteamSettings.Client.inventory.items.Count)
            {
                var nElement = inventoryItemEntry.CloneTree();
                inventoryContainer.Add(nElement);

                Button grantButton = nElement.Query<Button>(name = "btnGrant");
                var index = inventoryContainer.childCount - 1;
                grantButton.clicked += () =>
                {
                    var item = SteamSettings.Client.inventory.items[index];
                    API.Inventory.Client.GenerateItems(new Steamworks.SteamItemDef_t[] { item.Id }, new uint[] { 1 }, (i) =>
                    {
                        Debug.Log("Generate of " + item.Id.m_SteamItemDef + " completed with result: " + i.result);
                    });
                };
                Button clearButton = nElement.Query<Button>(name = "btnClear");
                clearButton.clicked += () =>
                {
                    var item = SteamSettings.Client.inventory.items[index];
                    foreach (var instance in item.Details)
                    {
                        if (instance.Quantity > 0)
                        {
                            var quantity = instance.Quantity;
                            API.Inventory.Client.ConsumeItem(instance.ItemId, instance.Quantity, (i) =>
                            {
                                if (i.result == Steamworks.EResult.k_EResultOK)
                                    Debug.Log("Consume request: " + i.result);
                                else
                                {
                                    Debug.Log("Consume request: " + i.result + ", you may need to run the clear request multiple times.");
                                }
                            });
                        }
                    }
                };
            }

            var itemList = new List<VisualElement>(inventoryContainer.Children());

            for (int i = 0; i < Heathen.SteamworksIntegration.SteamSettings.Client.inventory.items.Count; i++)
            {
                var item = Heathen.SteamworksIntegration.SteamSettings.Client.inventory.items[i];

                Label lblName = itemList[i].Query<Label>(name = "LblInvItemName");
                lblName.text = item.Name;

                Label lblId = itemList[i].Query<Label>(name = "LblInvItemId");
                lblId.text = item.Id.m_SteamItemDef.ToString();

                Label lblScore = itemList[i].Query<Label>(name = "LblInvItemType");
                lblScore.text = item.Type.ToString();

                Label lblRank = itemList[i].Query<Label>(name = "LblInvItemCount");
                lblRank.text = item.TotalQuantity.ToString();

                Button clearButton = itemList[i].Query<Button>(name = "btnClear");
                if (item.Type == InventoryItemType.item && item.TotalQuantity > 0)
                {
                    clearButton.style.display = DisplayStyle.Flex;
                }
                else
                {
                    clearButton.style.display = DisplayStyle.None;
                }

            }
        }
    }
}
#endif