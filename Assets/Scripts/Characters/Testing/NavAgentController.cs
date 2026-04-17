using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The distance to move in each direction")]
    public Vector3 offsetCoords; // TODO: Change this to move to mouse look position

    [Header("Debug")]
    public Vector3 startingCoords;

    private NavMeshAgent _agent;
    private Animator _animator;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Update the "VelocityX" animator parameter while moving
        if (_animator != null && _agent != null)
        {
            float velocityX = _agent.velocity.magnitude > 0.1f ? 1f : 0f;
            _animator.SetFloat("VelocityX", velocityX);
        }
    }

    [Button("Move To Start Position", EButtonEnableMode.Playmode)]
    public void MoveToStart()
    {
        _agent.SetDestination(startingCoords);
    }

    [Button("Move To Distance", EButtonEnableMode.Playmode)]
    public void MoveToCoords()
    {
        _agent.SetDestination(startingCoords + offsetCoords);
    }
}
