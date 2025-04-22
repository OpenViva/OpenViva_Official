#if !DISABLESTEAMWORKS
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Heathen.SteamworksIntegration.Editors
{
    public class HeathenSteamworksMenuItems
    {
        [InitializeOnLoadMethod]
        public static void CheckForSteamworksInstall()
        {
            StartCoroutine(ValidateAndInstall());
        }

        private static IEnumerator ValidateAndInstall()
        {
            var listProc = Client.List();

            while (!listProc.IsCompleted)
                yield return null;

            bool needsRemove = false;
            if (listProc.Status == StatusCode.Success)
            {
                var steamworksComplete = listProc.Result.FirstOrDefault(p => p.name == "com.heathen.steamworks");
                if (steamworksComplete != null)
                    SessionState.SetString("com.heathen.steamworks-version", steamworksComplete.version);

                if (listProc.Result.Any(p => p.name == "com.heathen.steamworksfoundation"))
                    needsRemove = true;
            }
            else
                Debug.LogError("Failed to check Package Manager dependencies: " + listProc.Error.message);

            if (needsRemove)
            {
                if (EditorUtility.DisplayDialog("Heathen Installer", "Steamworks Foundation appears to be installed and must be removed before continuing. Should we uninstall Steamworks Foundation now?", "Uninstall", "No"))
                {
                    var removeProc = Client.Remove("com.heathen.steamworksfoundation");

                    while (!removeProc.IsCompleted)
                        yield return null;
                }
            }
#if !STEAMWORKSNET
            if (EditorUtility.DisplayDialog("Heathen Installer", "Steam API not found, Steamworks.NET must be installed via the Unity Package Manager for this asset to work correctly. Would you like to install it now?", "Install Steamworks.NET", "Cancel"))
            {
                yield return null;
                AddRequest steamProc = null;

                if (!SessionState.GetBool("SteamInstall", false))
                {
                    SessionState.SetBool("SteamInstall", true);
                    steamProc = Client.Add("https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net");
                }

                if (steamProc.Status == StatusCode.Failure)
                    Debug.LogError("PackageManager Steamworks.NET install failed, Error Message: " + steamProc.Error.message);
                else if (steamProc.Status == StatusCode.Success)
                    Debug.Log("Steamworks.NET " + steamProc.Result.version + " installation complete");
                else
                {
                    Debug.Log("Installing Steamworks.NET ...");
                    while (steamProc.Status == StatusCode.InProgress)
                    {
                        yield return null;
                    }
                }

                if (steamProc.Status == StatusCode.Failure)
                    Debug.LogError("PackageManager Steamworks.NET install failed, Error Message: " + steamProc.Error.message);
                else if (steamProc.Status == StatusCode.Success)
                    Debug.Log("Steamworks.NET " + steamProc.Result.version + " installation complete");

                SessionState.SetBool("SteamInstall", false);
            }
#endif
        }

        [MenuItem("Help/Heathen/Steamworks/Update All Requirements (Package Manager)", priority = 0)]
        public static void InstallRequirements()
        {
            if (!SessionState.GetBool("SteamInstall", false))
            {
                StartCoroutine(InstallAll());
            }
        }
        [MenuItem("Help/Heathen/Steamworks/Update Steamworks.NET (Package Manager)", priority = 2)]
        public static void InstallSteamworksMenuItem()
        {
            if (!SessionState.GetBool("SteamInstall", false))
            {
                StartCoroutine(InstallSteamworks());
            }
        }
        [MenuItem("Help/Heathen/Steamworks/Documentation", priority = 3)]
        public static void Documentation()
        {
            Application.OpenURL("https://kb.heathen.group/assets/steamworks");
        }

        [MenuItem("Help/Heathen/Steamworks/Support", priority = 4)]
        public static void Support()
        {
            Application.OpenURL("https://discord.gg/RMGtDXV");
        }

        private static IEnumerator InstallAll()
        {
            yield return null;
            AddRequest steamProc = null;

            if (!SessionState.GetBool("SteamInstall", false))
            {
                SessionState.SetBool("SteamInstall", true);
                steamProc = Client.Add("https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net");
            }

            if (steamProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager Steamworks.NET install failed, Error Message: " + steamProc.Error.message);
            else if (steamProc.Status == StatusCode.Success)
                Debug.Log("Steamworks.NET " + steamProc.Result.version + " installation complete");
            else
            {
                Debug.Log("Installing Steamworks.NET ...");
                while (steamProc.Status == StatusCode.InProgress)
                {
                    yield return null;
                }
            }

            if (steamProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager Steamworks.NET install failed, Error Message: " + steamProc.Error.message);
            else if (steamProc.Status == StatusCode.Success)
                Debug.Log("Steamworks.NET " + steamProc.Result.version + " installation complete");

            SessionState.SetBool("SteamInstall", false);
        }

        private static IEnumerator InstallSteamworks()
        {
            yield return null;
            AddRequest steamProc = null;

            if (!SessionState.GetBool("SteamInstall", false))
            {
                SessionState.SetBool("SteamInstall", true);
                steamProc = Client.Add("https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net");
            }

            if (steamProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager Steamworks.NET install failed, Error Message: " + steamProc.Error.message);
            else if (steamProc.Status == StatusCode.Success)
                Debug.Log("Steamworks.NET " + steamProc.Result.version + " installation complete");
            else
            {
                Debug.Log("Installing Steamworks.NET ...");
                while (steamProc.Status == StatusCode.InProgress)
                {
                    yield return null;
                }
            }

            if (steamProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager Steamworks.NET install failed, Error Message: " + steamProc.Error.message);
            else if (steamProc.Status == StatusCode.Success)
                Debug.Log("Steamworks.NET " + steamProc.Result.version + " installation complete");

            SessionState.SetBool("SteamInstall", false);
        }

        private static List<IEnumerator> coroutines;

        private static void StartCoroutine(IEnumerator handle)
        {
            if (coroutines == null)
            {
                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;
                coroutines = new List<IEnumerator>();
            }

            coroutines.Add(handle);
        }

        private static void EditorUpdate()
        {
            List<IEnumerator> done = new List<IEnumerator>();

            if (coroutines != null)
            {
                foreach (var e in coroutines)
                {
                    if (!e.MoveNext())
                        done.Add(e);
                    else
                    {
                        if (e.Current != null)
                            Debug.Log(e.Current.ToString());
                    }
                }
            }

            foreach (var d in done)
                coroutines.Remove(d);
        }
    }
}
#endif