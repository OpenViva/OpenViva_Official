#if !DISABLESTEAMWORKS  && STEAMWORKSNET
//#define UNITY_SERVER
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// <para>The root of Heathen's Steamworks system. <see cref="SteamSettings"/> provides access to all core functionality including stats, achievements, the friend system and the overlay system.</para>
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="SteamSettings"/> object is the root of Heathen Engineering's Steamworks kit.
    /// <see cref="SteamSettings"/> contains the configuration for the fundamental systems of the Steamworks API and provides access to all core functionality.
    /// You can easily access the active <see cref="SteamSettings"/> object any time via <see cref="current"/> a static member that is populated on initialization of the Steamworks API with the settings that are being used to configure it.</para>
    /// <para><see cref="SteamSettings"/> is divided into 2 major areas being <see cref="client"/> and <see cref="server"/>.
    /// The <see cref="client"/> member provides easy access to features and systems relevant for your "client" that is the application the end user is actually playing e.g. your game.
    /// This would include features such as overlay, friends, clans, stats, achievements, etc.
    /// <see cref="server"/> in contrast deals with the configuration of Steamworks Game Server features and only comes into play for server builds.
    /// Note that the <see cref="server"/> member and its functionality are stripped out of client builds, that is it is only accessible in a server build and in the Unity Editor</para>
    /// </remarks>
    [HelpURL("https://kb.heathen.group/assets/steamworks/objects/steam-settings")]
    public class SteamSettings : ScriptableObject
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RunTimeInit()
        {
            current = null;
            behaviour = null;
        }

        #region Internal Classes
        /// <summary>
        /// Simple wrapper around common Steam related colors such as used on persona status indicators.
        /// </summary>
        public static class Colors
        {
            public static Color SteamBlue = new(0.2f, 0.60f, 0.93f, 1f);
            public static Color SteamGreen = new(0.2f, 0.42f, 0.2f, 1f);
            public static Color BrightGreen = new(0.4f, 0.84f, 0.4f, 1f);
            public static Color HalfAlpha = new(1f, 1f, 1f, 0.5f);
            public static Color ErrorRed = new(1, 0.5f, 0.5f, 1);
        }

        /// <summary>
        /// configuration settings and features unique to the Server API
        /// </summary>
        /// <remarks>
        /// Note that this is not available in client builds and can only be accessed in server and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        [Serializable]
        public class GameServer
        {
            public bool LoggedOn { get; private set; }
            public SteamGameServerConfiguration Configuration => new SteamGameServerConfiguration
            {
                autoInitialize = autoInitialize,
                anonymousServerLogin = anonymousServerLogin,
                autoLogon = autoLogon,
                botPlayerCount = botPlayerCount,
                enableHeartbeats = enableHeartbeats,
                gameData = gameData,
                gameDescription = gameDescription,
                gameDirectory = gameDirectory,
                gamePort = gamePort,
                gameServerToken = gameServerToken,
                ip = ip,
                isDedicated = isDedicated,
                isPasswordProtected = isPasswordProtected,
                mapName = mapName,
                maxPlayerCount = maxPlayerCount,
                queryPort = queryPort,
                rulePairs = rulePairs.ToArray(),
                serverName = serverName,
                serverVersion = serverVersion,
                spectatorPort = spectatorPort,
                spectatorServerName = spectatorServerName,
                supportSpectators = supportSpectators,
                usingGameServerAuthApi = usingGameServerAuthApi
            };

            public bool autoInitialize = false;
            public bool autoLogon = false;
            public uint ip = 0;
            public ushort queryPort = 27016;
            public ushort gamePort = 27015;
            public string serverVersion = "1.0.0.0";
            public ushort spectatorPort = 27017;

            public CSteamID ServerId => SteamGameServer.GetSteamID();
            public bool usingGameServerAuthApi = false;
            public bool enableHeartbeats = true;
            public bool supportSpectators = false;
            public string spectatorServerName = "Usually GameDescription + Spectator";
            public bool anonymousServerLogin = false;
            public string gameServerToken = "See https://steamcommunity.com/dev/managegameservers";
            public bool isPasswordProtected = false;
            public string serverName = "My Server Name";
            public string gameDescription = "Usually the name of your game";
            public string gameDirectory = "e.g. its folder name";
            public bool isDedicated = false;
            public int maxPlayerCount = 4;
            public int botPlayerCount = 0;
            public string mapName = "";
            [Tooltip("A delimited string used for Matchmaking Filtering e.g. CoolPeopleOnly,NoWagonsAllowed.\nThe above represents 2 data points matchmaking will then filter accordingly\n... see Heathen Game Server Browser for more information.")]
            public string gameData;
            public List<StringKeyValuePair> rulePairs = new List<StringKeyValuePair>();

            public API.App.Server.DisconnectedEvent EventDisconnected => API.App.Server.eventDisconnected;
            public API.App.Server.ConnectedEvent EventConnected => API.App.Server.eventConnected;
            public API.App.Server.FailureEvent EventFailure => API.App.Server.eventFailure;
        }

        /// <summary>
        /// configuration settings and features unique to the Client API
        /// </summary>
        /// <remarks>
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// </para>
        /// </remarks>
        [Serializable]
        public class GameClient
        {
            #region Input System
            public List<InputActionSet> actionSets = new();
            public List<InputActionSetLayer> actionSetLayers = new();
            public List<InputAction> actions = new();
            #endregion

            #region Inventory System
            /// <summary>
            /// The collection of inventory items registered for this application.
            /// </summary>
            /// <remarks>
            /// See <see cref="inventory"/> for more information. This field simply access the <see cref="inventory"/> member for the <see cref="current"/> <see cref="SteamworksClientApiSettings"/> object.
            /// </remarks>
            public static InventorySettings Inventory => current.client.inventory;

            /// <summary>
            /// The Steamworks Inventory settings associated with this application.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Steamworks Inventory is a powerful and flexible system which can enable player driven economies, microtransaction systems, in game shops using in game currency and or real currency, etc.
            /// These concepts are advanced and depend on well structured design. For security reasons there are various limitations on what can be done with inventory without the support of a trusted server.
            /// That said it is possible to build robust MTX, player economy, etc. systems without a dedicated server backend by using Steamworks Inventory.
            /// </para>
            /// <para>
            /// As noted this is a very complex topic, you should read and understand Valve's documentation on Steamworks Inventory before designing your game around this feature.
            /// </para>
            /// </remarks>
            public InventorySettings inventory = new();
            #endregion

        }
        #endregion

        #region Static Access
        public static void Unload() => API.App.Unload();

        /// <summary>
        /// A reference to the initialized <see cref="SteamSettings"/> object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value gets set when the <see cref="Initialize"/> method is called which should be done by the <see cref="InitializeSteamworks"/>. Note that your app should have 1 <see cref="InitializeSteamworks"/> defined in a scene that is loaded once and is never reloaded, that is you should not put the <see cref="InitializeSteamworks"/> on a menu scene that will be reloaded multiple times during your games session life as this will break events and other features of the Steamworks API.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class ExampleBehaviour : MonoBehaviour
        /// {
        ///     public void SayMyName()
        ///     {
        ///         Debug.Log("This user's name is " + SystemSettings.Client.user.DisplayName);
        ///     }
        /// }
        /// </code>
        /// </example>
        public static SteamSettings current = null;
        public static InitializeSteamworks behaviour = null;
        /// <summary>
        /// The AppId_t value configured and initialized for.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the app id value the developer would have typed in to the Unity Editor when setting up the project.
        /// Note that hackers can easily modify this value to cause the Steamworks API to initialize as a different game or can use the steam_appid.txt to force the Steamworks API to register as a different ID.
        /// You can confirm what ID Valve sees this program as running as by calling <see cref="GetAppId"/> you can then compare this fixed value to insure your user is not attempting to manipulate your program.
        /// In addition if you are integrating deeply with the Steamworks API such as using stats, achievements, leaderboards and other features with a configuration specific to your app ID ... this will further insure that if a user manages to initialize as an app other than your App ID ... such as an attempt to pirate your game that these features will break insuring a degraded experience for pirates.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class ExampleBehaviour : MonoBehaviour
        /// {
        ///     public void AppIdTests()
        ///     {
        ///         Debug.Log("Configured to run as " + SystemSettings.ApplicationId + ", actually running as " + SystemSettings.GetAppId() );
        ///     }
        /// }
        /// </code>
        /// </example>
        public static AppData ApplicationId => current != null ? current.applicationId : default;

        /// <summary>
        /// Indicates an error with API initialization
        /// </summary>
        /// <remarks>
        /// If true than an error occurred during the initialization of the Steamworks API and normal functionality will not be possible.
        /// </remarks>
        public static bool HasInitializationError => API.App.HasInitializationError;

        /// <summary>
        /// Initialization error message if any
        /// </summary>
        /// <remarks>
        /// See <see cref="HasInitializationError"/> to determine if an error occurred, if so this message will describe possible causes.
        /// </remarks>
        public static string InitializationErrorMessage => API.App.InitializationErrorMessage;

        /// <summary>
        /// Indicates rather or not the Steamworks API is initialized
        /// </summary>
        /// <remarks>
        /// <para>This value gets set to true when <see cref="Initialize"/> is called by the <see cref="SteamworksClientApiSystem"/>.
        /// Note that if Steamworks API fails to initialize such as if the Steamworks client is not installed, running and logged in with a valid Steamworks user then the call to Init will fail and the <see cref="Initialized"/> value will remain false.</para>
        /// </remarks>
        public static bool Initialized => API.App.Initialized;

        /// <summary>
        /// Static access to the active <see cref="GameClient"/> object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="GameClient"/> object provides easy access to client specific functions such as the <see cref="Steam.UserData"/> for the local user ... you can access this via.
        /// <code>
        /// SteamSettings.Client.user
        /// </code>
        /// or you can fetch the <see cref="Steam.UserData"/> for any given user via code such as
        /// <code>
        /// SteamSettings.Client.GetUserData(ulong userId)
        /// </code>
        /// For more information please see the documentation on the <see cref="GameClient"/> object.
        /// </para>
        /// </remarks>
        public static GameClient Client => current == null ? null : current.client;

        /// <summary>
        /// Static access to the active <see cref="GameServer"/> object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="GameServer"/> object provides easy access to Steamworks Game Server configuration and server only features.
        /// Note that your server can be a Steamworks Game Server and not have to use the Steamworks Networking transports ... e.g. you can use any transport you like and host anywhere you like.
        /// Being a Steamworks Game Server simply means that your server has initialized the Steamworks API and registered its self against Valve's backend ... in addition if this server has an  IP address of a trusted server as defined in your app configuration on the Steamworks Portal,
        /// then it may perform GS only actions such as setting stats and achievements that are marked as GS only.
        /// </para>
        /// </remarks>
        public static GameServer Server => current == null ? null : current.server;

        /// <summary>
        /// The list of achievements registered for this application.
        /// </summary>
        /// <remarks>
        /// See <see cref="achievements"/> for more information. This field simply access the <see cref="achievements"/> member for the <see cref="current"/> <see cref="SteamworksClientApiSettings"/> object.
        /// </remarks>
        public static List<AchievementObject> Achievements => current == null ? null : current.achievements;

        /// <summary>
        /// The list of stats registered for this application.
        /// </summary>
        /// <remarks>
        /// See <see cref="stats"/> for more information. This field simply access the <see cref="stats"/> member for the <see cref="current"/> <see cref="SteamworksClientApiSettings"/> object.
        /// </remarks>
        public static List<StatObject> Stats => current == null ? null : current.stats;

        #region DLC System
        /// <summary>
        /// The list of dlc registered for this application.
        /// </summary>
        /// <remarks>
        /// See <see cref="dlc"/> for more information. This field simply access the <see cref="dlc"/> member for the <see cref="current"/> <see cref="SteamworksClientApiSettings"/> object.
        /// </remarks>
        public static List<DownloadableContentObject> DLC => current.dlc;
        #endregion

        #region Leaderboard System
        /// <summary>
        /// The list of leaderboards registered for this application.
        /// </summary>
        /// <remarks>
        /// See <see cref="leaderboards"/> for more information. This field simply access the <see cref="leaderboards"/> member for the <see cref="current"/> <see cref="SteamworksClientApiSettings"/> object.
        /// </remarks>
        public static List<LeaderboardObject> Leaderboards => current.leaderboards;
        #endregion

        #endregion

        #region Instanced Members
        /// <summary>
        /// The current application ID
        /// </summary>
        /// <remarks>
        /// <para>It is important that this is set to your game's appId.
        /// Note that when working in Unity Editor you need to change this value in the <see cref="SteamworksClientApiSettings"/> object your using but also in the steam_appid.txt file located in the root of your project.
        /// You can read more about the steam_appid.txt file here <a href="https://heathen-engineering.mn.co/posts/steam_appidtxt"/></para>
        /// </remarks>
        public uint applicationId = 480;
        public int callbackTick_Milliseconds = 15;

        /// <summary>
        /// Used in various processes to determine the level of detail to log
        /// </summary>
        public bool isDebugging = false;

        /// <summary>
        /// Contains server side functionality and is not available in client builds
        /// </summary>
        /// <remarks>
        /// Note that this is not available in client builds and can only be accessed in server and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public GameServer server = new();

        /// <summary>
        /// Contains client side functionality
        /// </summary>
        /// <remarks>
        /// Note that this is not available in server builds and can only be accessed in client and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public GameClient client = new();

        /// <summary>
        /// The registered stats associated with this configuration
        /// </summary>
        /// <remarks>
        /// This collection is used by Valve callbacks to match incoming stat updates and records the value against the appropriate stat.
        /// Put more simply if a stat is listed here then the system will update that <see cref="StatObject"/> object with score changes as that information comes in from the Valve backend insuring that these <see cref="StatObject"/> objects are an up to date snap shot of the local user's stat value.
        /// For servers these objects simplify fetching and setting stat values for targeted users but of course doesn't cache values for a local user since server's have no local user.
        /// </remarks>
        public List<StatObject> stats = new();

        /// <summary>
        /// The registered achievements associated with this configuration
        /// </summary>
        /// <remarks>
        /// This collection is used by Valve callbacks to match incoming stat updates and records the value against the appropriate achievement.
        /// Put more simply if a stat is listed here then the system will update that <see cref="SteamAchievementData"/> object with state changes as that information comes in from the Valve backend insuring that these <see cref="AchievementObject"/> objects are an up to date snap shot of the local user's achievement value.
        /// For servers these objects simplify fetching and setting stat values for targeted users but of course doesn't cache values for a local user since server's have no local user.
        /// </remarks>
        public List<AchievementObject> achievements = new();


        #region DLC System
        public List<DownloadableContentObject> dlc = new();
        #endregion

        #region Leaderboard System
        /// <summary>
        /// The list of leaderboards registered for this application.
        /// </summary>
        /// <remarks>
        /// Leaderboards are registered by name and on initialization are resolved to there respective leaderboard IDs.
        /// These IDs can be used in later operations, consequently this means that adding a leaderboard to this list after initialization will require that you register the board manually by calling its <see cref="LeaderboardObject.Register"/> method.
        /// </remarks>
        public List<LeaderboardObject> leaderboards = new();
        #endregion

        #endregion

        #region Internals
        /// <summary>
        /// Initialization logic for the Steam API
        /// </summary>
        public void Initialize()
        {
            if (Initialized)
                return;
            else
            {
                current = this;
                API.App.isDebugging = isDebugging;
                API.App.callbackTick_Milliseconds = callbackTick_Milliseconds;

#if !UNITY_SERVER
                #region Client
                if (client == null)
                {
                    Debug.LogError("Invalid SteamSettings object detected. the client object is null and will not initialize properly, aborting initialization.");
                    return;
                }

                if (leaderboards == null)
                {
                    Debug.LogError("Invalid SteamSettings object detected. the leaderboards list is null and will not be initialized.");
                    leaderboards = new List<LeaderboardObject>();
                }
                else
                {
                    leaderboards.RemoveAll(p => p == null);
                }

                API.App.Client.Initialize(applicationId, client.inventory, leaderboards.ToArray(), client.actions.ToArray());

                foreach (var action in client.actions)
                    API.Input.Client.AddInput(action.ActionName, action.Type);

                #endregion
#else
                #region Server
                API.App.Server.Initialize(applicationId, Server.Configuration);
                #endregion
#endif

            }
        }
        #endregion

