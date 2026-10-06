using UnityEngine;
using Viva.Interaction;

public class FlashlightGrabExample : MonoBehaviour, Viva.Interaction.IInteractable
{
    public InteractionType CurrentType { get; private set; } = InteractionType.Grab;

    private Rigidbody _rb;
    private bool isLightOn = false;
    [SerializeField] private Light spotLight;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public bool ExecuteAction(GameObject interactor, HandSide hand, Transform handTransform)
    {
        if (CurrentType == InteractionType.Grab)
        {
            Debug.Log($"Grabbed Flashlight with {hand} hand.");

            // 1. Parent object to the hand (Assume interactor has a way to get hand transforms)
            transform.SetParent(handTransform);
            transform.localPosition = Vector3.zero;

            // 2. Disable physics while held
            _rb.isKinematic = true;
            _rb.useGravity = false;

            // 2. Change layer so SmartTargetDetector stops highlighting it
            gameObject.layer = LayerMask.NameToLayer("Default");

            // 3. Transition State so the next click triggers "Use"
            CurrentType = InteractionType.Use;
            return true;
        }
        else if (CurrentType == InteractionType.Use)
        {
            // Hand is already holding it, input triggers this block
            isLightOn = !isLightOn;
            if (spotLight != null) spotLight.enabled = isLightOn;

            Debug.Log("Flashlight Toggled!");
            return true;
        }

        return false;
    }

    public void OnDropped()
    {
        // 1. Remove the parent so it falls loose in the world
        transform.SetParent(null);

        // 2. Re-enable physics so it drops to the ground
        _rb.isKinematic = false;
        _rb.useGravity = true;

        // 3. Reset layer so it can be targeted and outlined again
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        // 4. Reset state back to Grab
        CurrentType = InteractionType.Grab;
    }
}
