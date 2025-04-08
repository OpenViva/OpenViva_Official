#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Functions for adding screenshots to the user's screenshot library.
    /// </summary>
    /// <remarks>
    /// https://partner.steamgames.com/doc/api/ISteamScreenshots
    /// </remarks>
    public static class Screenshots
    {
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                eventScreenshotReady = new ScreenshotReadyEvent();
                eventScreenshotRequested = new UnityEvent();
                m_ScreenshotRequested_t = null;
                m_ScreenshotReady_t = null;
            }

            /// <summary>
            /// Checks if the app is hooking screenshots, or if the Steam Overlay is handling them.
            /// When set this will call <see cref="HookScreenshots(bool)"/>
            /// </summary>
            public static bool IsScreenshotsHooked
            {
                get => SteamScreenshots.IsScreenshotsHooked();
                set => SteamScreenshots.HookScreenshots(value);
            }

            /// <summary>
            /// A screenshot successfully written or otherwise added to the library and can now be tagged.
            /// </summary>
            public static ScreenshotReadyEvent EventScreenshotReady
            {
                get
                {
                    if (m_ScreenshotReady_t == null)
                        m_ScreenshotReady_t = Callback<ScreenshotReady_t>.Create((e) => eventScreenshotReady.Invoke(e));

                    return eventScreenshotReady;
                }
            }
            /// <summary>
            /// A screenshot has been requested by the user from the Steam screenshot hotkey. This will only be called if HookScreenshots has been enabled, in which case Steam will not take the screenshot itself.
            /// </summary>
            public static UnityEvent EventScreenshotRequested
            {
                get
                {
                    if (m_ScreenshotRequested_t == null)
                        m_ScreenshotRequested_t = Callback<ScreenshotRequested_t>.Create((e) => eventScreenshotRequested.Invoke());

                    return eventScreenshotRequested;
                }
            }

            private static ScreenshotReadyEvent eventScreenshotReady = new ScreenshotReadyEvent();
            private static UnityEvent eventScreenshotRequested = new UnityEvent();

            private static Callback<ScreenshotRequested_t> m_ScreenshotRequested_t;
            private static Callback<ScreenshotReady_t> m_ScreenshotReady_t;

            /// <summary>
            /// Adds a screenshot to the user's Steam screenshot library from disk.
            /// </summary>
            /// <param name="imageFilename">The absolute file path to the JPG, PNG, or TGA screenshot.</param>
            /// <param name="thumbnailFileName">The absolute file path to an optional thumbnail image. This must be 200px wide, as described by k_ScreenshotThumbWidth and the same aspect ratio. Pass NULL if there is no thumbnail, one will be created automatically.</param>
            /// <param name="width">The width of the screenshot.</param>
            /// <param name="height">The height of the screenshot.</param>
            /// <returns></returns>
            public static ScreenshotHandle AddScreenshotToLibrary(string imageFilename, string thumbnailFileName, int width, int height) => SteamScreenshots.AddScreenshotToLibrary(imageFilename, thumbnailFileName, width, height);
            /// <summary>
            /// Adds a VR screenshot to the user's Steam screenshot library from disk in the supported type.
            /// </summary>
            /// <param name="type">The type of VR screenshot that this is.</param>
            /// <param name="imageFilename">The absolute file path to a 2D JPG, PNG, or TGA version of the screenshot for the library view.</param>
            /// <param name="vrFilename">The absolute file path to the VR screenshot, this should be the same type of screenshot specified in eType.</param>
            /// <returns></returns>
            public static ScreenshotHandle AddVRScreenshotToLibrary(EVRScreenshotType type, string imageFilename, string vrFilename) => SteamScreenshots.AddVRScreenshotToLibrary(type, imageFilename, vrFilename);
            /// <summary>
            /// Toggles whether the overlay handles screenshots when the user presses the screenshot hotkey, or if the game handles them.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Hooking is disabled by default, and only ever enabled if you do so with this function.
            /// </para>
            /// <para>
            /// If the hooking is enabled, then the ScreenshotRequested_t callback will be sent if the user presses the hotkey or when TriggerScreenshot is called, and then the game is expected to call WriteScreenshot or AddScreenshotToLibrary in response.
            /// </para>
            /// </remarks>
            /// <param name="hook"></param>
            public static void HookScreenshots(bool hook) => SteamScreenshots.HookScreenshots(hook);
            /// <summary>
            /// Sets optional metadata about a screenshot's location. For example, the name of the map it was taken on.
            /// </summary>
            /// <remarks>
            /// You can get the handle to tag the screenshot once it has been successfully saved from the ScreenshotReady_t callback or via the WriteScreenshot, AddScreenshotToLibrary, AddVRScreenshotToLibrary calls.
            /// </remarks>
            /// <param name="handle"></param>
            /// <param name="location"></param>
            /// <returns></returns>
            public static bool SetLocation(ScreenshotHandle handle, string location) => SteamScreenshots.SetLocation(handle, location);
            /// <summary>
            /// Tags a UGC published file as being visible in the screenshot.
            /// </summary>
            /// <remarks>
            /// You can tag up to the value declared by <see cref="Constants.k_nScreenshotMaxTaggedPublishedFiles"/> in a single screenshot. Tagging more items than that will just be discarded.
            /// </remarks>
            /// <param name="handle">The handle to the screenshot to tag.</param>
            /// <param name="ugcFileId">The workshop item ID that is in the screenshot.</param>
            /// <returns></returns>
            public static bool TagPublishedFile(ScreenshotHandle handle, PublishedFileId_t ugcFileId) => SteamScreenshots.TagPublishedFile(handle, ugcFileId);
            /// <summary>
            /// Tags a Steam user as being visible in the screenshot.
            /// </summary>
            /// <remarks>
            /// You can tag up to the value declared by <see cref="Constants.k_nScreenshotMaxTaggedUsers"/> in a single screenshot. Tagging more users than that will just be discarded.
            /// </remarks>
            /// <param name="handle">The handle to the screenshot to tag.</param>
            /// <param name="userId">The Steam ID of a user that is in the screenshot.</param>
            /// <returns></returns>
            public static bool TagUser(ScreenshotHandle handle, CSteamID userId) => SteamScreenshots.TagUser(handle, userId);
            /// <summary>
            /// Either causes the Steam Overlay to take a screenshot, or tells your screenshot manager that a screenshot needs to be taken. Depending on the value of IsScreenshotsHooked.
            /// </summary>
            public static void TriggerScreenshot() => SteamScreenshots.TriggerScreenshot();
            /// <summary>
            /// Writes a screenshot to the user's Steam screenshot library given the raw image data, which must be in RGB format.
            /// </summary>
            /// <param name="data"></param>
            /// <param name="width"></param>
            /// <param name="height"></param>
            /// <returns></returns>
            public static ScreenshotHandle WriteScreenshot(byte[] data, int width, int height) => SteamScreenshots.WriteScreenshot(data, (uint)data.Length, width, height);
        }
    }
}
#endif