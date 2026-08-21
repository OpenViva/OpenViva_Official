using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExactReplacer : EditorWindow
{
    public enum SearchMode
    {
        ExactHierarchy,
        SingleMeshNoChildren
    }

    public GameObject replacementPrefab;
    public SearchMode searchMode = SearchMode.ExactHierarchy;

    private List<GameObject> matchingObjects = new();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Smart Object Replacer")]
    public static void ShowWindow()
    {
        GetWindow<ExactReplacer>("Exact Replacer");
    }

    void OnGUI()
    {
        GUILayout.Label("1. Setup", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        replacementPrefab = (GameObject)EditorGUILayout.ObjectField("Replacement Prefab", replacementPrefab, typeof(GameObject), false);

        if (EditorGUI.EndChangeCheck())
        {
            matchingObjects.Clear();

            if (replacementPrefab != null)
            {
                if (replacementPrefab.transform.childCount == 0 && replacementPrefab.GetComponent<MeshFilter>() != null)
                {
                    searchMode = SearchMode.SingleMeshNoChildren;
                }
                else
                {
                    searchMode = SearchMode.ExactHierarchy;
                }
            }
        }

        searchMode = (SearchMode)EditorGUILayout.EnumPopup("Search Mode", searchMode);

        bool canScan = ValidateSetup();

        EditorGUILayout.Space();
        GUILayout.Label("2. Find Objects", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(!canScan);
        if (GUILayout.Button("Scan Scene", GUILayout.Height(30)))
        {
            ScanScene();
        }
        EditorGUI.EndDisabledGroup();

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

    bool ValidateSetup()
    {
        if (replacementPrefab == null) return false;

        if (searchMode == SearchMode.ExactHierarchy)
        {
            int childCount = replacementPrefab.transform.childCount;
            if (childCount == 0)
            {
                EditorGUILayout.HelpBox("Hierarchy Mode: This Prefab has no children! Please assign a Prefab with a hierarchy, or switch to Single Mesh mode.", MessageType.Error);
                return false;
            }
            EditorGUILayout.HelpBox($"Hierarchy Mode: Will scan for GameObjects with exactly {childCount} direct children and matching sub-hierarchy components (Names are ignored).", MessageType.Info);
            return true;
        }
        else if (searchMode == SearchMode.SingleMeshNoChildren)
        {
            MeshFilter mf = replacementPrefab.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
                EditorGUILayout.HelpBox("Single Mesh Mode: This Prefab has no MeshFilter! Please assign a Prefab with a 3D model on its root.", MessageType.Error);
                return false;
            }
            EditorGUILayout.HelpBox($"Single Mesh Mode: Will scan for GameObjects that have NO children, and use the exact mesh: {mf.sharedMesh.name}", MessageType.Info);
            return true;
        }

        return false;
    }

    void ScanScene()
    {
        matchingObjects.Clear();

        if (searchMode == SearchMode.ExactHierarchy)
        {
            ScanForHierarchy();
        }
        else if (searchMode == SearchMode.SingleMeshNoChildren)
        {
            ScanForSingleMesh();
        }

        if (matchingObjects.Count == 0)
        {
            Debug.Log("[Exact Replacer] No matching objects found in the current scene.");
        }
    }

    void ScanForHierarchy()
    {
        Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);

        foreach (Transform sceneTransform in allTransforms)
        {
            GameObject obj = sceneTransform.gameObject;
            if (PrefabUtility.IsPartOfPrefabAsset(obj)) continue;
            if (PrefabUtility.GetCorrespondingObjectFromSource(obj) == replacementPrefab) continue;

            if (IsExactChildMatch(replacementPrefab.transform, sceneTransform))
            {
                matchingObjects.Add(obj);
            }
        }
    }

    void ScanForSingleMesh()
    {
        Mesh targetMesh = replacementPrefab.GetComponent<MeshFilter>().sharedMesh;
        MeshFilter[] allSceneFilters = FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);

        foreach (MeshFilter filter in allSceneFilters)
        {
            GameObject obj = filter.gameObject;

            if (PrefabUtility.IsPartOfPrefabAsset(obj)) continue;
            if (PrefabUtility.GetCorrespondingObjectFromSource(obj) == replacementPrefab) continue;

            if (obj.transform.childCount == 0 && filter.sharedMesh == targetMesh)
            {
                matchingObjects.Add(obj);
            }
        }
    }

    bool IsExactChildMatch(Transform prefabTransform, Transform sceneTransform)
    {
        if (prefabTransform.childCount != sceneTransform.childCount) return false;

        for (int i = 0; i < prefabTransform.childCount; i++)
        {
            Transform pChild = prefabTransform.GetChild(i);
            Transform sChild = sceneTransform.GetChild(i);

            if (!HaveSameComponentTypes(pChild.gameObject, sChild.gameObject)) return false;

            if (!IsExactChildMatch(pChild, sChild)) return false;
        }

        return true;
    }

    bool HaveSameComponentTypes(GameObject a, GameObject b)
    {
        Component[] compsA = a.GetComponents<Component>();
        Component[] compsB = b.GetComponents<Component>();

        HashSet<System.Type> typesA = new();
        foreach (Component c in compsA)
        {
            if (c != null) typesA.Add(c.GetType());
        }

        HashSet<System.Type> typesB = new();
        foreach (Component c in compsB)
        {
            if (c != null) typesB.Add(c.GetType());
        }

        return typesA.SetEquals(typesB);
    }

    void ApplyReplacement()
    {
        if (replacementPrefab == null) return;

        int replaceCount = 0;

        foreach (GameObject oldObject in matchingObjects)
        {
            if (oldObject == null) continue;

            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(replacementPrefab);
            Undo.RegisterCreatedObjectUndo(newObject, "Replace Object");

            newObject.transform.position = oldObject.transform.position;
            newObject.transform.rotation = oldObject.transform.rotation;
            newObject.transform.localScale = oldObject.transform.localScale;
            newObject.transform.SetParent(oldObject.transform.parent);

            Undo.DestroyObjectImmediate(oldObject);
            replaceCount++;
        }

        matchingObjects.Clear();
        Debug.Log($"[Exact Replacer] Successfully replaced {replaceCount} objects in the scene.");
    }
}