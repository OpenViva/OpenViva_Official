using UnityEngine;
using Viva.Interaction;

public class CharacterInteractable : MonoBehaviour, Viva.Interaction.IInteractable
{
    [Header("Selection Visuals")]
    [SerializeField] private GameObject selectionArrow;

    // Pointer always interacts with characters (never grabs them)
    public InteractionType CurrentType => InteractionType.Interact;

    private void Start()
    {
        if (selectionArrow != null)
            selectionArrow.SetActive(false);
    }

    public bool ExecuteAction(GameObject interactor, HandSide hand, Transform handTransform)
    {
        if (CharacterSelectionManager.Instance != null)
        {
            CharacterSelectionManager.Instance.SelectCharacter(this);
            return true;
        }
        return false;
    }

    public void OnDropped()
    {

    }

    public void SetSelectedVisual(bool isSelected)
    {
        if (selectionArrow != null)
        {
            selectionArrow.SetActive(isSelected);
        }
    }

    public void ReceiveCommand(string commandID)
    {
        Debug.Log($"{gameObject.name} received command: {commandID}");

        // TODO: Route these commands to the character manager
        switch (commandID)
        {
            case "Follow":
                // navAgent.SetDestination(player.transform.position);
                break;
            case "Wave":
                // animator.SetTrigger("Wave");
                break;
        }
    }
}
