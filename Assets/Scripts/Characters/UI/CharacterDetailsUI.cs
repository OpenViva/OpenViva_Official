using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Viva.Interaction;

public class CharacterDetailsUI : MonoBehaviour
{
    [Header("Details Panels")]
    [SerializeField] private GameObject leftDetailsPanel;
    [SerializeField] private GameObject rightDetailsPanel;
    [SerializeField] private GameObject leftRootPage;
    [SerializeField] private GameObject rightRootPage;

    [SerializeField] private TMP_Text nameElement;

    [SerializeField] private Button deselectButton;
    [SerializeField] private Button nameButton;

    private void Start()
    {
        if (deselectButton != null) deselectButton.onClick.AddListener(OnDeselectButtonClicked);
        if (nameButton != null) nameButton.onClick.AddListener(OpenDetailsPages);

        if (CharacterSelectionManager.Instance != null)
        {
            CharacterSelectionManager.Instance.OnCharacterSelected += DisplaySelectedCharacter;
            CharacterSelectionManager.Instance.OnCharacterDeselected += HideDetailsPages;
        }

        if (CharacterSelectionManager.Instance != null && CharacterSelectionManager.Instance.SelectedCharacter != null)
        {
            DisplaySelectedCharacter(CharacterSelectionManager.Instance.SelectedCharacter);
        }

        // Ensure there is text before selecting first char
        nameElement.text = "Not selected...";

        HideDetailsPages();
    }

    private void OnDestroy()
    {
        if (CharacterSelectionManager.Instance != null)
        {
            CharacterSelectionManager.Instance.OnCharacterSelected -= DisplaySelectedCharacter;
            CharacterSelectionManager.Instance.OnCharacterDeselected -= HideDetailsPages;
        }

        if (deselectButton != null) deselectButton.onClick.RemoveListener(OnDeselectButtonClicked);
        if (nameButton != null) nameButton.onClick.RemoveListener(OpenDetailsPages);
    }

    private void DisplaySelectedCharacter(CharacterInteractable character)
    {
        nameElement.text = character.characterName;
    }

    private void OnDeselectButtonClicked()
    {
        if (CharacterSelectionManager.Instance != null)
        {
            CharacterSelectionManager.Instance.DeselectCharacter();
        }
    }

    private void OpenDetailsPages()
    {
        if (leftDetailsPanel != null) leftDetailsPanel.SetActive(true);
        if (rightDetailsPanel != null) rightDetailsPanel.SetActive(true);

        if (leftRootPage != null) leftRootPage.SetActive(false);
        if (rightRootPage != null) rightRootPage.SetActive(false);

        // TODO: Fill details page
    }

    private void HideDetailsPages()
    {
        if (leftDetailsPanel != null) leftDetailsPanel.SetActive(false);
        if (rightDetailsPanel != null) rightDetailsPanel.SetActive(false);
    }
}
