using FIMSpace.FProceduralAnimation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterAssembler))]
[RequireComponent(typeof(PhysicsAttacher))]
public class RagdollSpawner : MonoBehaviour
{
    [Header("Prefab to Spawn")]
    public GameObject characterPrefab;
    public string tagToGive = "Character";
    [Tooltip("Drag the object named Player that will move")]
    public Transform playerFollowObject;
    public Transform spawnLocation;

    [Header("Animation Settings")]
    public RuntimeAnimatorController animationControllerName;

    [Header("Ragdoll Extra Features")]
    public List<RagdollAnimatorFeatureBase> extraFeaturesList;

    [Header("UI Reference")]
    public GameObject listContentObject;
    public GameObject characterItemPrefab;

    [Header("NavMeshAgent Settings")]
    public float agentSpeed = 1.5f;
    public float agentRadius = 0.3f;
    public float agentHeight = 1.4f;
    public float stoppingDistance = 1f;

    [Header("Nav Controller Settings")]
    public Vector3 moveCoords = new(0, 0, -3); // TODO: Do NOT initialize coords but get them from the spawner object (ex: mirror)

    [Header("Debug")]
    [SerializeField] private PhysicsAttacher _physicsAttacher;
    [SerializeField] private CharacterAssembler _characterReader;
    public List<GameObject> loadedCharacters;
    

    private Vector3 _startingCoords;
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

    private void OnEnable()
    {
        Globals.OnBookOpened += FillBookCharacters;
    }

    private void OnDestroy()
    {
        Globals.OnBookOpened -= FillBookCharacters;
    }

    void FillBookCharacters()
    {
        foreach (Transform child in listContentObject.transform)
        {
            Destroy(child.gameObject);
        }

        List<CharacterAssembler.CharacterModel> allModels = _characterReader.GetAllModels();

        
        foreach (var model in allModels)
        {
            GameObject newEntry = Instantiate(characterItemPrefab, listContentObject.transform);

            var item = newEntry.GetComponent<CharacterListItem>();

            item.SetupButton(model.bundleName, model.vivaCharacterData.Info.Name, SpawnAndSetupRagdoll);
        }
    }

    GameObject SpawnCharacter(GameObject prefabToSpawn)
    {
        GameObject instance = Instantiate(prefabToSpawn, spawnLocation.position, spawnLocation.rotation.normalized);
        instance.name = "RagdollCharacter_" + Time.frameCount;

        return instance;
    }

    public void SpawnAndSetupRagdoll(string bundleName)
    {
        if (characterPrefab == null) return;

        CharacterAssembler.CharacterModel importedModel = _characterReader.GetModelByName(bundleName);
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

        // TODO: Either change this to a name or dropdown based variable or make sure "Humanoid" is always the first type
        agent.agentTypeID = 0;
        agent.speed = agentSpeed;
        agent.radius = agentRadius;
        agent.height = agentHeight;
        agent.baseOffset = -0.03f;
        agent.angularSpeed = 3600f;
        agent.acceleration = 8f;
        agent.stoppingDistance = stoppingDistance;

        // Important for ragdoll compatibility
        // When ragdoll is active (Falling mode), disable NavMeshAgent movement
        agent.updatePosition = true;
        agent.updateRotation = false;
        agent.autoBraking = false;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.GoodQualityObstacleAvoidance;

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
        navAgentController.target = playerFollowObject;

        // 6. Enable and set initial state
        ragdoll.enabled = true;

        // Fill in all the selected Extra features
        AddRagdollFeatures(ragdoll);

        ragdoll.Settings.Initialize(ragdoll, newChar); // Prevents some runtime exceptions
        ragdoll.User_SwitchFallState(RagdollHandler.EAnimatingMode.Standing);

        // Preemtively lower the thickness of the automatically created colliders
        ReduceAndUpdateThickness(ragdoll);

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

    public void ReduceAndUpdateThickness(RagdollAnimator2 ragdoll)
    {
        ragdoll.Handler.Chains[0].ChainThicknessMultiplier = 0.6f;
        ragdoll.Handler.Chains[1].ChainThicknessMultiplier = 0.6f;
        ragdoll.Handler.Chains[2].ChainThicknessMultiplier = 0.6f;

        if (ragdoll.Handler.Chains.Count > 3)
        {
            ragdoll.Handler.Chains[3].ChainThicknessMultiplier = 0.6f;
        }
        if (ragdoll.Handler.Chains.Count > 4)
        {
            ragdoll.Handler.Chains[4].ChainThicknessMultiplier = 0.6f;
        }

        ragdoll.Settings.User_UpdateAllBonesParametersAfterManualChanges();
    }

    public void AddRagdollFeatures(RagdollAnimator2 ragdoll)
    {
        if (ragdoll != null && extraFeaturesList.Count > 0)
        {
            foreach (var extraFeature in extraFeaturesList)
            {
                ragdoll.Handler.AddRagdollFeature(extraFeature);
            }
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
