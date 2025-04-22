#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    public class InputActionSet : ScriptableObject
    {
        public string setName;
        public InputActionSetData Data { get; private set; }

        public bool IsActive(Steamworks.InputHandle_t controller)
        {
            if (Data == 0)
                Data = InputActionSetData.Get(setName);

            if (Data != 0)
            {
                var layers = API.Input.Client.GetCurrentActionSet(controller);
                if (layers.m_InputActionSetHandle == Data)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public void Activate(Steamworks.InputHandle_t controller)
        {
            if (Data == 0)
                Data = InputActionSetData.Get(setName);

            if (Data != 0)
            {
                API.Input.Client.ActivateActionSet(controller, Data);
            }
        }

        public void Activate()
        {
            if (Data == 0)
                Data = InputActionSetData.Get(setName);

            if (Data != 0)
            {
                API.Input.Client.ActivateActionSet(Data);
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(InputActionSet))]
    public class InputActionSetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        { }
    }

#endif
}
#endif