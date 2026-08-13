using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterListItem : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Button button;

    public void SetupButton(string bundleName, System.Action<string> onClicked)
    {
        nameText.text = bundleName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClicked(bundleName));
    }

    // Add more items here if needed
}
