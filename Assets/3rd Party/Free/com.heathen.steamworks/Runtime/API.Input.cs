#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Steam Input API is a flexible action-based API that supports all major controller types - Xbox, PlayStation, Nintendo Switch Pro, and Steam Controllers.
    /// </summary>
    /// <remarks>
    /// https://partner.steamgames.com/doc/api/isteaminput
    /// </remarks>
    public static class Input
    {
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void RuntimeInit()
            {
                m_inputActionSetHandles = new Dictionary<string, InputActionSetHandle_t>();
                m_inputAnalogActionHandles = new Dictionary<string, InputAnalogActionHandle_t>();
                m_inputDigitalActionHandles = new Dictionary<string, InputDigitalActionHandle_t>();

                foreach (var pair in glyphs)
                {
                    if (pair.Value != null)
                        GameObject.Destroy(pair.Value);
                }

                glyphs = new Dictionary<EInputActionOrigin, Texture2D>();
                actions = new List<(string name, InputActionType type)>();
                controllers = new Dictionary<InputHandle_t, InputControllerData>();
                controllerUpdates = new Dictionary<InputHandle_t, int>();

                EventInputDataChanged = new ControllerDataEvent();

                initialized = false;
            }

            public static ControllerDataEvent EventInputDataChanged = new ControllerDataEvent();

            public static bool Initialized => initialized;

            private static bool initialized = false;
            private static Dictionary<string, InputActionSetHandle_t> m_inputActionSetHandles = new Dictionary<string, InputActionSetHandle_t>();
            private static Dictionary<string, InputAnalogActionHandle_t> m_inputAnalogActionHandles = new Dictionary<string, InputAnalogActionHandle_t>();
            private static Dictionary<string, InputDigitalActionHandle_t> m_inputDigitalActionHandles = new Dictionary<string, InputDigitalActionHandle_t>();
            private static Dictionary<EInputActionOrigin, Texture2D> glyphs = new Dictionary<EInputActionOrigin, Texture2D>();
            private static List<(string name, InputActionType type)> actions = new List<(string name, InputActionType type)>();
            private static Dictionary<InputHandle_t, InputControllerData> controllers = new Dictionary<InputHandle_t, InputControllerData>();
            private static Dictionary<InputHandle_t, int> controllerUpdates = new Dictionary<InputHandle_t, int>();

            /// <summary>
            /// Poles for and returns the handles for all connected controllers
            /// </summary>
            public static InputHandle_t[] Controllers
            {
                get
                {
                    var controllers = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
                    var count = SteamInput.GetConnectedControllers(controllers);
                    Array.Resize(ref controllers, count);

                    return controllers;
                }
            }

            /// <summary>
            /// Record an input to be tracked
            /// </summary>
            /// <param name="name"></param>
            /// <param name="type"></param>
            public static void AddInput(string name, InputActionType type) => actions.Add((name, type));
            /// <summary>
            /// Remove an input from tracking
            /// </summary>
            /// <param name="name"></param>
            public static void RemoveInput(string name) => actions.RemoveAll(p => p.name == name);
            /// <summary>
            /// Gets the data for the action from the first controller in the collection
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public static InputActionData GetActionData(string name)
            {
                if(controllers.Count > 0)
                    return controllers.First().Value.GetActionData(name);
                else
                {
                    var controllers = Controllers;
                    if (controllers.Length > 0)
                    {
                        var controllerData = Update(controllers[0]);
                        return controllerData.GetActionData(name);
                    }
                    else
                        return default;
                }
            }
            public static InputActionData GetActionData(InputHandle_t controller, string name)
            {
                if (controllers.ContainsKey(controller))
                    return controllers[controller].GetActionData(name);
                else
                    return default;
            }

            public static InputControllerData Update(InputHandle_t controller)
            {
                if (!controllerUpdates.ContainsKey(controller))
                    controllerUpdates.Add(controller, -1);

                if (controllerUpdates[controller] != Time.frameCount)
                {
                    controllerUpdates[controller] = Time.frameCount;

                    InputControllerData conData = new InputControllerData
                    {
                        handle = controller,
                        inputs = new InputActionData[actions.Count],
                    };

                    if (!controllers.ContainsKey(controller))
                        controllers.Add(controller, new()
                        {
                            handle = controller,
                            inputs = new InputActionData[0]
                        });

                    var currentController = controllers[controller];
                    var updates = new List<InputActionUpdate>();

                    for (int i = 0; i < actions.Count; i++)
                    {
                        var (name, type) = actions[i];
                        if (type == InputActionType.Analog)
                        {
                            var handle = GetAnalogActionHandle(name);

                            if (handle.m_InputAnalogActionHandle != 0)
                            {
                                var currentInput = currentController.inputs.FirstOrDefault(p => p.name == name && p.type == type);
                                var rawData = GetAnalogActionData(controller, handle);

                                var update = new InputActionUpdate
                                {
                                    name = name,
                                    controller = controller,
                                    mode = rawData.eMode,
                                    type = type,
                                    wasActive = currentInput.active,
                                    wasState = currentInput.state,
                                    wasX = currentInput.x,
                                    wasY = currentInput.y,
                                    isActive = rawData.bActive != 0,
                                    isState = rawData.x != 0 || rawData.y != 0,
                                    isX = rawData.x,
                                    isY = rawData.y,
                                };

                                var change = currentInput.x != rawData.x
                                    || currentInput.y != rawData.y
                                    || currentInput.active != (rawData.bActive == 0 ? true : false)
                                    || currentInput.state != (rawData.x != 0 || rawData.y != 0);

                                conData.inputs[i] = update.Data;
                                if (change)
                                    updates.Add(update);
                            }
                        }
                        else
                        {
                            var handle = GetDigitalActionHandle(name);

                            if (handle.m_InputDigitalActionHandle != 0)
                            {
                                var rawData = GetDigitalActionData(controller, handle);
                                var currentInput = currentController.inputs.FirstOrDefault(p => p.name == name && p.type == type);

                                var update = new InputActionUpdate
                                {
                                    name = name,
                                    controller = controller,
                                    mode = Steamworks.EInputSourceMode.k_EInputSourceMode_None,
                                    type = currentInput.type,
                                    wasActive = currentInput.active,
                                    wasState = currentInput.state,
                                    wasX = currentInput.x,
                                    wasY = currentInput.y,
                                    isActive = rawData.bActive != 0,
                                    isState = rawData.bState != 0,
                                    isX = rawData.bState,
                                    isY = rawData.bState,
                                };

                                var change = rawData.bState != 0 != currentInput.state;

                                conData.inputs[i] = update.Data;
                                if (change)
                                    updates.Add(update);
                            }
                        }
                    }

                    conData.changes = updates.ToArray();

                    controllers[controller] = conData;

                    if (conData.changes != null
                        && conData.changes.Length > 0)
                        EventInputDataChanged?.Invoke(conData);

                    return conData;
                }
                else
                    return controllers[controller];
            }
                        
            /// <summary>
            /// Reconfigure the controller to use the specified action set (ie "Menu", "Walk", or "Drive").
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to activate an action set for.</param>
            /// <param name="actionSetHandle">The handle of the action set you want to activate.</param>
            public static void ActivateActionSet(InputHandle_t controllerHandle, InputActionSetHandle_t actionSetHandle) => SteamInput.ActivateActionSet(controllerHandle, actionSetHandle);
            public static void ActivateActionSet(InputActionSetHandle_t actionSetHandle)
            {
                if (controllers.Count > 0)
                    ActivateActionSet(controllers.First().Key, actionSetHandle);
                else
                {
                    var controllers = Controllers;
                    if (controllers.Length > 0)
                    {
                        var controllerData = Update(controllers[0]);
                        ActivateActionSet(controllerData.handle, actionSetHandle);
                    }
                }
            }
            /// <summary>
            /// Reconfigure the controller to use the specified action set (ie "Menu", "Walk", or "Drive").
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to activate an action set for.</param>
            /// <param name="actionSet">The name of the set to use ... we will read this from cache if available or fetch it if required</param>
            public static void ActivateActionSet(InputHandle_t controllerHandle, string actionSet)
            {
                if (m_inputActionSetHandles.ContainsKey(actionSet))
                    SteamInput.ActivateActionSet(controllerHandle, m_inputActionSetHandles[actionSet]);
                else
                {
                    var handle = GetActionSetHandle(actionSet);
                    SteamInput.ActivateActionSet(controllerHandle, m_inputActionSetHandles[actionSet]);
                }
            }
            /// <summary>
            /// Reconfigure the controller to use the specified action set layer.
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to activate an action set layer for.</param>
            /// <param name="actionSetHandle">The handle of the action set layer you want to activate.</param>
            public static void ActivateActionSetLayer(InputHandle_t controllerHandle, InputActionSetHandle_t actionSetHandle) => SteamInput.ActivateActionSetLayer(controllerHandle, actionSetHandle);
            public static void ActivateActionSetLayer(InputActionSetHandle_t actionSetHandle)
            {
                if (controllers.Count > 0)
                    ActivateActionSetLayer(controllers.First().Key, actionSetHandle);
                else
                {
                    var controllers = Controllers;
                    if (controllers.Length > 0)
                    {
                        var controllerData = Update(controllers[0]);
                        ActivateActionSetLayer(controllerData.handle, actionSetHandle);
                    }
                }
            }
            /// <summary>
            /// Reconfigure the controller to use the specified action set (ie "Menu", "Walk", or "Drive").
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to activate an action set for.</param>
            /// <param name="actionSet">The name of the set to use ... we will read this from cache if available or fetch it if required</param>
            public static void ActivateActionSetLayer(InputHandle_t controllerHandle, string actionSet)
            {
                if (m_inputActionSetHandles.ContainsKey(actionSet))
                    SteamInput.ActivateActionSetLayer(controllerHandle, m_inputActionSetHandles[actionSet]);
                else
                {
                    var handle = GetActionSetHandle(actionSet);
                    SteamInput.ActivateActionSetLayer(controllerHandle, m_inputActionSetHandles[actionSet]);
                }
            }
            /// <summary>
            /// Reconfigure the controller to stop using the specified action set layer.
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to deactivate an action set layer for.</param>
            /// <param name="actionSetHandle">The handle of the action set layer you want to deactivate.</param>
            public static void DeactivateActionSetLayer(InputHandle_t controllerHandle, InputActionSetHandle_t actionSetHandle) => SteamInput.DeactivateActionSetLayer(controllerHandle, actionSetHandle);
            /// <summary>
            /// Reconfigure the controller to stop using the specified action set layer.
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to deactivate an action set layer for.</param>
            /// <param name="actionSet">The action set layer you want to deactivate.</param>
            public static void DeactivateActionSetLayer(InputHandle_t controllerHandle, string actionSet)
            {
                if (m_inputActionSetHandles.ContainsKey(actionSet))
                    SteamInput.DeactivateActionSetLayer(controllerHandle, m_inputActionSetHandles[actionSet]);
                else
                {
                    var handle = GetActionSetHandle(actionSet);
                    SteamInput.DeactivateActionSetLayer(controllerHandle, m_inputActionSetHandles[actionSet]);
                }
            }
            /// <summary>
            /// Reconfigure the controller to stop using all action set layers.
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to deactivate all action set layers for.</param>
            public static void DeactivateAllActionSetLayers(InputHandle_t controllerHandle) => SteamInput.DeactivateAllActionSetLayers(controllerHandle);
            /// <summary>
            /// Get the currently active action set layers for a specified controller handle.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <returns></returns>
            public static InputActionSetHandle_t[] GetActiveActionSetLayers(InputHandle_t controllerHandle)
            {
                var actionSetHandles = new InputActionSetHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
                var size = SteamInput.GetActiveActionSetLayers(controllerHandle, actionSetHandles);
                Array.Resize(ref actionSetHandles, size);
                return actionSetHandles;
            }
            /// <summary>
            /// Lookup the handle for an Action Set.
            /// </summary>
            /// <param name="setName">The name of the set to fetch</param>
            /// <returns></returns>
            public static InputActionSetHandle_t GetActionSetHandle(string setName)
            {
                var result = SteamInput.GetActionSetHandle(setName);
                if (m_inputActionSetHandles.ContainsKey(setName))
                    m_inputActionSetHandles[setName] = result;
                else
                    m_inputActionSetHandles.Add(setName, result);

                return result;
            }
            /// <summary>
            /// Returns the current state of the supplied analog game action.
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to query.</param>
            /// <param name="analogActionHandle">The handle of the analog action you want to query.</param>
            /// <returns></returns>
            public static InputAnalogActionData_t GetAnalogActionData(InputHandle_t controllerHandle, InputAnalogActionHandle_t analogActionHandle) => SteamInput.GetAnalogActionData(controllerHandle, analogActionHandle);
            /// <summary>
            /// Returns the current state of the supplied analog game action.
            /// </summary>
            /// <param name="controllerHandle">The handle of the controller you want to query.</param>
            /// <param name="actionName">The analog action you want to query.</param>
            /// <returns></returns>
            public static InputAnalogActionData_t GetAnalogActionData(InputHandle_t controllerHandle, string actionName)
            {
                if (m_inputAnalogActionHandles.ContainsKey(actionName))
                    return SteamInput.GetAnalogActionData(controllerHandle, m_inputAnalogActionHandles[actionName]);
                else
                {
                    var handle = GetAnalogActionHandle(actionName);
                    return SteamInput.GetAnalogActionData(controllerHandle, handle);
                }
            }
            /// <summary>
            /// Get the handle of the specified Analog action.
            /// </summary>
            /// <remarks>
            /// This function does not take an action set handle parameter. That means that each action in your VDF file must have a unique string identifier. In other words, if you use an action called "up" in two different action sets, this function will only ever return one of them and the other will be ignored.
            /// </remarks>
            /// <param name="actionName">The string identifier of the analog action defined in the game's VDF file.</param>
            /// <returns></returns>
            public static InputAnalogActionHandle_t GetAnalogActionHandle(string actionName)
            {
                var result = SteamInput.GetAnalogActionHandle(actionName);
                if (m_inputAnalogActionHandles.ContainsKey(actionName))
                    m_inputAnalogActionHandles[actionName] = result;
                else
                    m_inputAnalogActionHandles.Add(actionName, result);

                return result;
            }
            /// <summary>
            /// Get the origin(s) for an analog action within an action set by filling originsOut with EInputActionOrigin handles. Use this to display the appropriate on-screen prompt for the action.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="actionSetHandle"></param>
            /// <param name="analogActionHandle"></param>
            /// <returns></returns>
            public static EInputActionOrigin[] GetAnalogActionOrigins(InputHandle_t controllerHandle, InputActionSetHandle_t actionSetHandle, InputAnalogActionHandle_t analogActionHandle)
            {
                var origins = new EInputActionOrigin[Constants.STEAM_INPUT_MAX_ORIGINS];

                SteamInput.GetAnalogActionOrigins(controllerHandle, actionSetHandle, analogActionHandle, origins);

                return origins;
            }
            /// <summary>
            /// Get the origin(s) for an analog action within an action set by filling originsOut with EInputActionOrigin handles. Use this to display the appropriate on-screen prompt for the action.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="actionSet"></param>
            /// <param name="analogName"></param>
            /// <returns></returns>
            public static EInputActionOrigin[] GetAnalogActionOrigins(InputHandle_t controllerHandle, string actionSet, string analogName)
            {
                var origins = new EInputActionOrigin[Constants.STEAM_INPUT_MAX_ORIGINS];

                if (!m_inputAnalogActionHandles.ContainsKey(analogName))
                    GetAnalogActionHandle(analogName);

                if (!m_inputActionSetHandles.ContainsKey(actionSet))
                    GetActionSetHandle(actionSet);

                SteamInput.GetAnalogActionOrigins(controllerHandle, m_inputActionSetHandles[actionSet], m_inputAnalogActionHandles[analogName], origins);

                return origins;
            }
            /// <summary>
            /// Returns the associated controller handle for the specified emulated gamepad. Can be used with GetInputTypeForHandle to determine the controller type of a controller using Steam Input Gamepad Emulation.
            /// </summary>
            /// <param name="index">The index of the emulated gamepad you want to get a controller handle for.</param>
            /// <returns></returns>
            public static InputHandle_t GetControllerForGamepadIndex(int index) => SteamInput.GetControllerForGamepadIndex(index);
            /// <summary>
            /// Get the currently active action set for the specified controller.
            /// </summary>
            /// <param name="controllerHandle">	The handle of the controller you want to query.</param>
            /// <returns></returns>
            public static InputActionSetHandle_t GetCurrentActionSet(InputHandle_t controllerHandle) => SteamInput.GetCurrentActionSet(controllerHandle);
            /// <summary>
            /// Returns the current state of the supplied digital game action.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="actionHandle"></param>
            /// <returns></returns>
            public static InputDigitalActionData_t GetDigitalActionData(InputHandle_t controllerHandle, InputDigitalActionHandle_t actionHandle) => SteamInput.GetDigitalActionData(controllerHandle, actionHandle);
            /// <summary>
            /// Returns the current state of the supplied digital game action.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="actionName"></param>
            /// <returns></returns>
            public static InputDigitalActionData_t GetDigitalActionData(InputHandle_t controllerHandle, string actionName)
            {
                if (!m_inputDigitalActionHandles.ContainsKey(actionName))
                {
                    var actionHandle = GetDigitalActionHandle(actionName);
                    return SteamInput.GetDigitalActionData(controllerHandle, actionHandle);
                }
                else
                {
                    return SteamInput.GetDigitalActionData(controllerHandle, m_inputDigitalActionHandles[actionName]);
                }
            }
            /// <summary>
            /// Get the handle of the specified digital action.
            /// </summary>
            /// <remarks>
            /// NOTE: This function does not take an action set handle parameter. That means that each action in your VDF file must have a unique string identifier. In other words, if you use an action called "up" in two different action sets, this function will only ever return one of them and the other will be ignored.
            /// </remarks>
            /// <param name="actionName"></param>
            /// <returns></returns>
            public static InputDigitalActionHandle_t GetDigitalActionHandle(string actionName)
            {
                var result = SteamInput.GetDigitalActionHandle(actionName);
                if (m_inputDigitalActionHandles.ContainsKey(actionName))
                    m_inputDigitalActionHandles[actionName] = result;
                else
                    m_inputDigitalActionHandles.Add(actionName, result);

                return result;
            }
            /// <summary>
            /// Get the origin(s) for an digital action within an action set by filling originsOut with EInputActionOrigin handles. Use this to display the appropriate on-screen prompt for the action.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="actionSetHandle"></param>
            /// <param name="analogActionHandle"></param>
            /// <returns></returns>
            public static EInputActionOrigin[] GetDigitalActionOrigins(InputHandle_t controllerHandle, InputActionSetHandle_t actionSetHandle, InputDigitalActionHandle_t digitalActionHandle)
            {
                var origins = new EInputActionOrigin[Constants.STEAM_INPUT_MAX_ORIGINS];

                SteamInput.GetDigitalActionOrigins(controllerHandle, actionSetHandle, digitalActionHandle, origins);

                return origins;
            }
            /// <summary>
            /// Get the origin(s) for an analog action within an action set by filling originsOut with EInputActionOrigin handles. Use this to display the appropriate on-screen prompt for the action.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="actionSet"></param>
            /// <param name="actionName"></param>
            /// <returns></returns>
            public static EInputActionOrigin[] GetDigitalActionOrigins(InputHandle_t controllerHandle, string actionSet, string actionName)
            {
                var origins = new EInputActionOrigin[Constants.STEAM_INPUT_MAX_ORIGINS];

                if (!m_inputDigitalActionHandles.ContainsKey(actionName))
                    GetDigitalActionHandle(actionName);

                if (!m_inputDigitalActionHandles.ContainsKey(actionSet))
                    GetActionSetHandle(actionSet);

                SteamInput.GetDigitalActionOrigins(controllerHandle, m_inputActionSetHandles[actionSet], m_inputDigitalActionHandles[actionName], origins);

                return origins;
            }
            /// <summary>
            /// Returns the associated gamepad index for the specified controller, if emulating a gamepad.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <returns></returns>
            public static int GetGamepadIndexForController(InputHandle_t controllerHandle) => SteamInput.GetGamepadIndexForController(controllerHandle);
            /// <summary>
            /// Get and cache glyph images
            /// </summary>
            /// <param name="origin"></param>
            /// <returns></returns>
            public static Texture2D GetGlyphActionOrigin(EInputActionOrigin origin)
            {
                if (glyphs.ContainsKey(origin))
                    return glyphs[origin];
                else
                {
                    var path = GetGlyphPNGForActionOrigin(origin, ESteamInputGlyphSize.k_ESteamInputGlyphSize_Large, 0);
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (System.IO.File.Exists(path))
                        {
                            var fileData = System.IO.File.ReadAllBytes(path);
                            var tex = new Texture2D(2, 2);
                            tex.LoadImage(fileData);

                            glyphs.Add(origin, tex);
                            return tex;
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
            }
            public static void UnloadGlyphImages()
            {
                foreach (var pair in glyphs)
                {
                    if (pair.Value != null)
                        GameObject.Destroy(pair.Value);
                }

                glyphs = new Dictionary<EInputActionOrigin, Texture2D>();
            }
            public static string GetGlyphPNGForActionOrigin(EInputActionOrigin origin, ESteamInputGlyphSize size, uint flags) => SteamInput.GetGlyphPNGForActionOrigin(origin, size, flags);
            public static string GetGlyphSVGForActionOrigin(EInputActionOrigin origin, uint flags) => SteamInput.GetGlyphSVGForActionOrigin(origin, flags);
            /// <summary>
            /// Returns the input type (device model) for the specified controller. This tells you if a given controller is a Steam controller, XBox 360 controller, PS4 controller, etc.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <returns></returns>
            public static ESteamInputType GetInputTypeForHandle(InputHandle_t controllerHandle) => SteamInput.GetInputTypeForHandle(controllerHandle);
            /// <summary>
            /// Returns raw motion data for the specified controller.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <returns></returns>
            public static InputMotionData_t GetMotionData(InputHandle_t controllerHandle) => SteamInput.GetMotionData(controllerHandle);
            /// <summary>
            /// Returns a localized string (from Steam's language setting) for the specified origin.
            /// </summary>
            /// <param name="origin"></param>
            /// <returns></returns>
            public static string GetStringForActionOrigin(EInputActionOrigin origin) => SteamInput.GetStringForActionOrigin(origin);
            /// <summary>
            /// Must be called when starting use of the Input interface.
            /// </summary>
            public static bool Init(IEnumerable<(string name, InputActionType type)> actions = null)
            {
                initialized = SteamInput.Init(false);
                foreach(var action in actions)
                {
                    Client.actions.Add(action);
                }
                return initialized;
            }
            /// <summary>
            /// Synchronize API state with the latest Steam Controller inputs available. This is performed automatically by SteamAPI_RunCallbacks, but for the absolute lowest possible latency, you can call this directly before reading controller state.
            /// </summary>
            public static void RunFrame() => SteamInput.RunFrame();
            /// <summary>
            /// Sets the color of the controllers LED
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="color"></param>
            public static void SetLEDColor(InputHandle_t controllerHandle, Color32 color) => SteamInput.SetLEDColor(controllerHandle, color.r, color.g, color.b, 0);
            /// <summary>
            /// Resets the color fo the controllers LED to the users default
            /// </summary>
            /// <param name="controllerHandle"></param>
            public static void ResetLEDColor(InputHandle_t controllerHandle) => SteamInput.SetLEDColor(controllerHandle, 0, 0, 0, 1);
            /// <summary>
            /// Must be called when ending use of the Input interface.
            /// </summary>
            public static bool Shutdown()
            {
                initialized = false;
                return SteamInput.Shutdown();
            }
            /// <summary>
            /// Invokes the Steam overlay and brings up the binding screen.
            /// </summary>
            public static void ShowBindingPanel(InputHandle_t controllerHandle) => SteamInput.ShowBindingPanel(controllerHandle);
            /// <summary>
            /// Stops the momentum of an analog action (where applicable, ie a touchpad w/ virtual trackball settings).
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="analogAction"></param>
            public static void StopAnalogActionMomentum(InputHandle_t controllerHandle, InputAnalogActionHandle_t analogAction) => SteamInput.StopAnalogActionMomentum(controllerHandle, analogAction);
            /// <summary>
            /// Stops the momentum of an analog action (where applicable, ie a touchpad w/ virtual trackball settings).
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="actionName"></param>
            public static void StopAnalogActionMomentum(InputHandle_t controllerHandle, string actionName)
            {
                if (m_inputAnalogActionHandles.ContainsKey(actionName))
                    SteamInput.StopAnalogActionMomentum(controllerHandle, m_inputAnalogActionHandles[actionName]);
                else
                {
                    var action = GetAnalogActionHandle(actionName);
                    SteamInput.StopAnalogActionMomentum(controllerHandle, action);
                }
            }
            /// <summary>
            /// Trigger a vibration event on supported controllers.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="leftSpeed"></param>
            /// <param name="rightSpeed"></param>
            public static void TriggerVibration(InputHandle_t controllerHandle, ushort leftSpeed, ushort rightSpeed) => SteamInput.TriggerVibration(controllerHandle, leftSpeed, rightSpeed);
            /// <summary>
            /// Get an action origin that you can use in your glyph look up table or passed into GetGlyphForActionOrigin or GetStringForActionOrigin
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="origin"></param>
            public static void GetActionOriginFromXboxOrigin(InputHandle_t controllerHandle, EXboxOrigin origin) => SteamInput.GetActionOriginFromXboxOrigin(controllerHandle, origin);
            /// <summary>
            /// Get the equivalent origin for a given controller type or the closest controller type that existed in the SDK you built into your game if eDestinationInputType is k_ESteamInputType_Unknown. This action origin can be used in your glyph look up table or passed into GetGlyphForActionOrigin or GetStringForActionOrigin
            /// </summary>
            /// <param name="destination"></param>
            /// <param name="source"></param>
            public static void TranslateActionOrigin(ESteamInputType destination, EInputActionOrigin source) => SteamInput.TranslateActionOrigin(destination, source);
            /// <summary>
            /// Gets the major and minor device binding revisions for Steam Input API configurations. Major revisions are to be used when changing the number of action sets or otherwise reworking configurations to the degree that older configurations are no longer usable. When a user's binding disagrees with the major revision of the current official configuration Steam will forcibly update the user to the new configuration. New configurations will need to be made for every controller when updating the major revision. Minor revisions are for small changes such as adding a new optional action or updating localization in the configuration. When updating the minor revision you generally can update a single configuration and check the "Use Action Block" to apply the action block changes to the other configurations.
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <param name="major"></param>
            /// <param name="minor"></param>
            /// <returns></returns>
            public static bool GetDeviceBindingRevision(InputHandle_t controllerHandle, out int major, out int minor) => SteamInput.GetDeviceBindingRevision(controllerHandle, out major, out minor);
            /// <summary>
            /// Get the Steam Remote Play session ID associated with a device, or 0 if there is no session associated with it. See iSteamRemotePlay.h for more information on Steam Remote Play sessions
            /// </summary>
            /// <param name="controllerHandle"></param>
            /// <returns></returns>
            public static uint GetRemotePlaySessionID(InputHandle_t controllerHandle) => SteamInput.GetRemotePlaySessionID(controllerHandle);
        }
    }
}
#endif