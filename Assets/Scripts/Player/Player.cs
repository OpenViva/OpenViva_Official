using UnityEngine;

public class Player : MonoBehaviour
{
    // PS Viva player controls
    public PlayerControls Controls {  get; private set; }

    private void Awake()
    {
        Controls = new PlayerControls();
    }

    private void OnEnable()
    {
        Controls.Enable();
    }

    private void OnDisable()
    {
        Controls.Disable();
    }
}
