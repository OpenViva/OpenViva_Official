using System.Collections.Generic;
using UnityEngine;

public static class GameObjectHelpers
{
    /// <summary>
    /// Deactivates every GameObject in the given list.
    /// Null entries and already-inactive objects are ignored safely.
    /// </summary>
    /// <param name="objects">The list of GameObjects to turn off.</param>
    public static void DeactivateAllGameobjects(this IEnumerable<GameObject> objects)
    {
        if (objects == null) return;

        foreach (var go in objects)
        {
            if (go != null && go.activeSelf)   // activeSelf = true only if the object itself is active
                go.SetActive(false);
        }
    }

    // Overload for List<GameObject> just calls the IEnumerable version
    public static void DeactivateAllGameobjects(this List<GameObject> objects) => DeactivateAllGameobjects((IEnumerable<GameObject>)objects);
}
