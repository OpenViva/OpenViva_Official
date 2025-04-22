#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    public class InputAction : ScriptableObject
    {
        public InputActionType Type
        {
            get => type;
#if UNITY_EDITOR
            set => type = value;
#endif
        }
        public string ActionName
        {
            get => actionName;
#if UNITY_EDITOR
            set => actionName = value;
#endif
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [SerializeField]
        private InputActionType type;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [SerializeField]
        private string actionName;

        public Steamworks.InputAnalogActionHandle_t AnalogHandle => API.Input.Client.GetAnalogActionHandle(actionName);
        public Steamworks.InputDigitalActionHandle_t DigitalHandle => API.Input.Client.GetDigitalActionHandle(actionName);
        
        public InputActionData GetActionData(Steamworks.InputHandle_t controller) => API.Input.Client.GetActionData(controller, actionName);
        public InputActionData GetActionData() => API.Input.Client.GetActionData(actionName);
        public Texture2D[] GetInputGlyphs(Steamworks.InputHandle_t controller, InputActionSet set) => GetInputGlyphs(controller, set.Data);
        public Texture2D[] GetInputGlyphs(Steamworks.InputHandle_t controller, InputActionSetLayer set) => GetInputGlyphs(controller, set.Data);
        public Texture2D[] GetInputGlyphs(Steamworks.InputHandle_t controller, Steamworks.InputActionSetHandle_t set)
        {
            if (type == InputActionType.Analog)
            {
                var origins = API.Input.Client.GetAnalogActionOrigins(controller, set, AnalogHandle);

                var textArray = new Texture2D[origins.Length];
                for (int i = 0; i < origins.Length; i++)
                {
                    textArray[i] = API.Input.Client.GetGlyphActionOrigin(origins[i]);
                }

                return textArray;
            }
            else
            {
                var origins = API.Input.Client.GetDigitalActionOrigins(controller, set, DigitalHandle);

                var textArray = new Texture2D[origins.Length];
                for (int i = 0; i < origins.Length; i++)
                {
                    textArray[i] = API.Input.Client.GetGlyphActionOrigin(origins[i]);
                }

                return textArray;
            }
        }

        public string[] GetInputNames(Steamworks.InputHandle_t controller, InputActionSet set) => GetInputNames(controller, set.Data);
        public string[] GetInputNames(Steamworks.InputHandle_t controller, InputActionSetLayer set) => GetInputNames(controller, set.Data);
        public string[] GetInputNames(Steamworks.InputHandle_t controller, Steamworks.InputActionSetHandle_t set)
        {
            if (type == InputActionType.Analog)
            {
                var origins = API.Input.Client.GetAnalogActionOrigins(controller, set, AnalogHandle);

                var stringArray = new string[origins.Length];
                for (int i = 0; i < origins.Length; i++)
                {
                    stringArray[i] = API.Input.Client.GetStringForActionOrigin(origins[i]);
                }

                return stringArray;
            }
            else
            {
                var origins = API.Input.Client.GetDigitalActionOrigins(controller, set, DigitalHandle);

                var stringArray = new string[origins.Length];
                for (int i = 0; i < origins.Length; i++)
                {
                    stringArray[i] = API.Input.Client.GetStringForActionOrigin(origins[i]);
                }

                return stringArray;
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(InputAction))]
    public class InputActionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        { }
    }
#endif
}
#endif