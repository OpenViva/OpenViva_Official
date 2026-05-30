using UnityEngine;

public class PestleGrind : MonoBehaviour
{
    private Player _player;
    private PlayerKB_GrabObject _grabScript;
    public bool IsGrinding = false;
    private MortarLogic _currentMortar;

    private void Start()
    {
        _player = FindFirstObjectByType<Player>();

        _grabScript = GetComponent<PlayerKB_GrabObject>();
        AssignInputs();
    }

    private void Update()
    {
        if (IsGrinding && _currentMortar != null)
        {
            _currentMortar.GrindWheat();
        }
    }

    private void GrindLeft()
    {
        if (_grabScript == null || _grabScript.GetIsGrabbed() != 2) { return; }

        GameObject objectInLeft = PlayerManager.Instance.GetObjectLeft();
        if (objectInLeft.name.Equals("mortar") && objectInLeft.TryGetComponent(out MortarLogic mortar))
        {
            IsGrinding = true;
            _currentMortar = mortar;
        }
    }

    private void GrindRight()
    {
        if (_grabScript == null || _grabScript.GetIsGrabbed() != 1) { return; }


        if (PlayerManager.Instance.GetItemRight() == 15)
        {
            IsGrinding = true;
        }
    }

    private void StopGrinding()
    {
        IsGrinding = false;
    }

    private void AssignInputs()
    {
        _player.Controls.Viva.InteractLeft.performed += context => GrindLeft();
        _player.Controls.Viva.InteractRight.performed += context => GrindRight();
        _player.Controls.Viva.InteractLeft.canceled += context => StopGrinding();
        _player.Controls.Viva.InteractRight.canceled += context => StopGrinding();
    }

    //private void OnDisable()
    //{
    //    _player.Controls.Viva.InteractLeftHold.performed -= context => GrindLeft();
    //    _player.Controls.Viva.InteractRightHold.performed -= context => GrindRight();
    //}
}
