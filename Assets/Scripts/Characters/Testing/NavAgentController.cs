using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentController : MonoBehaviour
{
    public Vector3 coords; // Change this to move to mouse look position

    private NavMeshAgent agent;
    private Animator animator;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Update the "VelocityX" animator parameter while moving
        if (animator != null && agent != null)
        {
            float velocityX = agent.velocity.magnitude > 0.1f ? 1f : 0f;
            animator.SetFloat("VelocityX", velocityX);
        }
    }

    [Button("Move Agent To Center", EButtonEnableMode.Playmode)]
    public void MoveToCenter()
    {
        agent.SetDestination(Vector3.zero);
    }

    [Button("Move Agent To Coords", EButtonEnableMode.Playmode)]
    public void MoveToCoords()
    {
        agent.SetDestination(coords);
    }
}
