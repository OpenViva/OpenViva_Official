using System;
using UnityEngine;

namespace Viva.Interaction
{
    public class CharacterSelectionManager : MonoBehaviour
    {
        public static CharacterSelectionManager Instance { get; private set; }

        public CharacterInteractable SelectedCharacter { get; private set; }

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
            if (SelectedCharacter == character) return;

            DeselectCharacter();

            SelectedCharacter = character;
            SelectedCharacter.SetSelectedVisual(true);

            OnCharacterSelected?.Invoke(SelectedCharacter);
            Debug.Log($"[CharacterSelection] Selected: {character.gameObject.name}");
        }

        public void DeselectCharacter()
        {
            if (SelectedCharacter != null)
            {
                SelectedCharacter.SetSelectedVisual(false);
                SelectedCharacter = null;

                OnCharacterDeselected?.Invoke();
                Debug.Log("[CharacterSelection] Character deselected.");
            }
        }

        public void IssueCommandToSelected(string commandID)
        {
            if (SelectedCharacter == null)
            {
                Debug.LogWarning("[CharacterSelection] Cannot issue command: No character selected.");
                return;
            }

            SelectedCharacter.ReceiveCommand(commandID);
        }
    }
}