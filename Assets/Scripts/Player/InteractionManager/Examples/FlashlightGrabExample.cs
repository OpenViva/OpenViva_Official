using UnityEngine;
using Viva.Interaction;

public class FlashlightGrabExample : MonoBehaviour, Viva.Interaction.IInteractable
{
    // Starts as a grab target. 
    public InteractionType CurrentType { get; private set; } = InteractionType.Grab;

    private bool isLightOn = false;
    [SerializeField] private Light spotLight;

    public bool ExecuteAction(GameObject interactor, HandSide hand)
    {
        if (CurrentType == InteractionType.Grab)
        {
            Debug.Log($"Grabbed Flashlight with {hand} hand.");

            // 1. Parent object to the hand (Assume interactor has a way to get hand transforms)
            // transform.SetParent(handTransform); 
            // transform.localPosition = Vector3.zero;

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
}
