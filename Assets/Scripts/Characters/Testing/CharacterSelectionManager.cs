using System;
using UnityEngine;

namespace Viva.Interaction
{
    public class CharacterSelectionManager : MonoBehaviour
    {
        public static CharacterSelectionManager Instance { get; private set; }

        [SerializeField] private CharacterInteractable selectedCharacter;
        public CharacterInteractable SelectedCharacter => selectedCharacter;

        public event Action<CharacterInteractable> OnCharacterSelected;
        public event Action OnCharacterDeselected;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SelectCharacter(CharacterInteractable character)
        {
            if (selectedCharacter == character)
            {
                DeselectCharacter();
                return;
            }

            DeselectCharacter();

            selectedCharacter = character;
            selectedCharacter.SetSelectedVisual(true);

            OnCharacterSelected?.Invoke(selectedCharacter);
            Debug.Log($"[CharacterSelection] Selected: {character.gameObject.name}");
        }

        public void DeselectCharacter()
        {
            if (selectedCharacter != null)
            {
                selectedCharacter.SetSelectedVisual(false);
                selectedCharacter = null;

                OnCharacterDeselected?.Invoke();
                Debug.Log("[CharacterSelection] Character deselected.");
            }
        }

        public void IssueCommandToSelected(string commandID)
        {
            if (selectedCharacter == null)
            {
                Debug.LogWarning("[CharacterSelection] Cannot issue command: No character selected.");
                return;
            }

            selectedCharacter.ReceiveCommand(commandID);
        }
    }
}