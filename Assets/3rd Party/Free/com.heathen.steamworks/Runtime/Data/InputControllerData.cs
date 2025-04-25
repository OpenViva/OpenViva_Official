#if !DISABLESTEAMWORKS  && STEAMWORKSNET

using System.Linq;
using Unity.Mathematics;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a controller's current data state
    /// </summary>
    [System.Serializable]
    public struct InputControllerData
    {
        /// <summary>
        /// The handle of the controller this data represents
        /// </summary>
        public Steamworks.InputHandle_t handle;
        /// <summary>
        /// A collection of all the inputs related to this controller and their current state
        /// </summary>
        public InputActionData[] inputs;
        /// <summary>
        /// A collection of all the changes to input state since the last update of this controller
        /// </summary>
        public InputActionUpdate[] changes;
        /// <summary>
        /// Gets the action data of a specific input action
        /// </summary>
        /// <param name="action">The action whose state you want to read</param>
        /// <returns>The <see cref="InputActionData"/> representing the current state of the action</returns>
        public InputActionData GetActionData(InputAction action) => GetActionData(action.ActionName);
        /// <summary>
        /// Gets the action data of a specific input action
        /// </summary>
        /// <param name="name">The name of the action whose state you want to read</param>
        /// <returns>The <see cref="InputActionData"/> representing the current state of the action</returns>
        public InputActionData GetActionData(string name) => inputs.FirstOrDefault(p => p.name == name);
        /// <summary>
        /// Check if the indicated action is active
        /// </summary>
        /// <param name="name">The name of the input to check</param>
        /// <returns>True if this action is available to be bound</returns>
        public bool GetActive(string name) => inputs.FirstOrDefault(p => p.name == name).active;
        /// <summary>
        /// Check for the state of the indicated action e.g. <see cref="true"/> or <see cref="false"/>
        /// </summary>
        /// <param name="name">The name of the input to check</param>
        /// <returns>True if this action is <see cref="true"/></returns>
        public bool GetState(string name) => inputs.FirstOrDefault(p => p.name == name).state;
        /// <summary>
        /// Gets the float value of this action
        /// </summary>
        /// <param name="name">The name of the input to check</param>
        /// <returns>The float value associated with the first float of the action e.g. x if known</returns>
        public float GetFloat(string name) => inputs.FirstOrDefault(p => p.name == name).x;
        /// <summary>
        /// Gets the <see cref="float2"/> value of this action
        /// </summary>
        /// <param name="name">The name of the input to check</param>
        /// <returns>The <see cref="float2"/> value associated the with action if known</returns>
        public float2 GetFloat2(string name)
        {
            var data = inputs.FirstOrDefault(p => p.name == name);
            return new float2(data.x, data.y);
        }
        /// <summary>
        /// Get the <see cref="Steamworks.EInputSourceMode"/> of the indicated action
        /// </summary>
        /// <param name="name">The input to check</param>
        /// <returns>The <see cref="Steamworks.EInputSourceMode"/> associated with the action if known</returns>
        public Steamworks.EInputSourceMode GetMode(string name) => inputs.FirstOrDefault(p => p.name == name).mode;
    }
}
#endif