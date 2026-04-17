using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class RemoveMissingScripts : MonoBehaviour
{
    [MenuItem("GameObject/Remove Missing Scripts (Recursive)", false, 10)]
    private static void RemoveFromSelected()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("No GameObject selected!");
            return;
        }

        int totalRemoved = 0;

        foreach (GameObject go in Selection.gameObjects)
        {
            // Include the object itself and all children (active or inactive)
            Transform[] transforms = go.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                GameObject current = t.gameObject;
                int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(current);
                if (count > 0)
                {
                    Undo.RegisterCompleteObjectUndo(current, "Remove Missing Scripts");
                    EditorUtility.SetDirty(current);
                    totalRemoved += count;
                    Debug.Log($"Removed {count} missing script(s) from {current.name}", current);
                }
            }
        }

        if (totalRemoved > 0)
        {
            Debug.Log($"Total missing scripts removed: {totalRemoved}");

            // Mark the scene as dirty so you can save
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        else
        {
            Debug.Log("No missing scripts found on the selected object or its children.");
        }
    }
}
