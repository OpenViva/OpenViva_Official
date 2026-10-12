using UnityEngine;

public class AbyssTeleporter : MonoBehaviour
{
    [Header("Teleport Point")]
    [SerializeField] private Transform teleportTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CharacterController controller))
        {
            controller.enabled = false;
            other.transform.position = teleportTarget.position;
            other.transform.rotation = teleportTarget.rotation;
            controller.enabled = true;
            return;
        }

        if (other.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        other.transform.position = teleportTarget.position;
        other.transform.rotation = teleportTarget.rotation;
    }
}
