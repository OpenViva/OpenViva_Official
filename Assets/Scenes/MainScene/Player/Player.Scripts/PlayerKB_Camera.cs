using UnityEngine;

public class PlayerKB_Camera : MonoBehaviour
{
    // This class is responsible for the camera movement of the player using mouse.
    // Code by Edenity on Unity Asset Store

    // FIELDS
    [Range(1f, 10f)]
    [Tooltip("speed of the camera movement")]
    [SerializeField] private float _mouseSensity = 1;

    // mouse rotation
    private float _xRotation;
    private float _yRotation;

    [Tooltip("the parent of this object")]
    [SerializeField] private Transform _controller;

    // PROPERTIES

    // Start is called before the first frame update
    private void Start()
    {
        // locks cursor and makes it invisible
        Cursor.lockState = Cursor.lockState;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // get input of the mouse 
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensity;

        _yRotation += mouseX;
        _xRotation -= mouseY;

        // limits camera rotation
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        // rotates camera on the y- and x-axis
        transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);

        // rotates the controller on the y-axis so that it is on the same rotation as the camera
        _controller.rotation = Quaternion.Euler(0, _yRotation, 0);
    }
}
