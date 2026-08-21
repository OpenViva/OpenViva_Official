using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LODReplacer : EditorWindow
{
    public GameObject replacementPrefab;

    // Search target
    private List<Mesh> targetMeshes = new();

    private List<GameObject> matchingObjects = new();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Viva/Replace LODs With Prefab")]
    public static void ShowWindow()
    {
        GetWindow<LODReplacer>("LOD Replacer");
    }

    void OnGUI()
    {
        GUILayout.Label("1. Setup", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        replacementPrefab = (GameObject)EditorGUILayout.ObjectField("Replacement Prefab", replacementPrefab, typeof(GameObject), false);

        if (EditorGUI.EndChangeCheck())
        {
            UpdateTargetMeshes();
            matchingObjects.Clear();
        }

        if (targetMeshes.Count > 0)
        {
            EditorGUILayout.HelpBox($"Detected {targetMeshes.Count} distinct meshes in the Prefab.", MessageType.Info);

            // Show them in disabled fields just for visual confirmation
            EditorGUI.BeginDisabledGroup(true);
            foreach (Mesh m in targetMeshes)
            {
                EditorGUILayout.ObjectField(m, typeof(Mesh), false);
            }
            EditorGUI.EndDisabledGroup();
        }
        else if (replacementPrefab != null)
        {
            EditorGUILayout.HelpBox("No meshes found in the Prefab's children.", MessageType.Warning);
        }

        EditorGUILayout.Space();
        GUILayout.Label("2. Find Objects", EditorStyles.boldLabel);

        if (GUILayout.Button("Scan Scene for Matching LOD Groups", GUILayout.Height(30)))
        {
            ScanScene();
        }

        EditorGUILayout.Space();

        GUILayout.Label($"Objects Found: {matchingObjects.Count}", EditorStyles.boldLabel);

        if (matchingObjects.Count > 0)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(100), GUILayout.MaxHeight(250));

            for (int i = matchingObjects.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.ObjectField(matchingObjects[i], typeof(GameObject), true);

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    matchingObjects.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
            if (GUILayout.Button("Apply Replacement", GUILayout.Height(40)))
            {
                ApplyReplacement();
            }
            GUI.backgroundColor = Color.white;
        }
    }

    void UpdateTargetMeshes()
    {
        targetMeshes.Clear();
        if (replacementPrefab == null) return;

        // Get all MeshFilters in the prefab and its children
        MeshFilter[] mfs = replacementPrefab.GetComponentsInChildren<MeshFilter>(true);
        foreach (MeshFilter mf in mfs)
        {
            // Add unique meshes to the target list
            if (mf.sharedMesh != null && !targetMeshes.Contains(mf.sharedMesh))
            {
                targetMeshes.Add(mf.sharedMesh);
            }
        }
    }

    void ScanScene()
    {
        matchingObjects.Clear();

        if (targetMeshes.Count == 0)
        {
            Debug.LogWarning("[LOD Replacer]: The assigned Prefab has no meshes to match against.");
            return;
        }

        HashSet<Mesh> targetMeshSet = new(targetMeshes);

        // Find all objects in the scene with an LODGroup component
        LODGroup[] allLODGroups = FindObjectsByType<LODGroup>(FindObjectsSortMode.None);

        foreach (LODGroup lodGroup in allLODGroups)
        {
            GameObject obj = lodGroup.gameObject;

            // Skip project assets
            if (PrefabUtility.IsPartOfPrefabAsset(obj)) continue;

            MeshFilter[] objMfs = obj.GetComponentsInChildren<MeshFilter>(true);
            HashSet<Mesh> objMeshSet = new();

            foreach (MeshFilter mf in objMfs)
            {
                if (mf.sharedMesh != null)
                {
                    objMeshSet.Add(mf.sharedMesh);
                }
            }

            // Only a match if the scene object contains the exact same set of meshes as our Prefab
            if (objMeshSet.SetEquals(targetMeshSet))
            {
                matchingObjects.Add(obj);
            }
        }

        if (matchingObjects.Count == 0)
        {
            Debug.Log("[LOD Replacer]: No matching LOD groups found in the current scene.");
        }
    }

    void ApplyReplacement()
    {
        if (replacementPrefab == null)
        {
            Debug.LogWarning("[LOD Replacer]: Please assign a Replacement Prefab before applying.");
            return;
        }

        int replaceCount = 0;

        foreach (GameObject oldObject in matchingObjects)
        {
            if (oldObject == null) continue;

            // 1. Instantiate the new prefab
            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(replacementPrefab);

            // 2. Register Undo
            Undo.RegisterCreatedObjectUndo(newObject, "Replace LOD Object");

            // 3. Copy Position, Rotation, Scale, and Parent
            newObject.transform.SetPositionAndRotation(oldObject.transform.position, oldObject.transform.rotation);
            newObject.transform.localScale = oldObject.transform.localScale;
            newObject.transform.SetParent(oldObject.transform.parent);

            // 4. Destroy the old object
            Undo.DestroyObjectImmediate(oldObject);

            replaceCount++;
        }

        matchingObjects.Clear();

        Debug.Log($"[LOD Replacer]: Successfully replaced {replaceCount} LOD objects in the scene.");
    }
}