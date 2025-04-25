#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public class SearchArguments
    {
        /// <summary>
        /// If less than or equal to 0 then we wont use the open slot filter
        /// </summary>
        [UnityEngine.Tooltip("If less than or equal to 0 then we wont use the open slot filter")]
        public int slots = -1;
        /// <summary>
        /// The distance from teh searching user that should be considered when searching
        /// </summary>
        [UnityEngine.Tooltip("The distance from the searching user that should be considered when searching")]
        public ELobbyDistanceFilter distance = ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault;
        /// <summary>
        /// Metadata values that should be used to sort the results e.g. values `closer` to these values will be weighted higher in the resutls
        /// </summary>
        [UnityEngine.Tooltip("Metadata values that should be used to sort the results e.g. values `closer` to these values will be weighted higher in the resutls")]
        public List<NearFilter> nearValues = new();
        /// <summary>
        /// Metadata values that should be compared as numeric values e.g. should follow typical maths rules for concepts such as less than, greater than, etc.
        /// </summary>
        [UnityEngine.Tooltip("Metadata values that should be compared as numeric values e.g. should follow typical maths rules for concepts such as less than, greater than, etc.")]
        public List<NumericFilter> numericFilters = new();
        /// <summary>
        /// Metadata values that should be compared as strings
        /// </summary>
        [UnityEngine.Tooltip("Metadata values that should be compared as strings")]
        public List<StringFilter> stringFilters = new();
    }
}
#endif