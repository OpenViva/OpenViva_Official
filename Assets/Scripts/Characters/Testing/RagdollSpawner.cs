using FIMSpace.FProceduralAnimation;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RagdollSpawner : MonoBehaviour
{
    [Header("Prefab to Spawn")]
    public GameObject characterPrefab;

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

    private Vector3 _startingCoords;
    List<GameObject> rootBoneObjects;

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

    [Button("Instantiate First Char", EButtonEnableMode.Playmode)]
    public void SpawnFirstLoaded()
    {
        // TODO: Get the whole list and make a method to set up and spawn a single character at a specific location
        CharacterAssembler.CharacterModel model = _characterReader.GetFirstModel();
        characterPrefab = model.prefab;

        loadedCharacters.Add(model.prefab);
        //Instantiate(model.prefab, _startingCoords, transform.rotation.normalized);
    }

    [Button("Spawn & Setup Ragdoll", EButtonEnableMode.Playmode)]
    public void SpawnAndSetupRagdoll()
    {
        if (characterPrefab == null) return;

        GameObject instance = SpawnCharacter(characterPrefab);

        // TODO: Collect bones and add them here before settting up the model!!!

        // Find root bones for cloth physics if script is present
        rootBoneObjects = instance.GetComponent<RootBonesHolder>().rootBoneObjects;

        // 1. Add the component
        RagdollAnimator2 ragdoll = instance.AddComponent<RagdollAnimator2>();

        // 2. Set BaseTransform BEFORE setup!
        ragdoll.Settings.BaseTransform = instance.transform;

        // 3. Auto-detect bones and generate colliders, rigidbodies, joints, dummy, etc.
        ragdoll.TryFindBonesAndDoFullSetup();

        // 4. Add and configure NavMeshAgent
        if (!instance.TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent = instance.AddComponent<NavMeshAgent>();
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
        if (!instance.TryGetComponent(out RagdollController _))
        {
            instance.AddComponent<RagdollController>();
        }

        if (!instance.TryGetComponent(out NavAgentController navAgentController))
        {
            navAgentController = instance.AddComponent<NavAgentController>();
        }
        navAgentController.offsetCoords = moveCoords;
        navAgentController.startingCoords = _startingCoords;

        // 6. Enable and set initial state
        ragdoll.enabled = true;
        ragdoll.Settings.Initialize(ragdoll, instance); // Prevents some runtime exceptions
        ragdoll.User_SwitchFallState(RagdollHandler.EAnimatingMode.Standing);

        // 7. Set up cloth physics
        if (rootBoneObjects.Count != 0)
        {
            // TODO: Use PhysicsBone parameters instead of default values here
            _physicsAttacher.CreateBoneCloth(instance, rootBoneObjects, "Hair_BoneCloth");
        }
        else
        {
            Debug.LogError("Root bones List for cloth physics is empty or missing on character!");
        }

        Debug.Log($"Ragdoll fully auto-setup on {instance.name}");
    }
}
