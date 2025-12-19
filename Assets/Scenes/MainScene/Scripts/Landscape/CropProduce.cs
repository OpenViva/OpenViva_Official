using System;
using UnityEditor;
using UnityEngine;

public class CropProduce : MonoBehaviour
{
    // FIELDS
    private CropTypes.CropType cropType; // The type of crop
    private int position; // The position of the crop
    private bool isPicked = false; // Whether the crop still in the same place that is was in when it was initialized

    // CONSTRUCTOR
    public CropProduce(CropTypes.CropType cropType, int position)
    {
        this.cropType = cropType;
        this.position = position;
    }

    // GETTERS AND SETTERS
    public void setIsPicked()
    {
        isPicked = true;
    }

    public GameObject getGameObject()
    {
        return gameObject;
    }

    // PROPERTIES
    // When the crop is put in a bowl, destroy the object
    public void inBowl()
    {
        Destroy(gameObject);
    }
}
