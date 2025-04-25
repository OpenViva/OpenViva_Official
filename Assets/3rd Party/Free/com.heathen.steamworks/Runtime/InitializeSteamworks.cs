#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [HelpURL("https://kb.heathen.group/toolkit-for-steamworks/unity/getting-started#component")]
    [DisallowMultipleComponent]
    public class InitializeSteamworks : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        internal SteamSettings targetSettings;
        [SerializeField]
        [HideInInspector]
        internal SteamSettings mainSettings;
        [SerializeField]
        [HideInInspector]
        internal SteamSettings demoSettings;
        [SerializeField]
        [HideInInspector]
        internal List<SteamSettings> playtestSettings;

        private void Start()
        {
            if (targetSettings != null && !API.App.Initialized)
                targetSettings.Initialize();
            else
            {
#if !UNITY_EDITOR
                Debug.LogError("No settings found");
#else
                Debug.LogError($"No settings have been selected, either you have removed the previously selected settings or you have never set the desired settings on the Initialize Steamworks object.");
#endif
            }
        }

#if UNITY_EDITOR
        internal void RefreshSettings()
        {
            mainSettings = SteamSettings.GetOrCreateSettings();
            
            if(SteamSettings.HasDemoSettings())
                demoSettings = SteamSettings.GetOrCreateDemoSettings();
            else
                demoSettings = null;

            playtestSettings = SteamSettings.GetPlayTestSettings();
        }

        [UnityEditor.CustomEditor(typeof(InitializeSteamworks))]
        public class InitializeSteamworksEditor : UnityEditor.Editor
        {
            private InitializeSteamworks parent;

            public override void OnInspectorGUI()
            {
                EditorGUILayout.BeginHorizontal();
                if(UnityEditor.EditorGUILayout.LinkButton("Settings"))
                    UnityEditor.SettingsService.OpenProjectSettings("Project/Player/Steamworks");
                if (UnityEditor.EditorGUILayout.LinkButton("Inspector"))
                    EditorApplication.ExecuteMenuItem("Window/Steamworks/Inspector");
                EditorGUILayout.EndHorizontal();


                parent = target as InitializeSteamworks;
                parent.RefreshSettings();

                if (parent.targetSettings == null)
                    parent.targetSettings = parent.mainSettings;

                List<string> names = new List<string>();
                int index = 0;

                names.Add("Main");
                if (parent.demoSettings != null)
                {
                    if(parent.targetSettings == parent.demoSettings)
                        index = 1;
                    names.Add("Demo");
                }
                var currentLength = names.Count;
                if(parent.playtestSettings != null)
                {
                    foreach(var setting in  parent.playtestSettings)
                    {
                        if (parent.targetSettings == setting)
                            index = 1 + names.Count - currentLength;

                        names.Add(setting.name);
                    }
                }

                var newIndex = UnityEditor.EditorGUILayout.Popup(index, names.ToArray());
                if(newIndex != index)
                {
                    UnityEditor.Undo.RecordObject(target, "Active Settings");
                    if (newIndex == 0)
                        parent.targetSettings = parent.mainSettings;
                    else if (parent.demoSettings != null && newIndex == 1)
                        parent.targetSettings = parent.demoSettings;
                    else
                    {
                        if(newIndex - currentLength < parent.playtestSettings.Count)
                            parent.targetSettings = parent.playtestSettings[newIndex - currentLength];
                    }
                    UnityEditor.EditorUtility.SetDirty(target);
                }

                //base.OnInspectorGUI();
            }
        }
#endif
    }
}
#endif