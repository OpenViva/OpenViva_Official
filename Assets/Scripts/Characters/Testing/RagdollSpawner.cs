using FIMSpace.FProceduralAnimation;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterAssembler))]
[RequireComponent(typeof(PhysicsAttacher))]
public class RagdollSpawner : MonoBehaviour
{
    [Header("Prefab to Spawn")]
    public GameObject characterPrefab;

    [Header("Animation Settings")]
    public RuntimeAnimatorController animationControllerName;

    [Header("NavMeshAgent Settings")]
    public float agentSpeed = 1.5f;
    public float agentRadius = 0.3f;
    public float agentHeight = 1.4f;

    [Header("Nav Controller Settings")]
    public Vector3 moveCoords = new(0, 0, -3); // TODO: Do NOT initialize coords but get them from the spawner object (ex: mirror)

    [Header("Debug")]
    [SerializeField] private PhysicsAttacher _physicsAttacher;
    [SerializeField] private CharacterAssembler _characterReader;
    public List<GameObject> loadedCharacters;
    public RagdollAnimatorFeatureHelper kinematicFeetSwitcher;

    private Vector3 _startingCoords;
    List<GameObject> rootBoneObjects; // DEPRECATED
    List<PhysicsBoneData> physicsBoneDataList;

    private void Start()
    {
        _startingCoords = transform.position;

        _physicsAttacher = GetComponent<PhysicsAttacher>();

        _characterReader = GetComponent<CharacterAssembler>();

        if (_characterReader != null)
        {
            _characterReader.ReadAllCharacters();
        }

        List<CharacterAssembler.CharacterModel> loadedModels = _characterReader.GetAllModels();

        foreach (var model in loadedModels)
        {
            loadedCharacters.Add(model.prefab);
        }
    }

    GameObject SpawnCharacter(GameObject prefabToSpawn)
    {
        GameObject instance = Instantiate(prefabToSpawn, _startingCoords, transform.rotation.normalized);
        instance.name = "RagdollCharacter_" + Time.frameCount;

        return instance;
    }

    [Button("Spawn & Setup Ragdoll", EButtonEnableMode.Playmode)]
    public void SpawnAndSetupRagdoll()
    {
        if (characterPrefab == null) return;

        CharacterAssembler.CharacterModel importedModel = _characterReader.GetFirstModel();
        characterPrefab = importedModel.prefab;
        loadedCharacters.Add(importedModel.prefab);

        GameObject newChar = SpawnCharacter(characterPrefab);

        AssignAnimatorController(newChar, animationControllerName);

        // Find root bones for cloth physics if script is present
        if (importedModel.vivaCharacterData != null)
        {
            physicsBoneDataList = importedModel.vivaCharacterData.PhysicsBones;
        }

        // 1. Add the component
        RagdollAnimator2 ragdoll = newChar.AddComponent<RagdollAnimator2>();

        // 2. Set BaseTransform BEFORE setup!
        ragdoll.Settings.BaseTransform = newChar.transform;

        // 3. Auto-detect bones and generate colliders, rigidbodies, joints, dummy, etc.
        ragdoll.TryFindBonesAndDoFullSetup();

        // 4. Add and configure NavMeshAgent
        if (!newChar.TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent = newChar.AddComponent<NavMeshAgent>();
        }

        agent.speed = agentSpeed;
        agent.radius = agentRadius;
        agent.height = agentHeight;
        agent.baseOffset = 0f;
        agent.angularSpeed = 3600f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.5f;

        // Important for ragdoll compatibility
        // When ragdoll is active (Falling mode), disable NavMeshAgent movement
        agent.updatePosition = true;
        agent.updateRotation = true;

        // 5. Add additional scripts
        if (!newChar.TryGetComponent(out RagdollController _))
        {
            newChar.AddComponent<RagdollController>();
        }

        if (!newChar.TryGetComponent(out NavAgentController navAgentController))
        {
            navAgentController = newChar.AddComponent<NavAgentController>();
        }
        navAgentController.offsetCoords = moveCoords;
        navAgentController.startingCoords = _startingCoords;

        // 6. Enable and set initial state
        ragdoll.enabled = true;

        AddKinematicFeetAtRuntime(ragdoll);

        ragdoll.Settings.Initialize(ragdoll, newChar); // Prevents some runtime exceptions
        ragdoll.User_SwitchFallState(RagdollHandler.EAnimatingMode.Standing);

        // 7. Set up cloth physics
        if (physicsBoneDataList.Count != 0)
        {
            // TODO: Use PhysicsBone parameters instead of default values here
            _physicsAttacher.CreateBoneCloth(newChar, physicsBoneDataList);
        }
        else
        {
            Debug.LogError("Root bones List for cloth physics is empty or missing on character!");
        }

        Debug.Log($"Ragdoll fully auto-setup on {newChar.name}");
    }

    public void AddKinematicFeetAtRuntime(RagdollAnimator2 animator)
    {
        if (animator != null)
        {
            animator.Handler.ExtraFeatures.Add(kinematicFeetSwitcher);

            RagdollAnimatorFeatureHelper kinematicFeature = animator.Handler.ExtraFeatures.Find(x => x == kinematicFeetSwitcher);

            kinematicFeature.Enabled = true;
        }
    }

    private void AssignAnimatorController(GameObject characterRoot, RuntimeAnimatorController animatorController)
    {
        if (animatorController == null)
        {
            Debug.LogWarning("[Chara Loader] No Animator Controller specified.");
            return;
        }

        Animator animator = characterRoot.GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            Debug.LogWarning($"[Chara Loader] No Animator found on character {characterRoot.name}");
            return;
        }

        if (animatorController != null)
        {
            animator.runtimeAnimatorController = animatorController;
            Debug.Log($"[Chara Loader] Assigned Animator Controller: {animatorController}");
        }
        else
        {
            Debug.LogError($"[Chara Loader] Failed to load Animator Controller: {animatorController}");
        }
    }
}
