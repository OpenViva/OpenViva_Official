#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Handles the player's HUD for KB&M

public class PlayerKB_HUD : MonoBehaviour
{
    [SerializeField] private GameObject player; // Track the player's position and rotation
    [SerializeField] private GameObject gps; // The GPS UI element

    // Control Hints
    // [SerializeField] private List<TextMeshProUGUI> _hintTexts = new List<TextMeshProUGUI>(); // The UI Text elements for displaying control hints
    [SerializeField] private TextMeshProUGUI[] _hintTexts = new TextMeshProUGUI[6];
    private List<string> _controlHints = new List<string>(); // The internal list of strings for control hints
    private int _nextFreeIndex = 0;

    private void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            _controlHints.Add("");
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

        int i = 0;
        foreach (TextMeshProUGUI hint in _hintTexts)
        {
            hint.text = _controlHints[i];
            i++;
        }
    }

    public void CreateHint(string text)
    {
        if (_nextFreeIndex < 6)
        {
            _controlHints[_nextFreeIndex] = text;
            _nextFreeIndex++;
        }
    }

    public void ClearHint(string text)
    {
        _controlHints.Remove(text);
        _controlHints.Add("");
        if (_nextFreeIndex > 0) 
        { 
            _nextFreeIndex--; 
        }
    }

    private void RemoveDuplicates()
    {
        // TO DO
    }
}

#endif