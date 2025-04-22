#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [HelpURL("https://kb.heathen.group/assets/steamworks/for-unity-game-engine/components/steam-input-manager")]
    public class SteamInputManager : MonoBehaviour
    {
        public static SteamInputManager current;

        [Tooltip("If set to true then we will attempt to force Steam to use input for this app on start.\nThis is generally only needed in editor testing.")]
        [SerializeField]
        private bool forceInput = true;
        [Tooltip("If set to true the system will update every input action every frame for every controller found")]
        public bool autoUpdate = true;

        public ControllerDataEvent evtInputDataChanged;


        public static bool AutoUpdate
        {
            get => current != null ? current.autoUpdate : false;
            set 
            { 
                if(current != null)
                    current.autoUpdate = value;
            }
        }

        private static Steamworks.InputHandle_t[] controllers = null;
        public static List<InputControllerData> Controllers { get; private set; } = new List<InputControllerData>();

        private void Start()
        {
            current = this;

            API.Input.Client.EventInputDataChanged.AddListener(evtInputDataChanged.Invoke);

            if (!API.App.Initialized)
                API.App.evtSteamInitialized.AddListener(HandleInitialization);
            else
                HandleInitialization();
        }

        private void HandleInitialization()
        {
            API.App.evtSteamInitialized.RemoveListener(HandleInitialization);

            if (forceInput)
            {
                Application.OpenURL($"steam://forceinputappid/{API.App.Id}");
                Invoke("RefreshNow", 1);
            }
            else
                RefreshControllers();
        }

        private void OnDestroy()
        {
            if(current == this)
                current = null;

            API.Input.Client.EventInputDataChanged.RemoveListener(evtInputDataChanged.Invoke);

            if (forceInput)
                Application.OpenURL("steam://forceinputappid/0");
        }

        private void Update()
        {
            if (autoUpdate)
                UpdateAll();
        }

        public static void UpdateAll()
        {
            if (API.Input.Client.Initialized && controllers != null && controllers.Length > 0)
            {
                Controllers.Clear();
                foreach (var controller in controllers)
                    Controllers.Add(API.Input.Client.Update(controller));
            }
        }

        [ContextMenu("Refresh Controllers")]
        public void RefreshNow() => RefreshControllers();

        public static void RefreshControllers()
        {
            if (API.Input.Client.Initialized)
            {
                controllers = API.Input.Client.Controllers;
                Debug.Log($"Controllers refreshed found count {(controllers != null ? controllers.Length : 0)}");
            }
        }
    }
}
#endif