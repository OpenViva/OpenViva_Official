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
    public void CreateBoneCloth(GameObject character, List<PhysicsBoneData> boneDataList)
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
            var clothObj = new GameObject(boneData.GameObjectName);
            clothObj.transform.SetParent(character.transform, false);

            // Add MagicaCloth Component
            MagicaCloth cloth = clothObj.AddComponent<MagicaCloth>();
            ClothSerializeData sdata = cloth.SerializeData;

            // Configure as BoneCloth
            sdata.clothType = ClothProcess.ClothType.BoneCloth;

            // Fill in the root bones if there are any
            if (boneData.bonePaths.Count > 0)
            {
                List<Transform> rootBones = new();

                foreach (string bonePath in boneData.bonePaths)
                {
                    if (!string.IsNullOrEmpty(bonePath))
                    {
                        sdata.rootBones.Add(VivaUtilities.FindGameObjectByPath(character, bonePath).transform);
                    }
                    else
                    {
                        Debug.LogError($"No BonePath specified for bone '{boneData.GameObjectName}'!");
                    }
                }
            }

            // Setup parameters from this specific bone data
            sdata.gravity = boneData.Gravity;
            sdata.damping.SetValue(boneData.Damping);
            sdata.angleRestorationConstraint.stiffness.SetValue(boneData.StiffnessValue, boneData.StiffnessCurveStart, boneData.StiffnessCurveEnd, boneData.UseStiffnessCurve);
            sdata.angleRestorationConstraint.velocityAttenuation = boneData.VelocityAttenuation;
            sdata.tetherConstraint.distanceCompression = boneData.DistanceCompression;
            sdata.inertiaConstraint.particleSpeedLimit.SetValue(boneData.UseLimit, boneData.SpeedLimit);
            sdata.colliderCollisionConstraint.mode = ColliderCollisionConstraint.Mode.None;

            cloth.name = boneData.GameObjectName;

            // ONLY start adding colliders if there have been any given for the current bone
            if (boneData.Colliders.Count > 0)
            {
                AddCollidersToBones(character, cloth, boneData.Colliders);

                // Set the collision type of this bone, the radius and do not use curve
                sdata.colliderCollisionConstraint.mode = ColliderCollisionConstraint.Mode.Point;
                sdata.radius = new() { value = 0.036f, useCurve = false }; // TODO: This could be an option in the SDK
            }

            // Build and start simulation
            cloth.BuildAndRun();
        }
    }

    #region Helpers
    /// <summary>
    /// Loop through the given `Colliders To Add`, add a new collider if bone is found and set up all it's values.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="targetCloth"></param>
    /// <param name="collidersToAdd"></param>
    private void AddCollidersToBones(GameObject character, MagicaCloth targetCloth, List<ColliderData> collidersToAdd)
    {
        List<ColliderComponent> colliderList = targetCloth.SerializeData.colliderCollisionConstraint.colliderList;
        bool addedAnyCollider = false;

        foreach (ColliderData collider in collidersToAdd)
        {
            GameObject foundBone = VivaUtilities.FindGameObjectByPath(character, collider.BonePath);

            if (foundBone != null)
            {
                if (collider.ShapeType == ShapeType.Capsule)
                {
                    // Try to find new collider or add one
                    MagicaCapsuleCollider newMagicaCapsuleCollider = foundBone.GetComponent<MagicaCapsuleCollider>()
                              ?? foundBone.AddComponent<MagicaCapsuleCollider>();

                    // 1. Set axis
                    switch (collider.AxisDirection)
                    {
                        case Direction.X: newMagicaCapsuleCollider.direction = MagicaCapsuleCollider.Direction.X;
                            break;
                        case Direction.Y: newMagicaCapsuleCollider.direction = MagicaCapsuleCollider.Direction.Y;
                            break;
                        case Direction.Z: newMagicaCapsuleCollider.direction = MagicaCapsuleCollider.Direction.Z;
                            break;
                    }
                    // 2. Set sizes
                    newMagicaCapsuleCollider.SetSize(collider.Radius, collider.Radius, collider.Length);
                    // 3. Set the offset center
                    newMagicaCapsuleCollider.center = collider.Center;

                    if (!colliderList.Contains(newMagicaCapsuleCollider))
                    {
                        colliderList.Add(newMagicaCapsuleCollider);
                        addedAnyCollider = true;
                    }
                }
                else
                {
                    MagicaSphereCollider newMagicaSphereCollider = foundBone.GetComponent<MagicaSphereCollider>()
                              ?? foundBone.AddComponent<MagicaSphereCollider>();

                    newMagicaSphereCollider.SetSize(collider.Radius);
                    newMagicaSphereCollider.center = collider.Center;

                    if (!colliderList.Contains(newMagicaSphereCollider))
                    {
                        colliderList.Add(newMagicaSphereCollider);
                        addedAnyCollider = true;
                    }
                }
            }
        }

        if (addedAnyCollider)
        {
            targetCloth.SetParameterChange();
        }
    }
    #endregion
}
