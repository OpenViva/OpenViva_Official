#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    public class InputActionSetLayer : ScriptableObject
    {
        public string layerName;

        public InputActionSetData Data { get; private set; }

        public bool IsActive(Steamworks.InputHandle_t controller)
        {
            if (Data == 0)
                Data = InputActionSetData.Get(layerName);

            if (Data != 0)
            {
                var layers = API.Input.Client.GetActiveActionSetLayers(controller);
                if (layers.Any(p => p.m_InputActionSetHandle == Data))
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
                Data = InputActionSetData.Get(layerName);

            if (Data != 0)
            {
                API.Input.Client.ActivateActionSetLayer(controller, Data);
            }
        }

        public void Activate()
        {
            if (Data == 0)
                Data = InputActionSetData.Get(layerName);

            if (Data != 0)
            {
                API.Input.Client.ActivateActionSetLayer(Data);
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(InputActionSetLayer))]
    public class InputActionSetLayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        { }
    }
#endif
}
#endif