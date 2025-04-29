using UnityEditor;
using UnityEngine;
using System.Collections;
public class AutoSaveProject : EditorWindow
{
    private static bool autoSaveEnabled = true; // To toggle auto-saving on and off
    private static float saveInterval = 30.0f; // Interval in seconds
    private static double nextSaveTime = 0;

    // Add menu named "AutoSave Project" to the Window menu
    [MenuItem("Window/AutoSave Project")]
    public static void ShowWindow()
    {
        // Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(AutoSaveProject));
    }

    void OnGUI()
    {
        GUILayout.Label("AutoSave Project Settings", EditorStyles.boldLabel);
        autoSaveEnabled = EditorGUILayout.Toggle("Enable Auto Save", autoSaveEnabled);
        saveInterval = EditorGUILayout.FloatField("Save Interval (Seconds)", saveInterval);
    }

    void Update()
    {
        if (autoSaveEnabled && EditorApplication.timeSinceStartup > nextSaveTime && !EditorApplication.isPlaying)
        {
            Debug.Log("Auto-saving all open scenes... " + System.DateTime.Now);
            EditorApplication.SaveScene();
            AssetDatabase.SaveAssets();
            nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;
        }
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
}