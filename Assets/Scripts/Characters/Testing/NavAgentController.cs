using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentController : MonoBehaviour
{
    private static readonly int VelocityXHash = Animator.StringToHash("VelocityX");
    [Header("Settings")]
    [Tooltip("The distance to move in each direction")]
    public Vector3 offsetCoords; // TODO: Change this to move to mouse look position
    public float turnSpeed = 5;

    [Header("Follow Settings")]
    [Tooltip("The Transform this agent should follow")]
    public Transform target;
    [Tooltip("When true, follow the target. When false, idle")]
    public bool isFollowing = false;
    [Tooltip("How often (in seconds) the agent checks the target's position")]
    public float updateInterval = 0.2f;
    [Tooltip("How far the target must move before a new path is calculated")]
    public float movementThreshold = 0.5f;

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
    private Vector3[] _cornerBuffer = new Vector3[64];

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

    void FixedUpdate()
    {
        // Update the "VelocityX" animator parameter while moving
        if (_animator != null && _agent != null)
        {
            float velocityX = _agent.velocity.magnitude > 0.1f ? 1f : 0f;
            _animator.SetFloat(VelocityXHash, velocityX);

            if (velocityX > 0.1)
            {
                FaceTarget(GetNextPathpoint());
            }
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

    protected Vector3 GetNextPathpoint()
    {
        NavMeshPath path = _agent.path;

        int cornerCount = path.GetCornersNonAlloc(_cornerBuffer);

        if (cornerCount < 2)
        {
            return _agent.destination;
        }

        for (int i = 0; i < cornerCount; i++)
        {
            if (Vector3.Distance(_agent.transform.position, _cornerBuffer[i]) < 1)
            {
                if (i + 1 < cornerCount)
                {
                    return _cornerBuffer[i + 1];
                }

                break;
            }
        }

        return _agent.destination;
    }

    public void FaceTarget(Vector3 target)
    {
        Quaternion targetRotation = Quaternion.LookRotation(target - transform.position);

        Vector3 currentEulerAngles = transform.rotation.eulerAngles;

        float yRotation = Mathf.LerpAngle(currentEulerAngles.y, targetRotation.eulerAngles.y, turnSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(currentEulerAngles.x, yRotation, currentEulerAngles.z);
    }

    private Camera GetActiveMainCamera()
    {
        Camera cam = Camera.main;
        if (cam != null && cam.isActiveAndEnabled)
            return cam;

        foreach (Camera c in Camera.allCameras)
        {
            if (c.CompareTag(TagConstants.MainCamera) && c.isActiveAndEnabled)
                return c;
        }
        return null;
    }
}
