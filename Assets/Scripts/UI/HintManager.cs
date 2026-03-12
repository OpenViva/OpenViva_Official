using TMPro;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] _hintTexts;

    public void CreateHint(string hintType)
    {
        // If no text found for this type, do nothing
        if (string.IsNullOrEmpty(hintType))
        {
            Debug.LogWarning($"No text found for HintType: {hintType}");
            return;
        }
        foreach (TextMeshProUGUI element in _hintTexts)
        {
            if (string.IsNullOrEmpty(element.text))
            {
                element.text = hintType;
                Debug.Log($"Added hint text '{hintType}' to UI element.");
                return;
            }
        }
    }

    public void ClearHint(string hintType)
    {
        // If no text found for this type, do nothing
        if (string.IsNullOrEmpty(hintType))
        {
            Debug.LogWarning($"No text found for HintType: {hintType}");
            return;
        }

        // Ensure _hintTexts is initialized (add your initialization logic if not already set)
        if (_hintTexts == null || _hintTexts.Length == 0)
        {
            Debug.LogWarning("_hintTexts array is not initialized.");
            return;
        }

        // Iterate through the array to find and clear the matching text
        foreach (TextMeshProUGUI element in _hintTexts)
        {
            if (element.text == hintType)
            {
                element.text = "";
                Debug.Log($"Removed hint text '{hintType}' from UI element.");
                //return;  // Exit after clearing the first match (assuming unique texts)
            }
        }

        // If no match found, do nothing
        //Debug.Log($"No matching UI element found for HintType: {hintType}. No changes made.");
    }
}
