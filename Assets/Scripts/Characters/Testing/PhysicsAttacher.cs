using MagicaCloth2;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsAttacher : MonoBehaviour
{
    public PhysicsBoneData physicsBoneData;

    /// <summary>
    /// Creates BoneCloth components at runtime from a list of PhysicsBoneData.
    /// Each bone data entry creates a separate MagicaCloth GameObject with its own parameters.
    /// </summary>
    /// <param name="character">Character that the GameObjects will be parented to</param>
    /// <param name="boneDataList">List of PhysicsBoneData entries for each bone cloth</param>
    /// <param name="prefixClothName">Prefix name for the created MagicaCloth GameObjects</param>
    public void CreateBoneCloth(GameObject character, List<PhysicsBoneData> boneDataList, string postfixClothName = "_BoneCloth")
    {
        if (character == null || boneDataList == null || boneDataList.Count == 0)
        {
            Debug.LogWarning("Cannot create BoneCloth: missing character or bone data!");
            return;
        }

        // Loop through each PhysicsBoneData entry
        foreach (var boneData in boneDataList)
        {
            if (boneData == null)
            {
                Debug.LogWarning("Null PhysicsBoneData entry skipped!");
                continue;
            }

            // Create container GameObject for this specific bone
            var clothObj = new GameObject(boneData.BoneName + "Cloth");
            clothObj.transform.SetParent(character.transform, false);

            // Add MagicaCloth Component
            MagicaCloth cloth = clothObj.AddComponent<MagicaCloth>();
            ClothSerializeData sdata = cloth.SerializeData;

            // Configure as BoneCloth
            sdata.clothType = ClothProcess.ClothType.BoneCloth;

            // Find the bone transform using BonePath
            if (!string.IsNullOrEmpty(boneData.BonePath))
            {
                Transform boneTransform = character.transform.Find(boneData.BonePath);
                if (boneTransform != null)
                {
                    sdata.rootBones.Add(boneTransform);
                }
                else
                {
                    Debug.LogError($"Bone path '{boneData.BonePath}' not found on character '{character.name}' for bone '{boneData.BoneName}'!");
                }
            }
            else
            {
                Debug.LogError($"No BonePath specified for bone '{boneData.BoneName}'!");
            }

            // Setup parameters from this specific bone data
            sdata.gravity = boneData.Gravity;
            sdata.damping.SetValue(boneData.Damping);
            sdata.angleRestorationConstraint.stiffness.SetValue(boneData.StiffnessValue, boneData.StiffnessCurveStart, boneData.StiffnessCurveEnd, boneData.UseStiffnessCurve);
            sdata.angleRestorationConstraint.velocityAttenuation = boneData.VelocityAttenuation;
            sdata.tetherConstraint.distanceCompression = boneData.DistanceCompression;
            sdata.inertiaConstraint.particleSpeedLimit.SetValue(boneData.UseLimit, boneData.SpeedLimit);
            sdata.colliderCollisionConstraint.mode = ColliderCollisionConstraint.Mode.None;

            cloth.name = boneData.BoneName + postfixClothName;

            // Build and start simulation
            cloth.BuildAndRun();
        }
    }
}
