#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Management;

[InitializeOnLoad]
public class VrFix
{
    static VrFix()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }
}
#endif