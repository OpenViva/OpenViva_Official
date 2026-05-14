using MagicaCloth2;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsAttacher : MonoBehaviour
{
    [Header("Default Bone Data")]
    [SerializeField] private float gravity = 3;
    [SerializeField] private float damping = 0.05f;
    [SerializeField] private float distanceCompression = 0.5f;

    [Header("Angle Restoration Constraint Data")]
    [SerializeField] private float stiffnessValue = 0.15f;
    [SerializeField] private bool useStiffnessCurve = true;
    [SerializeField] private float stiffnessCurveStart = 1;
    [SerializeField] private float stiffnessCurveEnd = 0.15f;
    [SerializeField] private float velocityAttenuation = 0.6f;

    [Header("Inertia Constraint Particle SpeedLimit")]
    [SerializeField] private bool useLimit = true;
    [SerializeField] private float speedLimit = 3;

    /// <summary>
    /// Creates a BoneCloth at runtime from a list of root bone objects and sets default parameters.
    /// </summary>
    /// <param name="character">Character that the GameObject will be parented to</param>
    /// <param name="rootBoneObjects">List of GameObjects that will act as root bones for this BoneCloth</param>
    /// <param name="clothName">Name for the created MagicaCloth GameObject</param>
    public void CreateBoneCloth(GameObject character, List<GameObject> rootBoneObjects, string clothName = "New_BoneCloth")
    {
        if (character == null || rootBoneObjects == null || rootBoneObjects.Count == 0)
        {
            Debug.LogWarning("Cannot create BoneCloth: missing character or root bones!");
            return;
        }

        // Create container GameObject
        var obj = new GameObject(clothName);
        obj.transform.SetParent(character.transform, false);

        // Add MagicaCloth Component
        MagicaCloth cloth = obj.AddComponent<MagicaCloth>();
        ClothSerializeData sdata = cloth.SerializeData;

        // Configure as BoneCloth
        sdata.clothType = ClothProcess.ClothType.BoneCloth;

        // Add all root bones from the list
        foreach (var rootBone in rootBoneObjects)
        {
            if (rootBone != null && rootBone.transform != null)
            {
                sdata.rootBones.Add(rootBone.transform);
            }
            else
            {
                Debug.LogWarning($"Null or invalid root bone skipped in {clothName}!");
            }
        }

        // Setup parameters
        sdata.gravity = gravity;
        sdata.damping.SetValue(damping);
        sdata.angleRestorationConstraint.stiffness.SetValue(stiffnessValue, stiffnessCurveStart, stiffnessCurveEnd, useStiffnessCurve);
        sdata.angleRestorationConstraint.velocityAttenuation = velocityAttenuation;
        sdata.tetherConstraint.distanceCompression = distanceCompression;
        sdata.inertiaConstraint.particleSpeedLimit.SetValue(useLimit, speedLimit);
        sdata.colliderCollisionConstraint.mode = ColliderCollisionConstraint.Mode.None;

        cloth.name = clothName;

        // Build and start simulation
        cloth.BuildAndRun();
    }
}
