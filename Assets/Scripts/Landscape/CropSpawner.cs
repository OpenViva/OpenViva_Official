using System;
using System.Collections;
using UnityEngine;

public class CropSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _cropToSpawn;
    private GameObject _currentCrop;

    private float _timer = 0f;
    public float GrowTimer = 480f; // Should this depend on Day/Night cycle speed?

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

    private void CropPicked()
    {
        StartCoroutine(StartTimer());
    }

    private IEnumerator StartTimer()
    {
        AssignMethod(false);
        yield return new WaitForSeconds(GrowTimer);
        SpawnCrop();
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
        _currentCrop.transform.SetParent(transform);
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
                crop.OnIsGrabbed += CropPicked;
            }
            else
            {
                crop.OnIsGrabbed -= CropPicked;
            }
        }
        else
        {
            Debug.LogWarning($"Game Object '{_currentCrop.name}' does not have a Crop script");
        }
    }
}
