//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace JBooth.FoliageRendering
{
    public static class FoliageMenuOptions 
    {

        #region ForceSceneRefreshToggle
        public const string _MENU_SCENEREFRESH = "Window/FoliageRenderer/Force Scene View Refresh";
        public const string _PREFS_SCENEREFRESH = "ForceSceneRefresh";

        public static bool ForceSceneRefresh
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetBool(_PREFS_SCENEREFRESH, defaultValue: true);
#else
                return false;
#endif
            }
        }

#if UNITY_EDITOR
        [MenuItem(_MENU_SCENEREFRESH)]
#endif
        public static void ToggleForceSceneRefresh()
        {
            bool currentValue = ForceSceneRefresh;
#if UNITY_EDITOR
            EditorPrefs.SetBool(_PREFS_SCENEREFRESH, !currentValue);
#endif
        }

#if UNITY_EDITOR
        [MenuItem(_MENU_SCENEREFRESH, true)]
#endif
        private static bool ToggleForceSceneRefreshValidate()
        {
#if UNITY_EDITOR
            Menu.SetChecked(_MENU_SCENEREFRESH, ForceSceneRefresh);
#endif
            return true;
        }
        #endregion
    }
}