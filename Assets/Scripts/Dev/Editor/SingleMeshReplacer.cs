using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SingleMeshReplacer : EditorWindow
{
    public GameObject replacementPrefab;
    private Mesh targetMesh;

    private List<GameObject> matchingObjects = new List<GameObject>();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Viva/Simple Prefab Replacer")]
    public static void ShowWindow()
    {
        GetWindow<SingleMeshReplacer>("Single Mesh Replacer");
    }

    void OnGUI()
    {
        GUILayout.Label("1. Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        replacementPrefab = (GameObject)EditorGUILayout.ObjectField("Replacement Prefab", replacementPrefab, typeof(GameObject), false);

        // Auto-detect the mesh on the ROOT of the prefab when dropped in
        if (EditorGUI.EndChangeCheck())
        {
            matchingObjects.Clear();

            if (replacementPrefab != null)
            {
                MeshFilter mf = replacementPrefab.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    targetMesh = mf.sharedMesh;
                }
                else
                {
                    targetMesh = null;
                    Debug.LogWarning("Simple Replacer: The selected Prefab does not have a MeshFilter on its root GameObject.");
                }
            }
            else
            {
                targetMesh = null;
            }
        }

        // Show the detected mesh (read-only)
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Detected Root Mesh", targetMesh, typeof(Mesh), false);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
        GUILayout.Label("2. Find Objects", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(replacementPrefab == null || targetMesh == null);
        if (GUILayout.Button("Scan Scene for Matches", GUILayout.Height(30)))
        {
            ScanScene();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        // 3. List the found objects
        GUILayout.Label($"Objects Found: {matchingObjects.Count}", EditorStyles.boldLabel);

        if (matchingObjects.Count > 0)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(100), GUILayout.MaxHeight(250));

            for (int i = matchingObjects.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.ObjectField(matchingObjects[i], typeof(GameObject), true);

                // Button to remove individual items from the replacement list
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    matchingObjects.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // 4. Apply Button
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f); // Green button
            if (GUILayout.Button("Apply Replacement", GUILayout.Height(40)))
            {
                ReplaceObjects();
            }
            GUI.backgroundColor = Color.white;
        }
    }

    void ScanScene()
    {
        matchingObjects.Clear();

        if (targetMesh == null) return;

        MeshFilter[] allSceneFilters = FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);

        foreach (MeshFilter filter in allSceneFilters)
        {
            GameObject obj = filter.gameObject;

            // Skip project assets
            if (PrefabUtility.IsPartOfPrefabAsset(obj)) continue;

            // Skip objects that are already this exact prefab
            if (PrefabUtility.GetCorrespondingObjectFromSource(obj) == replacementPrefab) continue;

            // Check if the scene object's root mesh matches the prefab's root mesh
            if (filter.sharedMesh == targetMesh)
            {
                matchingObjects.Add(obj);
            }
        }

        if (matchingObjects.Count == 0)
        {
            Debug.Log("Simple Replacer: No matching objects found in the current scene.");
        }
    }

    void ReplaceObjects()
    {
        if (replacementPrefab == null || targetMesh == null) return;

        int replaceCount = 0;

        foreach (GameObject oldObject in matchingObjects)
        {
            if (oldObject == null) continue;

            // 1. Instantiate the new prefab
            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(replacementPrefab);

            // 2. Register Undo
            Undo.RegisterCreatedObjectUndo(newObject, "Replace Object");

            // 3. Copy Position, Rotation, Scale, and Parent hierarchy
            newObject.transform.position = oldObject.transform.position;
            newObject.transform.rotation = oldObject.transform.rotation;
            newObject.transform.localScale = oldObject.transform.localScale;
            newObject.transform.SetParent(oldObject.transform.parent);

            // 4. Destroy the old object safely
            Undo.DestroyObjectImmediate(oldObject);

            replaceCount++;
        }

        matchingObjects.Clear();
        Debug.Log($"Simple Replacer: Successfully replaced {replaceCount} objects in the scene.");
    }
}