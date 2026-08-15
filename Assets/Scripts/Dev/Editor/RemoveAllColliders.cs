using UnityEditor;
using UnityEngine;

public class RemoveAllColliders : Editor
{
    [MenuItem("Tools/Remove All Box Colliders")]
    private static void RemoveBoxColliders()
    {
        GameObject selectedTarget = Selection.activeGameObject;

        if (selectedTarget == null)
        {
            Debug.LogWarning("Select GameObject first!");
            return;
        }

        Undo.RegisterCompleteObjectUndo(selectedTarget, "Remove Box Colldiers");

        BoxCollider[] colliders = selectedTarget.GetComponentsInChildren<BoxCollider>();

        if (colliders.Length == 0)
        {
            return;
        }

        int count = 0;
        foreach (var collider in colliders)
        {
            DestroyImmediate(collider);
            count++;
        }

        Debug.Log($"Destroyed {count} Box Colldiers");
    }
}