#if UNITY_EDITOR
        public void HandleSettingsInventoryDefinitionUpdate()
        {
            Debug.Log("Processing inventory item definition update!");
            client.inventory.UpdateItemDefinitions(isDebugging);
            API.Inventory.Client.EventSteamInventoryDefinitionUpdate.RemoveListener(HandleSettingsInventoryDefinitionUpdate);
        }

        public const string k_SettingsFolder = "Settings";
        public const string k_SteamworksSettingsName = "SteamMain.asset";
        public const string k_DemoSettingsName = "SteamDemo.asset";
        public const string k_PlaytestSettingsName = "Playtest ";
        public const string k_SteamworksMainPath = "Assets/" + k_SettingsFolder + "/" + k_SteamworksSettingsName;
        public const string k_SteamworksDemoPath = "Assets/" + k_SettingsFolder + "/" + k_DemoSettingsName;

        public static SteamSettings GetOrCreateSettings()
        {
            var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<SteamSettings>(k_SteamworksMainPath);
            if (settings == null)
            {
                if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/" + k_SettingsFolder))
                    UnityEditor.AssetDatabase.CreateFolder("Assets", k_SettingsFolder);

                settings = ScriptableObject.CreateInstance<SteamSettings>();
                UnityEditor.AssetDatabase.CreateAsset(settings, k_SteamworksMainPath);
                UnityEditor.AssetDatabase.SaveAssets();
                SetDefault(settings);
                UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(settings), UnityEditor.ImportAssetOptions.ForceUpdate);
            }
            return settings;
        }

        public static List<SteamSettings> GetPlayTestSettings()
        {
            var results = UnityEditor.AssetDatabase.FindAssets("t:SteamSettings", new string[] { "Assets/" + k_SettingsFolder });

            List<SteamSettings> foundPlaytest = new();
            foreach (var playtest in results)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(playtest);
                if (path.Contains("Assets/" + k_SettingsFolder + "/" + k_PlaytestSettingsName))
                {
                    var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<SteamSettings>(path);
                    foundPlaytest.Add(settings);
                    UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(settings), UnityEditor.ImportAssetOptions.ForceUpdate);
                }
            }

            return foundPlaytest;
        }

        public static List<SteamSettings> GetAllSettings()
        {
            var results = UnityEditor.AssetDatabase.FindAssets("t:SteamSettings", new string[] { "Assets/" + k_SettingsFolder });

            List<SteamSettings> foundPlaytest = new();
            foreach (var playtest in results)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(playtest);
                var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<SteamSettings>(path);
                foundPlaytest.Add(settings);
            }

            return foundPlaytest;
        }

        public static SteamSettings GetOrCreatePlaytestSettings(string name)
        {
            var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<SteamSettings>("Assets/" + k_SettingsFolder + "/" + k_PlaytestSettingsName + name + ".asset");
            if (settings == null)
            {
                if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/" + k_SettingsFolder))
                    UnityEditor.AssetDatabase.CreateFolder("Assets", k_SettingsFolder);

                settings = ScriptableObject.CreateInstance<SteamSettings>();
                UnityEditor.AssetDatabase.CreateAsset(settings, "Assets/" + k_SettingsFolder + "/" + k_PlaytestSettingsName + name + ".asset");
                UnityEditor.AssetDatabase.SaveAssets();
                SetClear(settings);
                UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(settings), UnityEditor.ImportAssetOptions.ForceUpdate);
            }
            return settings;
        }

        public static SteamSettings GetOrCreateDemoSettings()
        {
            var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<SteamSettings>(k_SteamworksDemoPath);
            if (settings == null)
            {
                if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/" + k_SettingsFolder))
                    UnityEditor.AssetDatabase.CreateFolder("Assets", k_SettingsFolder);

                settings = ScriptableObject.CreateInstance<SteamSettings>();
                UnityEditor.AssetDatabase.CreateAsset(settings, k_SteamworksDemoPath);
                UnityEditor.AssetDatabase.SaveAssets();
                SetClear(settings);
                UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(settings), UnityEditor.ImportAssetOptions.ForceUpdate);
            }
            return settings;
        }

        public static bool HasDemoSettings()
        {
            return UnityEditor.AssetDatabase.AssetPathExists(k_SteamworksDemoPath);
        }

        public static UnityEditor.SerializedObject GetSerializedSettings()
        {
            return new UnityEditor.SerializedObject(GetOrCreateSettings());
        }

        public static void SetDefault(SteamSettings settings)
        {
            settings.client ??= new();
            settings.server ??= new();

            //Reset App ID
            var appIdPath = new System.IO.DirectoryInfo(Application.dataPath).Parent.FullName + "/steam_appid.txt";
            System.IO.File.WriteAllText(appIdPath, "480");
            UnityEditor.Undo.RecordObject(settings, "editor");
            settings.applicationId = 480;
            UnityEditor.EditorUtility.SetDirty(settings);
            Debug.LogWarning("When updating the App ID we also update the steam_appid.txt for you. You must restart Unity and Visual Studio for this change to take full effect as seen from the Steamworks Client.");

            //Rest Achievements
            var achList = settings.achievements.ToArray();

            foreach (var ach in achList)
            {
                if (UnityEditor.AssetDatabase.GetAssetPath(ach) == UnityEditor.AssetDatabase.GetAssetPath(settings))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(ach);
                }
            }

            settings.achievements.Clear();

            //ACH_TRAVEL_FAR_ACCUM
            var ACH_TRAVEL_FAR_ACCUM_Obj = ScriptableObject.CreateInstance<AchievementObject>();
            ACH_TRAVEL_FAR_ACCUM_Obj.name = "[Ach] ACH_TRAVEL_FAR_ACCUM";
            ACH_TRAVEL_FAR_ACCUM_Obj.Id = "ACH_TRAVEL_FAR_ACCUM";
            UnityEditor.AssetDatabase.AddObjectToAsset(ACH_TRAVEL_FAR_ACCUM_Obj, settings);
            settings.achievements.Add(ACH_TRAVEL_FAR_ACCUM_Obj);

            //ACH_TRAVEL_FAR_SINGLE
            var ACH_TRAVEL_FAR_SINGLE_Obj = ScriptableObject.CreateInstance<AchievementObject>();
            ACH_TRAVEL_FAR_SINGLE_Obj.name = "[Ach] ACH_TRAVEL_FAR_SINGLE";
            ACH_TRAVEL_FAR_SINGLE_Obj.Id = "ACH_TRAVEL_FAR_SINGLE";
            UnityEditor.AssetDatabase.AddObjectToAsset(ACH_TRAVEL_FAR_SINGLE_Obj, settings);
            settings.achievements.Add(ACH_TRAVEL_FAR_SINGLE_Obj);

            //ACH_WIN_100_GAMES
            var ACH_WIN_100_GAMES_Obj = ScriptableObject.CreateInstance<AchievementObject>();
            ACH_WIN_100_GAMES_Obj.name = "[Ach] ACH_WIN_100_GAMES";
            ACH_WIN_100_GAMES_Obj.Id = "ACH_WIN_100_GAMES";
            UnityEditor.AssetDatabase.AddObjectToAsset(ACH_WIN_100_GAMES_Obj, settings);
            settings.achievements.Add(ACH_WIN_100_GAMES_Obj);

            //ACH_WIN_ONE_GAME
            var ACH_WIN_ONE_GAME_Obj = ScriptableObject.CreateInstance<AchievementObject>();
            ACH_WIN_ONE_GAME_Obj.name = "[Ach] ACH_WIN_ONE_GAME";
            ACH_WIN_ONE_GAME_Obj.Id = "ACH_WIN_ONE_GAME";
            UnityEditor.AssetDatabase.AddObjectToAsset(ACH_WIN_ONE_GAME_Obj, settings);
            settings.achievements.Add(ACH_WIN_ONE_GAME_Obj);

            //NEW_ACHIEVEMENT_0_4
            var NEW_ACHIEVEMENT_0_4_Obj = ScriptableObject.CreateInstance<AchievementObject>();
            NEW_ACHIEVEMENT_0_4_Obj.name = "[Ach] NEW_ACHIEVEMENT_0_4";
            NEW_ACHIEVEMENT_0_4_Obj.Id = "NEW_ACHIEVEMENT_0_4";
            UnityEditor.AssetDatabase.AddObjectToAsset(NEW_ACHIEVEMENT_0_4_Obj, settings);
            settings.achievements.Add(NEW_ACHIEVEMENT_0_4_Obj);

            //Rest DLC
            var dlcList = settings.dlc.ToArray();

            foreach (var dlc in dlcList)
            {
                if (UnityEditor.AssetDatabase.GetAssetPath(dlc) == UnityEditor.AssetDatabase.GetAssetPath(settings))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(dlc);
                }
            }

            settings.dlc.Clear();

            var dlcObj = ScriptableObject.CreateInstance<DownloadableContentObject>();
            dlcObj.name = "[DLC] pieterw test DLC";
            dlcObj.data = 110902;
            UnityEditor.AssetDatabase.AddObjectToAsset(dlcObj, settings);
            settings.dlc.Add(dlcObj);

            dlcObj = ScriptableObject.CreateInstance<DownloadableContentObject>();
            dlcObj.name = "[DLC] ticket test DLC";
            dlcObj.data = 447130;
            UnityEditor.AssetDatabase.AddObjectToAsset(dlcObj, settings);
            settings.dlc.Add(dlcObj);

            //Rest Inputs
            var actionList = settings.client.actions.ToArray();

            foreach (var action in actionList)
            {
                if (UnityEditor.AssetDatabase.GetAssetPath(action) == UnityEditor.AssetDatabase.GetAssetPath(settings))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(action);
                }
            }

            settings.client.actions.Clear();

            var layersList = settings.client.actionSetLayers.ToArray();

            foreach (var layer in layersList)
            {
                if (UnityEditor.AssetDatabase.GetAssetPath(layer) == UnityEditor.AssetDatabase.GetAssetPath(settings))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(layer);
                }
            }

            settings.client.actionSetLayers.Clear();

            var setList = settings.client.actionSets.ToArray();

            foreach (var set in setList)
            {
                if (UnityEditor.AssetDatabase.GetAssetPath(set) == UnityEditor.AssetDatabase.GetAssetPath(settings))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(set);
                }
            }

            settings.client.actionSets.Clear();

            var nItem = ScriptableObject.CreateInstance<InputAction>();
            nItem.ActionName = "analog_controls";
            nItem.name = "[Input-Action] analog_controls";
            nItem.Type = InputActionType.Analog;
            UnityEditor.AssetDatabase.AddObjectToAsset(nItem, settings);
            settings.client.actions.Add(nItem);

            nItem = ScriptableObject.CreateInstance<InputAction>();
            nItem.ActionName = "backward_thrust";
            nItem.name = "[Input-Action] backward_thrust";
            nItem.Type = InputActionType.Digital;
            UnityEditor.AssetDatabase.AddObjectToAsset(nItem, settings);
            settings.client.actions.Add(nItem);

            nItem = ScriptableObject.CreateInstance<InputAction>();
            nItem.ActionName = "fire_lasers";
            nItem.name = "[Input-Action] fire_lasers";
            nItem.Type = InputActionType.Digital;
            UnityEditor.AssetDatabase.AddObjectToAsset(nItem, settings);
            settings.client.actions.Add(nItem);

            nItem = ScriptableObject.CreateInstance<InputAction>();
            nItem.ActionName = "forward_thrust";
            nItem.name = "[Input-Action] forward_thrust";
            nItem.Type = InputActionType.Digital;
            UnityEditor.AssetDatabase.AddObjectToAsset(nItem, settings);
            settings.client.actions.Add(nItem);

            nItem = ScriptableObject.CreateInstance<InputAction>();
            nItem.ActionName = "menu_cancel";
            nItem.name = "[Input-Action] menu_cancel";
            nItem.Type = InputActionType.Digital;
            UnityEditor.AssetDatabase.AddObjectToAsset(nItem, settings);
            settings.client.actions.Add(nItem);

            nItem = ScriptableObject.CreateInstance<InputAction>();
            nItem.ActionName = "menu_down";
            nItem.name = "[Input-Action] menu_down";
            nItem.Type = InputActionType.Digital;
            UnityEditor.AssetDatabase.AddObjectToAsset(nItem, settings);
            settings.client.actions.Add(nItem);

            nItem = ScriptableObject.CreateInstance<InputAction>();
            nItem.ActionName = "menu_left";
            nItem.name = "[Input-Action] menu_left";
            nItem.Type = InputActionType.Digital;
            UnityEditor.AssetDatabase.AddObjectToAsset(nItem, settings);
            settings.client.actions.Add(nItem);

            nItem = ScriptableObject.CreateInstance<InputAction>();
            nItem.ActionName = "menu_right";
            nItem.name = "[Input-Action] menu_right";
            nItem.Type = InputActionType.Digital;
            UnityEditor.AssetDatabase.AddObjectToAsset(nItem, settings);
            settings.client.actions.Add(nItem);

            nItem = ScriptableObject.CreateInstance<InputAction>();
            nItem.ActionName = "menu_select";
            nItem.name = "[Input-Action] menu_select";
            nItem.Type = InputActionType.Digital;
            UnityEditor.AssetDatabase.AddObjectToAsset(nItem, settings);
            settings.client.actions.Add(nItem);

            nItem = ScriptableObject.CreateInstance<InputAction>();
            nItem.ActionName = "menu_up";
            nItem.name = "[Input-Action] menu_up";
            nItem.Type = InputActionType.Digital;
            UnityEditor.AssetDatabase.AddObjectToAsset(nItem, settings);
            settings.client.actions.Add(nItem);

            nItem = ScriptableObject.CreateInstance<InputAction>();
            nItem.ActionName = "pause_menu";
            nItem.name = "[Input-Action] pause_menu";
            nItem.Type = InputActionType.Digital;
            UnityEditor.AssetDatabase.AddObjectToAsset(nItem, settings);
            settings.client.actions.Add(nItem);

            nItem = ScriptableObject.CreateInstance<InputAction>();
            nItem.ActionName = "turn_left";
            nItem.name = "[Input-Action] turn_left";
            nItem.Type = InputActionType.Digital;
            UnityEditor.AssetDatabase.AddObjectToAsset(nItem, settings);
            settings.client.actions.Add(nItem);

            nItem = ScriptableObject.CreateInstance<InputAction>();
            nItem.ActionName = "turn_right";
            nItem.name = "[Input-Action] turn_right";
            nItem.Type = InputActionType.Digital;
            UnityEditor.AssetDatabase.AddObjectToAsset(nItem, settings);
            settings.client.actions.Add(nItem);

            var nSet = ScriptableObject.CreateInstance<InputActionSet>();
            nSet.setName = "menu_controls";
            nSet.name = "[Input-Set] menu_controls";
            UnityEditor.AssetDatabase.AddObjectToAsset(nSet, settings);
            settings.client.actionSets.Add(nSet);

            nSet = ScriptableObject.CreateInstance<InputActionSet>();
            nSet.setName = "ship_controls";
            nSet.name = "[Input-Set] ship_controls";
            UnityEditor.AssetDatabase.AddObjectToAsset(nSet, settings);
            settings.client.actionSets.Add(nSet);

            var nLayer = ScriptableObject.CreateInstance<InputActionSetLayer>();
            nLayer.layerName = "thrust_action_layer";
            nLayer.name = "[Input-SetLayer] thrust_action_layer";
            UnityEditor.AssetDatabase.AddObjectToAsset(nLayer, settings);
            settings.client.actionSetLayers.Add(nLayer);

            //Reset Boards
            var boardList = settings.leaderboards.ToArray();

            foreach (var board in boardList)
            {
                if (UnityEditor.AssetDatabase.GetAssetPath(board) == UnityEditor.AssetDatabase.GetAssetPath(settings))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(board);
                }
            }

            settings.leaderboards.Clear();

            //Reset Stats
            var statsList = settings.stats.ToArray();

            foreach (var stat in statsList)
            {
                if (UnityEditor.AssetDatabase.GetAssetPath(stat) == UnityEditor.AssetDatabase.GetAssetPath(settings))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(stat);
                }
            }

            settings.stats.Clear();

            LeaderboardObject nBoard = ScriptableObject.CreateInstance<LeaderboardObject>();
            nBoard.name = "[LdrBrd] Feet Traveled";
            nBoard.apiName = "Feet Traveled";
            UnityEditor.AssetDatabase.AddObjectToAsset(nBoard, settings);
            settings.leaderboards.Add(nBoard);

            //AverageSpeed
            AvgRateStatObject AverageSpeedStat = ScriptableObject.CreateInstance<AvgRateStatObject>();
            AverageSpeedStat.name = "[Stat AvgRate] AverageSpeed";
            AverageSpeedStat.data = "AverageSpeed";
            settings.stats.Add(AverageSpeedStat);
            UnityEditor.AssetDatabase.AddObjectToAsset(AverageSpeedStat, settings);

            //FeetTraveled
            FloatStatObject FeetTraveledStat = ScriptableObject.CreateInstance<FloatStatObject>();
            FeetTraveledStat.name = "[Stat Float] FeetTraveled";
            FeetTraveledStat.data = "FeetTraveled";
            UnityEditor.AssetDatabase.AddObjectToAsset(FeetTraveledStat, settings);
            settings.stats.Add(FeetTraveledStat);

            //MaxFeetTraveled
            FloatStatObject MaxFeetTraveledStat = ScriptableObject.CreateInstance<FloatStatObject>();
            MaxFeetTraveledStat.name = "[Stat Float] MaxFeetTraveled";
            MaxFeetTraveledStat.data = "MaxFeetTraveled";
            UnityEditor.AssetDatabase.AddObjectToAsset(MaxFeetTraveledStat, settings);
            settings.stats.Add(MaxFeetTraveledStat);

            //NumGames
            IntStatObject NumGamesStat = ScriptableObject.CreateInstance<IntStatObject>();
            NumGamesStat.name = "[Stat Int] NumGames";
            NumGamesStat.data = "NumGames";
            UnityEditor.AssetDatabase.AddObjectToAsset(NumGamesStat, settings);
            settings.stats.Add(NumGamesStat);

            //NumLosses
            IntStatObject NumLossesStat = ScriptableObject.CreateInstance<IntStatObject>();
            NumLossesStat.name = "[Stat Int] NumLosses";
            NumLossesStat.data = "NumLosses";
            UnityEditor.AssetDatabase.AddObjectToAsset(NumLossesStat, settings);
            settings.stats.Add(NumLossesStat);

            //NumWins
            IntStatObject NumWinsStat = ScriptableObject.CreateInstance<IntStatObject>();
            NumWinsStat.name = "[Stat Int] NumWins";
            NumWinsStat.data = "NumWins";
            UnityEditor.AssetDatabase.AddObjectToAsset(NumWinsStat, settings);
            settings.stats.Add(NumWinsStat);

            //Unused2
            IntStatObject Unused2Stat = ScriptableObject.CreateInstance<IntStatObject>();
            Unused2Stat.name = "[Stat Int] Unused2";
            Unused2Stat.data = "Unused2";
            UnityEditor.AssetDatabase.AddObjectToAsset(Unused2Stat, settings);
            settings.stats.Add(Unused2Stat);

            settings.isDebugging = true;

            settings.server.autoInitialize = true;
            settings.server.autoLogon = false;
            settings.server.ip = 0;
            settings.server.queryPort = 27016;
            settings.server.gamePort = 27015;
            settings.server.serverVersion = "1.0.0.0";
            settings.server.spectatorPort = 27017;
            settings.server.usingGameServerAuthApi = true;
            settings.server.enableHeartbeats = true;
            settings.server.supportSpectators = false;
            settings.server.spectatorServerName = "Usually GameDescription + Spectator";
            settings.server.anonymousServerLogin = true;
            settings.server.gameServerToken = "See https://steamcommunity.com/dev/managegameservers";
            settings.server.isPasswordProtected = false;
            settings.server.serverName = "My Server Name";
            settings.server.gameDescription = "Usually the name of your game";
            settings.server.gameDirectory = "e.g. its folder name";
            settings.server.isDedicated = true;
            settings.server.maxPlayerCount = 4;
            settings.server.botPlayerCount = 0;
            settings.server.mapName = "";
        }

        public static void SetClear(SteamSettings settings)
        {
            settings.client ??= new();
            settings.server ??= new();

            //Reset App ID
            var appIdPath = new System.IO.DirectoryInfo(Application.dataPath).Parent.FullName + "/steam_appid.txt";
            UnityEditor.Undo.RecordObject(settings, "editor");
            settings.applicationId = 0;
            UnityEditor.EditorUtility.SetDirty(settings);

            //Rest Achievements
            var achList = settings.achievements.ToArray();

            foreach (var ach in achList)
            {
                if (UnityEditor.AssetDatabase.GetAssetPath(ach) == UnityEditor.AssetDatabase.GetAssetPath(settings))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(ach);
                }
            }

            settings.achievements.Clear();

            //Rest DLC
            var dlcList = settings.dlc.ToArray();

            foreach (var dlc in dlcList)
            {
                if (UnityEditor.AssetDatabase.GetAssetPath(dlc) == UnityEditor.AssetDatabase.GetAssetPath(settings))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(dlc);
                }
            }

            settings.dlc.Clear();

            //Rest Inputs
            var actionList = settings.client.actions.ToArray();

            foreach (var action in actionList)
            {
                if (UnityEditor.AssetDatabase.GetAssetPath(action) == UnityEditor.AssetDatabase.GetAssetPath(settings))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(action);
                }
            }

            settings.client.actions.Clear();

            var layersList = settings.client.actionSetLayers.ToArray();

            foreach (var layer in layersList)
            {
                if (UnityEditor.AssetDatabase.GetAssetPath(layer) == UnityEditor.AssetDatabase.GetAssetPath(settings))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(layer);
                }
            }

            settings.client.actionSetLayers.Clear();

            var setList = settings.client.actionSets.ToArray();

            foreach (var set in setList)
            {
                if (UnityEditor.AssetDatabase.GetAssetPath(set) == UnityEditor.AssetDatabase.GetAssetPath(settings))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(set);
                }
            }

            settings.client.actionSets.Clear();

            //Reset Boards
            var boardList = settings.leaderboards.ToArray();

            foreach (var board in boardList)
            {
                if (UnityEditor.AssetDatabase.GetAssetPath(board) == UnityEditor.AssetDatabase.GetAssetPath(settings))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(board);
                }
            }

            settings.leaderboards.Clear();

            //Reset Stats
            var statsList = settings.stats.ToArray();

            foreach (var stat in statsList)
            {
                if (UnityEditor.AssetDatabase.GetAssetPath(stat) == UnityEditor.AssetDatabase.GetAssetPath(settings))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(stat);
                }
            }

            settings.stats.Clear();

            settings.isDebugging = true;

            settings.server.autoInitialize = true;
            settings.server.autoLogon = false;
            settings.server.ip = 0;
            settings.server.queryPort = 27016;
            settings.server.gamePort = 27015;
            settings.server.serverVersion = "1.0.0.0";
            settings.server.spectatorPort = 27017;
            settings.server.usingGameServerAuthApi = true;
            settings.server.enableHeartbeats = true;
            settings.server.supportSpectators = false;
            settings.server.spectatorServerName = "Usually GameDescription + Spectator";
            settings.server.anonymousServerLogin = true;
            settings.server.gameServerToken = "See https://steamcommunity.com/dev/managegameservers";
            settings.server.isPasswordProtected = false;
            settings.server.serverName = "My Server Name";
            settings.server.gameDescription = "Usually the name of your game";
            settings.server.gameDirectory = "e.g. its folder name";
            settings.server.isDedicated = true;
            settings.server.maxPlayerCount = 4;
            settings.server.botPlayerCount = 0;
            settings.server.mapName = "";
        }
#endif
    }
}
#endif