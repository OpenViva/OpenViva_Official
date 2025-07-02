#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public class ConvertLegacyClips : Editor
{
    [MenuItem("Tools/Convert Legacy Clips")]
    static void ConvertClips()
    {
        Object[] clips = Selection.objects;

        foreach (Object clip in clips)
        {
            if (clip is AnimationClip)
            {
                AnimationClip animationClip = (AnimationClip)clip;
                animationClip.legacy = false;  // Set legacy to false
                Debug.Log($"Converted {animationClip.name} to non-legacy.");
            }
        }
    }
}
#endif