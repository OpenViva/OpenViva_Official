using FIMSpace.FProceduralAnimation;
using NaughtyAttributes;
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

    [Button("Spawn & Setup Ragdoll", EButtonEnableMode.Playmode)]
    public void SpawnAndSetupRagdoll()
    {
        if (characterPrefab == null) return;

        GameObject instance = Instantiate(characterPrefab);
        instance.name = "RagdollCharacter_" + Time.frameCount;

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
        agent.angularSpeed = 360f;
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
        navAgentController.coords = moveCoords;

        // 6. Enable and set initial state
        ragdoll.enabled = true;
        ragdoll.Settings.Initialize(ragdoll, instance); // Prevents some runtime exceptions
        ragdoll.User_SwitchFallState(RagdollHandler.EAnimatingMode.Standing);

        Debug.Log($"Ragdoll fully auto-setup on {instance.name}");
    }
}
