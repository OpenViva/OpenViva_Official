#if !DISABLESTEAMWORKS  && STEAMWORKSNET

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// A simple interface that standardizes the concept of a Workshop Browser Item making it easier to create UI elements for the presentation and collection of Workshop items originating from UGC Query and similar.
    /// </summary>
    public interface IWorkshopBrowserItemTemplate
    {
        /// <summary>
        /// This should be used to apply a specific Workshop Item to this entry
        /// </summary>
        /// <param name="item">The item to be applied</param>
        void Load(WorkshopItem item);
    }
}
#endif