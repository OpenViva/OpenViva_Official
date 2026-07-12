using UnityEngine;

public class PhysicsBone : MonoBehaviour
{
    [Header("Bone Configuration")]
    public Transform boneTransform;
    public string boneName;

    // Preset System
    [Header("Preset")]
    public BonePreset preset = BonePreset.LongHair;

    public enum BonePreset
    {
        Skirt,
        ShortHair,
        LongHair,
        AnimalTail,
        AnimalEars
    }

    [Header("Preset Values")]
    public float gravity = 2f;
    public float damping = 0.05f;
    public float distanceCompression = 0.5f;
    public float stiffnessValue = 0.15f;
    public bool useStiffnessCurve = true;
    public float stiffnessCurveStart = 1f;
    public float stiffnessCurveEnd = 0.15f;
    public float velocityAttenuation = 0.6f;
    public bool useLimit = true;
    public float speedLimit = 3f;

    // Debug visuals
    [Header("Gizmo Settings")]
    public float rootBoneSize = 0.02f;
    public float childBoneSize = 0.016f;
    public bool showHierarchy = true;
    public Color markerColor = Color.green;

    private void OnValidate()
    {
        // Auto-fill bone name from transform
        if (boneTransform != null)
        {
            boneName = boneTransform.name;
        }

        // Auto-configure values based on preset
        ApplyPresetDefaults();
    }

    private void ApplyPresetDefaults()
    {
        switch (preset)
        {
            case BonePreset.Skirt:
                gravity = 2f;
                damping = 0.05f;
                stiffnessValue = 0.15f;
                break;
            case BonePreset.ShortHair:
                gravity = 1.5f;
                damping = 0.1f;
                stiffnessValue = 0.3f;
                break;
            case BonePreset.LongHair:
                gravity = 1.0f;
                damping = 0.08f;
                stiffnessValue = 0.2f;
                break;
            case BonePreset.AnimalTail:
                gravity = 2.5f;
                damping = 0.15f;
                stiffnessValue = 0.25f;
                break;
            case BonePreset.AnimalEars:
                gravity = 1.8f;
                damping = 0.07f;
                stiffnessValue = 0.35f;
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showHierarchy || boneTransform == null)
        {
            return;
        }

        // Draw sphere on the root bone
        Gizmos.color = markerColor;
        Gizmos.DrawSphere(boneTransform.position, rootBoneSize);

        DrawBoneHierarchy(boneTransform);
    }

    private void DrawBoneHierarchy(Transform bone)
    {
        foreach (Transform child in bone)
        {
            // Draw line between parent and child
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bone.position, child.position);

            // Draw sphere for child
            Gizmos.color = markerColor;
            Gizmos.DrawSphere(child.position, childBoneSize);

            // Recursively draw children of this child
            DrawBoneHierarchy(child);
        }
    }
}