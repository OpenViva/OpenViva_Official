using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The distance to move in each direction")]
    public Vector3 offsetCoords; // TODO: Change this to move to mouse look position

    [Header("Raycast Settings")]
    public float maxRayDistance = 1000f;
    public LayerMask groundLayer = ~0;

    [Header("NavMesh Sampling")]
    [Tooltip("How far to search for a valid NavMesh point")]
    public float maxSampleDistance = 5f;
    public int navMeshAreaMask = NavMesh.AllAreas;

    [Header("Debug")]
    public Vector3 startingCoords;
    [SerializeField] private Camera _cam;

    private NavMeshAgent _agent;
    private Animator _animator;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        _cam = Camera.main;

        if (_cam == null)
            Debug.LogError("No Main Camera found! Make sure your camera is tagged as 'MainCamera'.");
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

    private void OnEnable()
    {
        PlayerManager.OnMoveCharaToCamera += MoveAgentToCameraLookPoint;
    }

    private void OnDestroy()
    {
        PlayerManager.OnMoveCharaToCamera -= MoveAgentToCameraLookPoint;
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

    private void MoveAgentToCameraLookPoint()
    {
        _cam = GetActiveMainCamera();

        if (_cam == null) return;

        // Ray from camera center in the direction its facing
        Ray ray = new(_cam.transform.position, _cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, groundLayer))
        {
            Vector3 targetPoint = hit.point;

            // Find the nearest valid point on the NavMesh
            if (NavMesh.SamplePosition(targetPoint, out NavMeshHit navHit, maxSampleDistance, navMeshAreaMask))
            {
                _agent.SetDestination(navHit.position);
                Debug.DrawLine(targetPoint, navHit.position, Color.green, 2f); // Visual feedback
            }
            else
            {
                Debug.LogWarning("No NavMesh point found near the raycast hit. Increase maxSampleDistance or check your NavMesh bake!");

                // Try the raw hit point anyway
                _agent.SetDestination(targetPoint);
            }
        }
        else
        {
            Debug.Log("Raycast did not hit anything on the selected layers!");
        }
    }

    private Camera GetActiveMainCamera()
    {
        Camera cam = Camera.main;
        if (cam != null && cam.isActiveAndEnabled)
            return cam;

        foreach (Camera c in Camera.allCameras)
        {
            if (c.CompareTag("MainCamera") && c.isActiveAndEnabled)
                return c;
        }
        return null;
    }
}
