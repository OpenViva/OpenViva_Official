using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterListItem : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Button button;

    public void SetupButton(string bundleName, string charName, System.Action<string> onClicked)
    {
        nameText.text = charName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClicked(bundleName));
    }

    // Add more items here if needed
}
