using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropTower : MonoBehaviour
{
    [SerializeField] private GameObject _peachCrop;
    public int towerHeight = 200;

    private void Start()
    {
        if (_peachCrop.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
        for (int i = 0; i < towerHeight; i++)
        {
            GameObject crop = Instantiate(_peachCrop, new Vector3(transform.position.x, transform.position.y + (i*0.05f), transform.position.z), Quaternion.identity);
        }
    }
}
