using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectHelpers
{
    /// <summary>
    /// Deactivates every GameObject in the given list.
    /// Null entries and already-inactive objects are ignored safely.
    /// </summary>
    /// <param name="objects">List of GameObjects to disable.</param>
    public static void DeactivateList(this IEnumerable<GameObject> objects)
    {
        if (objects == null) return;

        foreach (var item in objects)
        {
            if (item != null && item.activeSelf)   // activeSelf = true only if the object itself is active
                item.SetActive(false);
        }
    }

    // Overload for List<GameObject> just calls the IEnumerable version
    public static void DeactivateList(this List<GameObject> objects) => DeactivateList((IEnumerable<GameObject>)objects);

    /// <summary>
    /// Deactivates all GameObjects in the list over multiple frames (async via coroutine).
    /// Ideal for large lists to avoid frame hitches.
    /// </summary>
    /// <param name="objects">List of GameObjects to disable.</param>
    /// <param name="batchSize">Amount of objects to process per frame.</param>
    /// <returns></returns>
    public static IEnumerator DeactivateListAsync(this List<GameObject> objects, int batchSize = 50)
    {
        if (objects == null) yield break;

        int count = objects.Count;
        for (int i = 0; i < count; i += batchSize)
        {
            int end = Mathf.Min(i + batchSize, count);
            for (int j = i; j < end; j++)
            {
                var go = objects[j];
                if (go != null && go.activeSelf)
                    go.SetActive(false);
            }
            yield return null;  // Yield to next frame after each batch
        }
    }
}
