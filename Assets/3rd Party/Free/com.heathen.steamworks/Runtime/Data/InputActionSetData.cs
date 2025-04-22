#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam Input Action Set and is used by object references such as <see cref="InputActionSet"/>
    /// </summary>
    [Serializable]
    public struct InputActionSetData : IEquatable<InputActionSetHandle_t>, IComparable<InputActionSetHandle_t>, IEquatable<ulong>, IComparable<ulong>
    {
        [SerializeField]
        private InputActionSetHandle_t handle;

        /// <summary>
        /// The primitive <see cref="ulong"/> value of the set's handle
        /// </summary>
        public ulong Handle
        {
            get => handle.m_InputActionSetHandle;
        }
        /// <summary>
        /// Check if this set is active and can be bound
        /// </summary>
        /// <param name="controller">The controller to check for</param>
        /// <returns>True if the set is active</returns>
        public bool IsActive(InputControllerData controller) => IsActive(controller.handle);
        /// <summary>
        /// Check if this set is active and can be bound
        /// </summary>
        /// <param name="controller">The controller to check for</param>
        /// <returns>True if the set is active</returns>
        public bool IsActive(Steamworks.InputHandle_t controller)
        {
            if (handle.m_InputActionSetHandle != 0)
            {
                var layers = API.Input.Client.GetCurrentActionSet(controller);
                if (layers.m_InputActionSetHandle == handle.m_InputActionSetHandle)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        /// <summary>
        /// Make this set active for the indicated controller
        /// </summary>
        /// <param name="controller">The controller to activate the set for</param>
        public void Activate(InputControllerData controller) => Activate(controller.handle);
        /// <summary>
        /// Make this set active for the indicated controller
        /// </summary>
        /// <param name="controller">The controller to active the set for</param>
        public void Activate(Steamworks.InputHandle_t controller)
        {
            if (handle.m_InputActionSetHandle != 0)
            {
                API.Input.Client.ActivateActionSet(controller, handle);
            }
        }
        /// <summary>
        /// Gets an <see cref="InputActionSetData"/> for the specified action set if available
        /// </summary>
        /// <param name="setName">The name of the set defined in the action file configuration for the app</param>
        /// <returns>The resulting <see cref="InputActionSetData"/></returns>
        public static InputActionSetData Get(string setName)
        {
            return new InputActionSetData
            {
                handle = API.Input.Client.GetActionSetHandle(setName)
            };
        }
        /// <summary>
        /// Gets an <see cref="InputActionSetData"/> for the indicated action set handle
        /// </summary>
        /// <param name="handle">The handle of the action set to fetch</param>
        /// <returns>The resulting <see cref="InputActionSetData"/></returns>
        public static InputActionSetData Get(InputActionSetHandle_t handle)
        {
            return new InputActionSetData
            {
                handle = handle,
            };
        }
        /// <summary>
        /// Gets an <see cref="InputActionSetData"/> for the indicated action set handle
        /// </summary>
        /// <param name="handleValue">The value of the handle to get the action set for</param>
        /// <returns>The resulting <see cref="InputActionSetData"/></returns>
        public static InputActionSetData Get(ulong handleValue)
        {
            return new InputActionSetData
            {
                handle = new InputActionSetHandle_t(handleValue),
            };
        }

        public bool Equals(InputActionSetHandle_t other)
        {
            return handle.Equals(other);
        }

        public int CompareTo(InputActionSetHandle_t other)
        {
            return handle.CompareTo(other);
        }

        public bool Equals(ulong other)
        {
            return handle.m_InputActionSetHandle.Equals(other);
        }

        public int CompareTo(ulong other)
        {
            return handle.m_InputActionSetHandle.CompareTo(other);
        }

        public override bool Equals(object obj)
        {
            return handle.Equals(obj);
        }

        public override int GetHashCode()
        {
            return handle.GetHashCode();
        }

        public static bool operator ==(InputActionSetData l, InputActionSetHandle_t r) => l.handle == r;
        public static bool operator ==(InputActionSetData l, ulong r) => l.handle == new InputActionSetHandle_t(r);
        public static bool operator ==(InputActionSetHandle_t l, InputActionSetData r) => l == r.handle;
        public static bool operator ==(ulong l, InputActionSetData r) => new InputActionSetHandle_t(l) == r.handle;
        public static bool operator !=(InputActionSetData l, InputActionSetHandle_t r) => l.handle != r;
        public static bool operator !=(InputActionSetData l, ulong r) => l.handle != new InputActionSetHandle_t(r);
        public static bool operator !=(InputActionSetHandle_t l, InputActionSetData r) => l != r.handle;
        public static bool operator !=(ulong l, InputActionSetData r) => new InputActionSetHandle_t(l) != r.handle;

        public static implicit operator ulong(InputActionSetData c) => c.handle.m_InputActionSetHandle;
        public static implicit operator InputActionSetData(ulong id) => Get(id);
        public static implicit operator InputActionSetHandle_t(InputActionSetData c) => c.handle;
        public static implicit operator InputActionSetData(InputActionSetHandle_t id) => Get(id);
    }
}
#endif