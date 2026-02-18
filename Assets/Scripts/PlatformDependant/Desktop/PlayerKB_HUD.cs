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

    void Update()
    {
        float bearing = player.transform.eulerAngles.y; // Get the player's bearing
        gps.transform.localRotation = Quaternion.Euler(0, 0, -bearing); // Rotate the GPS to match the player's bearing

        // Update the GPS position based on the player's position
        float posX = player.transform.position.x;
        float posZ = player.transform.position.z;
        gps.GetComponent<RectTransform>().localPosition = new Vector3(posX/(873.1f/102.8f) + 12.90f, posZ/(584.7f/65.2f) - 7.78f, 0); // Used maths for this. Based on map scale.
    }

    // Control Hints
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
        Debug.Log($"No matching UI element found for HintType: {hintType}. No changes made.");
    }

    private void RemoveDuplicates()
    {
        // TO DO
    }
}

#endif