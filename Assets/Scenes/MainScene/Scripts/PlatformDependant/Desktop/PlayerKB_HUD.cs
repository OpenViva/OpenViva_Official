#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Handles the player's HUD for KB&M

public class PlayerKB_HUD : MonoBehaviour
{
    [SerializeField] private GameObject player; // Track the player's position and rotation
    [SerializeField] private GameObject gps; // The GPS UI element

    [SerializeField] private TextMeshProUGUI[] _hintTexts = new TextMeshProUGUI[6];
    [SerializeField] private List<Hint> _hintTypes = new List<Hint>();

    private void Start()
    {
        foreach (Hint.HintType type in Enum.GetValues(typeof(Hint.HintType)))
        {
            Hint newHint = new()
            {
                Type = type,
                hintText = GetTextForHintType(type)
            };
            _hintTypes.Add(newHint);
        }
    }

    void Update()
    {
        float bearing = player.transform.eulerAngles.y; // Get the player's bearing
        gps.transform.localRotation = Quaternion.Euler(0, 0, -bearing); // Rotate the GPS to match the player's bearing

        // Update the GPS position based on the player's position
        float posX = player.transform.position.x;
        float posZ = player.transform.position.z;
        gps.GetComponent<RectTransform>().localPosition = new Vector3(posX/(873.1f/102.8f) + 12.90f, posZ/(584.7f/65.2f) - 7.78f, 0); // Used maths for this. Based on map scale.
    }

    private string GetTextForHintType(Hint.HintType type)
    {
        return type switch
        {
            Hint.HintType.GrabHint => "[LMB] or [RMB]: Grab",
            Hint.HintType.LeftReleaseHint => "[LMB]: Drop",
            Hint.HintType.RightReleaseHint => "[RMB]: Drop",
            Hint.HintType.FlashlightHint => "[E]: Toggle Flashlight",
            Hint.HintType.InteractHint => "[LMB] or [RMB]: Interact",
            Hint.HintType.OpenBagHint => "[Q]: Open Bag",
            Hint.HintType.CloseBagHint => "[Q]: Close Bag",
            Hint.HintType.SelectItemsHint => "[scrollwheel]: Select Items",
            Hint.HintType.TakeItemHint => "[E]: Take Item",
            Hint.HintType.LeftPlaceItemHint => "[LMB]: Place Item",
            Hint.HintType.RightPlaceItemHint => "[RMB]: Place Item",
            _ => "",
        };
    }

    // Control Hints
    public void CreateHint(Hint.HintType hintType)
    {
        string hintText = GetTextForHintType(hintType);

        // If no text found for this type, do nothing
        if (string.IsNullOrEmpty(hintText))
        {
            Debug.LogWarning($"No text found for HintType: {hintType}");
            return;
        }

        foreach (TextMeshProUGUI element in _hintTexts)
        {
            if (string.IsNullOrEmpty(element.text))
            {
                element.text = hintText;
                Debug.Log($"Added hint text '{hintText}' to UI element.");
                return;
            }
        }
    }

    public void ClearHint(Hint.HintType hintType)
    {
        string hintText = GetTextForHintType(hintType);

        // If no text found for this type, do nothing
        if (string.IsNullOrEmpty(hintText))
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
            if (element.text == hintText)
            {
                element.text = "";
                Debug.Log($"Removed hint text '{hintText}' from UI element.");
                return;  // Exit after clearing the first match (assuming unique texts)
            }
        }

        // If no match found, do nothing
        Debug.Log($"No matching UI element found for HintType: {hintType}. No changes made.");
    }

    private void RemoveDuplicates()
    {
        // TO DO
    }
}

#endif