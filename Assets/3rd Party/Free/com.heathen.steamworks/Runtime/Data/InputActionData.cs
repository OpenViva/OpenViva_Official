#if !DISABLESTEAMWORKS  && STEAMWORKSNET


namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents the current state of the data for a specific <see cref="InputAction"/>
    /// </summary>
    [System.Serializable]
    public struct InputActionData
    {
        /// <summary>
        /// The name of the action this data relates to
        /// </summary>
        public string name;
        /// <summary>
        /// The type of action this data relates to
        /// </summary>
        public InputActionType type;
        /// <summary>
        /// The controller this data is from
        /// </summary>
        public Steamworks.InputHandle_t controller;
        /// <summary>
        /// Whether or not this action is currently available to be bound in the active action set. If it is not available, OR does not belong to the active action set, this will be false.
        /// </summary>
        public bool active;
        /// <summary>
        /// The native <see cref="Steamworks.EInputSourceMode"/> related to this action
        /// </summary>
        public Steamworks.EInputSourceMode mode;
        /// <summary>
        /// The state of the action e.g. true if pressed, pulled, rocked or otherwise non-zero
        /// </summary>
        public bool state;
        /// <summary>
        /// The value of the x axis if any
        /// </summary>
        public float x;
        /// <summary>
        /// The value of the y axis if any
        /// </summary>
        public float y;
        /// <summary>
        /// Returns the formatted state of the action
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if(type == InputActionType.Analog)
            {
                if(active)
                {
                    return "Active: X[" + x + "] Y[" + y + "]";
                }
                else
                {
                    return "Inactive";
                }
            }
            else
            {
                if (active)
                {
                    return "Active: " + (state ? "Engaged" : "Idle");
                }
                else
                {
                    return "Inactive";
                }
            }
        }
    }
}
#endif