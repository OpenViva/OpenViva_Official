using System;
using System.Collections.Generic;
using UnityEngine;

public static class VivaScriptSanitizer
{
    private static HashSet<string> _allowedScripts = new();
    private static bool _initialized = false;

    public static void InitializeAllowedScripts()
    {
        if (_initialized) return;

        // TODO: Add more scripts in here
        // Check the name very carefully
        _allowedScripts.Add("PhysicsBone");
        _allowedScripts.Add("VivaDescriptor");

        _initialized = true;
    }

    /// <summary>
    /// Check if the given script type is allowed for exporting.
    /// </summary>
    /// <param name="scriptType"></param>
    /// <returns></returns>
    public static bool IsScriptAllowed(Type scriptType)
    {
        if (!_initialized)
        {
            Debug.LogWarning("[VivaScriptSanitizer] No initialized scripts in the whitelist. Using default list.");
            InitializeAllowedScripts();
        }

        string typeName = scriptType.FullName;

        if (string.IsNullOrEmpty(typeName))
            return false;

        // Check if script is in the whitelist
        if (_allowedScripts.Contains(typeName))
            return true;

        // Reject scripts that inherit from MonoBehaviour but aren't whitelisted
        if (typeof(MonoBehaviour).IsAssignableFrom(scriptType))
        {
            Debug.LogWarning($"[VivaScriptSanitizer] Script '{typeName}' not in whitelist.");
            return false;
        }

        return false;
    }

    /// <summary>
    /// Validate that all custom scripts on the given GameObject are whitelisted.
    /// Unity built-in components are ignored.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public static bool ValidateAllScripts(GameObject gameObject)
    {
        if (gameObject == null) return false;

        foreach (Component component in gameObject.GetComponentsInChildren<Component>(true))
        {
            if (component == null) continue;

            // Skip Unity buit-in components
            if (IsUnityComponent(component.GetType()))
                continue;

            // Skip UnityEngine.Object components
            if (typeof(UnityEngine.Object).IsAssignableFrom(component.GetType()) &&
                    !typeof(MonoBehaviour).IsAssignableFrom(component.GetType()))
                continue;

            // Check if custom script is allowed
            if (!IsScriptAllowed(component.GetType()))
            {
                Debug.LogError($"[VivaScriptSanitizer] Invalid script found: {component.GetType().FullName}");
                return false;
                // TODO: Remove script and continue export instead of stopping
            }
        }

        return true;
    }

    private static bool IsUnityComponent(Type componentType)
    {
        string assemblyName = componentType.Assembly.GetName().Name;
        return assemblyName == "UnityEngine" || assemblyName == "UnityEditor";
    }

    /// <summary>
    /// Get all allowed script types from a GameObject.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public static List<Type> GetAllowedScriptTypes(GameObject gameObject)
    {
        List<Type> allowedTypes = new();

        foreach (Component component in gameObject.GetComponents<Component>())
        {
            if (component == null) continue;

            if (IsScriptAllowed(component.GetType()))
            {
                allowedTypes.Add(component.GetType());
            }
        }

        return allowedTypes;
    }
}
