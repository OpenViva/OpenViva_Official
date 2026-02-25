using TMPro;
using UnityEngine;

public class PlayerVR_HUD : MonoBehaviour
{
    public static PlayerVR_HUD Instance;

    private TextMeshProUGUI _leftControlHint;
    private TextMeshProUGUI _rightControlHint;

    [SerializeField] private Camera _camera;

    private void Start()
    {
        _leftControlHint = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _rightControlHint = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        _leftControlHint.enabled = false;
        _rightControlHint.enabled = false;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward, _camera.transform.rotation * Vector3.up);
    }

    public void WarpToObject (Transform location, int hand)
    {
        transform.position = location.position;
        transform.rotation = location.rotation;

        if (hand == 0)
        {
            _leftControlHint.enabled = true;
            _rightControlHint.enabled = false;
        }
        else if (hand == 1)
        {
            _leftControlHint.enabled = false;
            _rightControlHint.enabled = true;
        }
    }

    public void WarpToOrigin()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        _leftControlHint.enabled = false;
        _rightControlHint.enabled = false;
    }
}
