using UnityEngine;

public class CropPositions : MonoBehaviour
{
    private int currentPosition = 0;

    private Vector3[] cantaloupePositions = new Vector3[12]
    {
        new Vector3(271.799f, 83.795f, 36.26f),
        new Vector3(272.801f, 83.795f, 36.325f),
        new Vector3(273.125f, 83.795f, 35.152f),
        new Vector3(272.455f, 83.795f, 34.922f),
        new Vector3(276.554f, 83.857f, 34.8203f),
        new Vector3(277.727f, 84.0229f, 35.1363f),
        new Vector3(278.215f, 83.883f, 34.104f),
        new Vector3(277.244f, 83.8356f, 33.5438f),
        new Vector3(270.591f, 83.917f, 37.942f),
        new Vector3(268.871f, 84.068f, 38.455f),
        new Vector3(269.209f, 84.059f, 38.98f),
        new Vector3(270.22f, 84.059f, 39.141f)
    };

    // To do: Add more crop types here

    public Vector3[] getPositions(CropTypes.CropType cropType)
    {
        switch (cropType)
        {
            case CropTypes.CropType.Cantaloupe:
                return cantaloupePositions;
            // To do: Add more crop types here
            default:
                return null;
        }
    }

    public int getCurrentPosition()
    {
        return currentPosition;
    }

    public void setCurrentPosition()
    {
        currentPosition++;
    }
}
