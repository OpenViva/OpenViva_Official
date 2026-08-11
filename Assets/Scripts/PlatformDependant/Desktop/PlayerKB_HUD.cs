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
        /*
         * Got these values using trail and error.
         * DO NOT CHANGE FOR THE LOVE OF ALL THAT IS HOLY
         * 0.171f; 17.78; 0.16; -11.44
         */
        gps.GetComponent<RectTransform>().localPosition = new Vector3((posX * 0.171f) + 17.78f, (posZ * 0.16f) + -11.44f, 0);
    }
}
