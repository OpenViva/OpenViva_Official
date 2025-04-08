#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.IO;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents the in game definition of a Steamworks DLC app.
    /// </summary>
    /// <remarks>Steamworks DLC or Downloadable Content is defined on the Steamworks API in your Steamworks Portal.
    /// Please carfully read <a href="https://partner.steamgames.com/doc/store/application/dlc">https://partner.steamgames.com/doc/store/application/dlc</a> before designing features are this concept.</remarks>
    [HelpURL("https://kb.heathen.group/assets/steamworks/downloadable-content-object")]
    [CreateAssetMenu(menuName = "Steamworks/Downloadable Content Object")]
    public class DownloadableContentObject : ScriptableObject
    {
        /// <summary>
        /// The root data of the DLC
        /// </summary>
        [SerializeField]
        public DlcData data;
        /// <summary>
        /// The DLC name
        /// </summary>
        public string Name => data.Name;
        /// <summary>
        /// Is the DLC available to the user
        /// </summary>
        public bool Available => data.Available;
        /// <summary>
        /// Is the current user 'subscribed' to this DLC.
        /// This indicates that the current user has right/license this DLC or not.
        /// </summary>
        public bool IsSubscribed => data.IsSubscribed;
        /// <summary>
        /// Is this DLC currently installed.
        /// </summary>
        public bool IsInstalled => data.IsInstalled;
        
        /// <summary>
        /// Returns the install location of the DLC
        /// </summary>
        /// <returns></returns>
        public DirectoryInfo GetInstallDirectory() => data.InstallDirectory;

        /// <summary>
        /// Updates the IsDownloading member and Returns the download progress of the DLC if any
        /// </summary>
        /// <returns></returns>
        public float GetDownloadProgress() => data.DownloadProgress;

        /// <summary>
        /// Gets the time of purchase
        /// </summary>
        /// <returns></returns>
        public DateTime GetEarliestPurchaseTime() => data.EarliestPurchaseTime;

        /// <summary>
        /// Installs the DLC
        /// </summary>
        public void Install() => data.Install();

        /// <summary>
        /// Uninstalls the DLC
        /// </summary>
        public void Uninstall() =>  data.Uninstall();

        /// <summary>
        /// Opens the store page to the DLC
        /// </summary>
        /// <param name="flag"></param>
        public void OpenStore(EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None) => data.OpenStore(flag);

        public override string ToString()
        {
            return Name + ":" + data.ToString();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(DownloadableContentObject))]
    public class DownloadContentObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var dlc = target as DownloadableContentObject;
            UnityEditor.EditorGUILayout.SelectableLabel("App ID: " + dlc.data.ToString());
        }
    }
#endif
}
#endif