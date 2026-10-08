using UnityEditor;
using UnityEngine;

public class MarkerControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MarkerController controller = (MarkerController)target;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Apply Text & Font Size Now", GUILayout.Height(30)))
        {
            controller.Apply();
            EditorUtility.SetDirty(controller);

            if (controller.targetTMP != null)
                EditorUtility.SetDirty(controller.targetTMP);
        }

        if (controller.targetTMP != null)
        {
            EditorGUILayout.HelpBox(
                $"Currently controlling: {controller.targetTMP.name}",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "No TextMeshPro found in children yet.",
                MessageType.Warning);
        }
    }
}
