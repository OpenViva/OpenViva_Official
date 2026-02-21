#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One-click setup: configures the PlayerKB prefab with the physics-based
/// interaction system matching old OpenViva's hand architecture.
/// Hands become real Rigidbodies with SphereColliders that push/interact physically.
/// Run via menu: Tools > Setup Interaction System
/// </summary>
public class InteractionSystemSetup : Editor
{
    [MenuItem("Tools/Setup Interaction System")]
    static void SetupInteractionSystem()
    {
        // --- Find PlayerKB in scene ---
        var playerKB = GameObject.Find("PlayerKB");
        if (playerKB == null)
        {
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == "PlayerKB" || root.GetComponentInChildren<PlayerKB_Movement>() != null)
                {
                    playerKB = root;
                    break;
                }
            }
        }

        if (playerKB == null)
        {
            EditorUtility.DisplayDialog("Setup Failed",
                "Could not find 'PlayerKB' GameObject in the current scene.\n\n" +
                "Make sure the PlayerKB prefab is placed in the scene before running this tool.",
                "OK");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(playerKB, "Setup Interaction System");

        // --- Find key child transforms ---
        Transform handL = FindChild(playerKB.transform, "hand_l");
        Transform handR = FindChild(playerKB.transform, "hand_r");
        Transform wristL = FindChild(playerKB.transform, "wrist_l");
        Transform wristR = FindChild(playerKB.transform, "wrist_r");
        Transform mainCam = FindChild(playerKB.transform, "Main Camera KB");

        if (handL == null || handR == null || wristL == null || wristR == null)
        {
            EditorUtility.DisplayDialog("Setup Failed",
                "Could not find hand_l/hand_r/wrist_l/wrist_r in PlayerKB hierarchy.",
                "OK");
            return;
        }

        // --- 1. Setup player Rigidbody (replaces CharacterController) ---
        SetupPlayerRigidbody(playerKB);

        // --- 2. Enable wrist_r Animator ---
        var wristRAnimator = wristR.GetComponent<Animator>();
        if (wristRAnimator != null && !wristRAnimator.enabled)
        {
            wristRAnimator.enabled = true;
            Debug.Log("[Setup] Enabled wrist_r Animator");
        }

        // --- 3. Setup hand_l as physics hand ---
        var leftGrab = SetupPhysicsHand(handL.gameObject, isLeft: true);

        // --- 4. Setup hand_r as physics hand ---
        var rightGrab = SetupPhysicsHand(handR.gameObject, isLeft: false);

        // --- 5. Add HandAnimator to wrists ---
        var leftAnim = GetOrAdd<HandAnimator>(wristL.gameObject);
        leftAnim.isLeftHand = true;
        leftAnim.handAnimator = wristL.GetComponent<Animator>();

        var rightAnim = GetOrAdd<HandAnimator>(wristR.gameObject);
        rightAnim.isLeftHand = false;
        rightAnim.handAnimator = wristRAnimator;

        // --- 6. DesktopHandDriver on PlayerKB ---
        var handDriver = GetOrAdd<DesktopHandDriver>(playerKB);
        handDriver.cameraTransform = mainCam;
        handDriver.leftHandGrab = leftGrab;
        handDriver.rightHandGrab = rightGrab;
        handDriver.leftWristBone = wristL;
        handDriver.rightWristBone = wristR;

        // --- 7. PlayerKB_GrabController on PlayerKB ---
        var grabController = GetOrAdd<PlayerKB_GrabController>(playerKB);
        grabController.leftHand = leftGrab;
        grabController.rightHand = rightGrab;
        grabController.handDriver = handDriver;
        grabController.leftHandAnimator = leftAnim;
        grabController.rightHandAnimator = rightAnim;
        if (mainCam != null)
            grabController.playerCamera = mainCam.GetComponent<Camera>();

        // --- 8. DesktopGestures on PlayerKB ---
        var gestures = GetOrAdd<DesktopGestures>(playerKB);
        gestures.leftHandAnimator = leftAnim;
        gestures.rightHandAnimator = rightAnim;
        gestures.leftHandGrab = leftGrab;
        gestures.rightHandGrab = rightGrab;

        // --- 9. Wire PlayerManager if present ---
        var playerManager = Object.FindFirstObjectByType<PlayerManager>();
        if (playerManager != null)
        {
            var so = new SerializedObject(playerManager);
            var leftHandProp = so.FindProperty("_leftHandGrab");
            var rightHandProp = so.FindProperty("_rightHandGrab");
            if (leftHandProp != null)
                leftHandProp.objectReferenceValue = leftGrab;
            if (rightHandProp != null)
                rightHandProp.objectReferenceValue = rightGrab;
            so.ApplyModifiedProperties();
            Debug.Log("[Setup] PlayerManager hand references updated");
        }

        // --- 10. Create crosshair ---
        SetupCrosshair(playerKB, grabController);

        // --- Mark dirty ---
        EditorUtility.SetDirty(playerKB);
        PrefabUtility.RecordPrefabInstancePropertyModifications(playerKB);

        Debug.Log("[Setup] ===== Interaction System setup complete! =====");
        EditorUtility.DisplayDialog("Setup Complete",
            "Physics-based interaction system wired to PlayerKB!\n\n" +
            "Components configured:\n" +
            "- Player Rigidbody (mass=100, freeze rotation, gravity)\n" +
            "- CapsuleCollider (matching old PLAYER.prefab)\n" +
            "- HandGrabSystem + Rigidbody on hand_l/hand_r\n" +
            "- SphereCollider (r=0.05) on each hand\n" +
            "- HandAnimator on wrist_l/wrist_r\n" +
            "- DesktopHandDriver, GrabController, Gestures on root\n" +
            "- Crosshair Canvas\n\n" +
            "Save the scene/prefab to persist.",
            "OK");
    }

    static void SetupPlayerRigidbody(GameObject playerKB)
    {
        // Remove CharacterController if present (old system used Rigidbody)
        var cc = playerKB.GetComponent<CharacterController>();
        if (cc != null)
        {
            Debug.Log("[Setup] Removing CharacterController (replaced by Rigidbody)");
            Undo.DestroyObjectImmediate(cc);
        }

        // Add Rigidbody matching old PLAYER.prefab
        var rb = GetOrAdd<Rigidbody>(playerKB);
        rb.mass = 100f;
        rb.linearDamping = 1f;
        rb.angularDamping = 50f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Add CapsuleCollider matching old PLAYER.prefab
        var capsule = GetOrAdd<CapsuleCollider>(playerKB);
        capsule.radius = 0.09f;
        capsule.height = 1.5f;
        capsule.center = new Vector3(0f, 0.75f, 0f);
        capsule.direction = 1; // Y-axis

        Debug.Log("[Setup] Player Rigidbody + CapsuleCollider configured (matching old PLAYER.prefab)");
    }

    static HandGrabSystem SetupPhysicsHand(GameObject handGO, bool isLeft)
    {
        // Add Rigidbody (old: mass=5, no gravity, not kinematic)
        var rb = GetOrAdd<Rigidbody>(handGO);
        rb.mass = 5f;
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Replace BoxCollider with SphereCollider (old: radius 0.05)
        var boxCol = handGO.GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            Undo.DestroyObjectImmediate(boxCol);
        }

        var sphereCol = GetOrAdd<SphereCollider>(handGO);
        sphereCol.radius = 0.05f;
        sphereCol.isTrigger = false; // Physical, not trigger — hands push things!

        // Add HandGrabSystem
        var grab = GetOrAdd<HandGrabSystem>(handGO);
        grab.isLeftHand = isLeft;
        grab.gripPoint = handGO.transform;
        grab.handCollider = sphereCol;

        string side = isLeft ? "hand_l" : "hand_r";
        Debug.Log($"[Setup] {side}: Rigidbody(mass=5) + SphereCollider(r=0.05) + HandGrabSystem");

        return grab;
    }

    static void SetupCrosshair(GameObject playerKB, PlayerKB_GrabController grabController)
    {
        var existingCrosshair = Object.FindFirstObjectByType<DesktopCrosshair>();
        if (existingCrosshair != null)
        {
            grabController.crosshairUI = existingCrosshair.gameObject;
            return;
        }

        var canvasGO = new GameObject("DesktopCrosshairCanvas");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Crosshair Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var crosshairGO = new GameObject("Crosshair");
        crosshairGO.transform.SetParent(canvasGO.transform, false);
        var rect = crosshairGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(4f, 4f);
        rect.anchoredPosition = Vector2.zero;

        var image = crosshairGO.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.5f);

        var crosshair = crosshairGO.AddComponent<DesktopCrosshair>();
        crosshair.grabController = grabController;

        grabController.crosshairUI = crosshairGO;
        Debug.Log("[Setup] Created crosshair Canvas + Image");
    }

    static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var existing = go.GetComponent<T>();
        if (existing != null) return existing;
        return go.AddComponent<T>();
    }

    static Transform FindChild(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            var found = FindChild(child, name);
            if (found != null) return found;
        }
        return null;
    }
}

#endif
