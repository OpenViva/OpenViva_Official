using MagicaCloth2;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsAttacher : MonoBehaviour
{
    [SerializeField]
    List<GameObject> rootBoneObjects;

    void Start()
    {

    }

    /// <summary>
    /// Creates a BoneCloth at runtime from a list of root bone objects and sets default parameters.
    /// </summary>
    /// <param name="character">Character that the GameObject will be parented to</param>
    /// <param name="rootBoneObjects">List of GameObjects that will act as root bones for this BoneCloth</param>
    /// <param name="clothName">Name for the created MagicaCloth GameObject</param>
    public void CreateBoneCloths(GameObject character, string clothName = "New_BoneCloth")
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
        sdata.gravity = 3.0f;
        sdata.damping.SetValue(0.05f);
        sdata.angleRestorationConstraint.stiffness.SetValue(0.15f, 1.0f, 0.15f, true);
        sdata.angleRestorationConstraint.velocityAttenuation = 0.6f;
        sdata.tetherConstraint.distanceCompression = 0.5f;
        sdata.inertiaConstraint.particleSpeedLimit.SetValue(true, 3.0f);
        sdata.colliderCollisionConstraint.mode = ColliderCollisionConstraint.Mode.None;

        cloth.name = clothName;

        // Build and start simulation
        cloth.BuildAndRun();
    }
}
