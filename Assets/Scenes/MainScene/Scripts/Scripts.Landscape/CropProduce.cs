using System;
using UnityEditor;
using UnityEngine;

public class CropProduce : MonoBehaviour
{
    // This class represents a crop
    // Code by Saien

    private CropTypes.CropType cropType;
    [SerializeField] private int position;
    private bool isPicked = false;

    public CropProduce(CropTypes.CropType cropType, int position)
    {
        this.cropType = cropType;
        this.position = position;
    }

    public void setIsPicked()
    {
        isPicked = true;
    }

    public GameObject getGameObject()
    {
        return gameObject;
    }

    public void inBowl()
    {
        Destroy(gameObject);
    }
}
