using UnityEngine;

public class VivaUtilities : MonoBehaviour
{
    public static GameObject FindGameObjectByPath(GameObject root, string path)
    {
        if (root == null)
        {
            Debug.LogError("[Chara Loader] Root GameObject is null!");
            return null;
        }

        if (string.IsNullOrEmpty(path))
        {
            return root;
        }

        string[] pathParts = path.Split('/');

        int startIndex = 0;
        if (pathParts.Length > 1 || pathParts[0] == root.name) startIndex = 1;

        // If path only contains the root name then give that instead
        if (pathParts.Length == 1)
        {
            return root;
        }

        Transform current = root.transform;

        for (int i = startIndex; i < pathParts.Length; i++)
        {
            string part = pathParts[i].Trim();

            if (string.IsNullOrEmpty(part)) continue;

            Transform found = current.Find(part);

            if (found == null)
            {
                Debug.LogWarning($"[Chara Loader] Failed to find {part} in path: {path}");
                return null;
            }

            current = found;
        }

        return current.gameObject;
    }
}
