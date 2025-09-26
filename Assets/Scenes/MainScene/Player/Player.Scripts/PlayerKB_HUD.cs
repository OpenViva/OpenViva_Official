using UnityEngine;

public class PlayerKB_HUD : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gps;

    void Update()
    {
        float bearing = player.transform.eulerAngles.y;
        gps.transform.localRotation = Quaternion.Euler(0, 0, -bearing);

        float posX = player.transform.position.x;
        float posZ = player.transform.position.z;
        gps.GetComponent<RectTransform>().localPosition = new Vector3(posX/(873.1f/102.8f) + 12.90f, posZ/(584.7f/65.2f) - 7.78f, 0);
    }
}
