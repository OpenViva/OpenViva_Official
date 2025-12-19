#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;

// Handles the player's HUD for KB&M

public class PlayerKB_HUD : MonoBehaviour
{
    [SerializeField] private GameObject player; // Track the player's position and rotation
    [SerializeField] private GameObject gps; // The GPS UI element

    void Update()
    {
        float bearing = player.transform.eulerAngles.y; // Get the player's bearing
        gps.transform.localRotation = Quaternion.Euler(0, 0, -bearing); // Rotate the GPS to match the player's bearing

        // Update the GPS position based on the player's position
        float posX = player.transform.position.x;
        float posZ = player.transform.position.z;
        gps.GetComponent<RectTransform>().localPosition = new Vector3(posX/(873.1f/102.8f) + 12.90f, posZ/(584.7f/65.2f) - 7.78f, 0); // Used maths for this. Based on map scale.
    }
}

#endif