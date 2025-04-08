#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A custom serializable <see cref="UnityEvent{T0}"/> which handles <see cref="PersonaStateChange_t"/> data.
    /// </summary>
    [Serializable]
    public class PersonaStateChangeEvent : UnityEvent<PersonaStateChange>
    { }
}
#endif
