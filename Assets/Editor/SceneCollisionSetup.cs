#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Adds missing colliders to scene objects (walls, floors, props) so the player
/// and physics hands can't pass through them.
/// 
/// Matches old OpenViva's collision approach:
/// - Static environment meshes get MeshCollider (concave by default)
/// - Small props get convex MeshCollider (for Rigidbody compatibility)
/// - Objects already having a collider are skipped
/// 
/// Usage: Select objects in Hierarchy → Tools → Add Missing Colliders
/// Or: Tools → Add Missing Colliders (All Static) for the entire scene
/// </summary>
public class SceneCollisionSetup : Editor
{
    [MenuItem("Tools/Add Missing Colliders (Selected)")]
    static void AddCollidersToSelected()
    {
        if (Selection.transforms.Length == 0)
        {
            EditorUtility.DisplayDialog("Add Colliders", "No objects selected.\nSelect objects in the Hierarchy first.", "OK");
            return;
        }

        int added = 0;
        foreach (Transform t in Selection.transforms)
        {
            added += AddCollidersRecursive(t);
        }

        Debug.Log($"[CollisionSetup] Added {added} colliders to selected objects");
        EditorUtility.DisplayDialog("Add Colliders", $"Added {added} colliders to selected objects and their children.", "OK");
    }

    [MenuItem("Tools/Add Missing Colliders (All Static)")]
    static void AddCollidersToAllStatic()
    {
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        int added = 0;
        foreach (var root in roots)
        {
            added += AddCollidersRecursive(root.transform, staticOnly: true);
        }

        Debug.Log($"[CollisionSetup] Added {added} colliders to static objects in scene");
        EditorUtility.DisplayDialog("Add Colliders",
            $"Added {added} colliders to static objects.\n\n" +
            "Objects that already had colliders were skipped.\n" +
            "Save the scene to persist changes.",
            "OK");
    }

    [MenuItem("Tools/Add Missing Colliders (Everything in Scene)")]
    static void AddCollidersToEverything()
    {
        if (!EditorUtility.DisplayDialog("Add Colliders to Everything",
            "This will add MeshColliders to ALL objects with MeshRenderers that don't already have colliders.\n\n" +
            "This may take a moment for large scenes. Continue?",
            "Yes", "Cancel"))
            return;

        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        int added = 0;
        foreach (var root in roots)
        {
            added += AddCollidersRecursive(root.transform, staticOnly: false);
        }

        Debug.Log($"[CollisionSetup] Added {added} colliders to all objects in scene");
        EditorUtility.DisplayDialog("Add Colliders",
            $"Added {added} colliders.\n\nSave the scene to persist changes.",
            "OK");
    }

    /// <summary>
    /// Recursively add MeshColliders to objects that have a MeshRenderer/MeshFilter
    /// but no existing Collider component.
    /// </summary>
    static int AddCollidersRecursive(Transform root, bool staticOnly = false)
    {
        int count = 0;

        // Check this object
        if (!staticOnly || root.gameObject.isStatic)
        {
            if (ShouldAddCollider(root.gameObject))
            {
                Undo.RegisterCompleteObjectUndo(root.gameObject, "Add Missing Collider");
                AddAppropriateCollider(root.gameObject);
                count++;
            }
        }

        // Recurse into children
        foreach (Transform child in root)
        {
            count += AddCollidersRecursive(child, staticOnly);
        }

        return count;
    }

    /// <summary>
    /// Returns true if the object has a visible mesh but no collider.
    /// </summary>
    static bool ShouldAddCollider(GameObject go)
    {
        // Skip if it already has any collider
        if (go.GetComponent<Collider>() != null)
            return false;

        // Must have a mesh to generate a collider from
        var mf = go.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return false;

        // Must have a renderer (skip invisible/helper objects)
        var mr = go.GetComponent<MeshRenderer>();
        if (mr == null)
            return false;

        // Skip very tiny objects (particles, decals, etc.)
        if (mf.sharedMesh.vertexCount < 3)
            return false;

        return true;
    }

    /// <summary>
    /// Add the appropriate collider type based on the object's properties.
    /// - Objects with Rigidbody get convex MeshCollider (required for dynamic physics)
    /// - Small objects get convex MeshCollider (better performance)
    /// - Large environment meshes get concave MeshCollider (accurate collision)
    /// </summary>
    static void AddAppropriateCollider(GameObject go)
    {
        var mf = go.GetComponent<MeshFilter>();
        bool hasRigidbody = go.GetComponent<Rigidbody>() != null;
        bool isSmall = mf.sharedMesh.vertexCount < 256;

        var mc = Undo.AddComponent<MeshCollider>(go);
        mc.sharedMesh = mf.sharedMesh;

        // Rigidbodies require convex colliders; small objects benefit from convex too
        if (hasRigidbody || isSmall)
        {
            mc.convex = true;
        }
        else
        {
            mc.convex = false;
        }

        EditorUtility.SetDirty(go);
    }
}

#endif
