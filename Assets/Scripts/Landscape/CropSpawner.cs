using System;
using UnityEngine;

public class CropSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _cropToSpawn;
    private GameObject _currentCrop;

    private void Awake()
    {
        try
        {
            _currentCrop = transform.GetChild(0).gameObject;
        }
        catch (Exception spawnerNoCrop)
        {
            Debug.LogWarning($"Crop Spawner was not given crop at runtime.");
        }

    }

    private void OnEnable()
    {
        AssignMethod(true);
    }

    private void SpawnCrop()
    {
        _currentCrop = ObjectPool.instance.GetObject(_cropToSpawn);
        AssignMethod(true);
        if (_currentCrop.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
        _currentCrop.transform.position = transform.position;
        _currentCrop.transform.localScale = Vector3.zero;
        _currentCrop.transform.SetParent(transform);
        _currentCrop.transform.localRotation = Quaternion.identity;
        _currentCrop.gameObject.name = _cropToSpawn.name;
    }

    private void OnDisable()
    {
        AssignMethod(false);
    }

    private void AssignMethod(bool value)
    {
        if (_currentCrop.TryGetComponent(out Crop crop))
        {
            if (value)
            {
                crop.OnIsGrabbed += SpawnCrop;
            }
            else
            {
                crop.OnIsGrabbed -= SpawnCrop;
            }
        }
        else
        {
            Debug.LogWarning($"Game Object '{_currentCrop.name}' does not have a Crop script");
        }
    }
}
