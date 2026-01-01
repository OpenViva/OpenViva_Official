using UnityEngine;

public class ObjectHoldPositions : MonoBehaviour
{
    private Vector3[] allObjectPositionsLeft = new Vector3[2]
    {
        new Vector3(0.0456f, 0.0211f, -0.0027f), // BAG
        new Vector3(0.0133f, 0.0137f, -0.007f) // RUBBER_DUCKY
    };

    private Vector3[] allObjectPositionsRight = new Vector3[2]
    {
        new Vector3(-0.04559939f, 0.02110242f, -0.003600158f), // BAG
        new Vector3(-0.004600528f, 0.00939743f, -0.007199669f) // RUBBER_DUCKY
    };

    private Quaternion[] allObjectRotationsLeft = new Quaternion[2]
    {
        Quaternion.Euler(0f, 90f, 0f),   // BAG
        Quaternion.Euler(-15.865f, 22.223f, 34.353f) // RUBBER_DUCKY
    };

    private Quaternion[] allObjectRotationsRight = new Quaternion[2]
    {
        Quaternion.Euler(0f, -90f, 0f),  // BAG
        Quaternion.Euler(0f, 159.498f, 9.144f) // RUBBER_DUCKY
    };

    public Vector3 GetObjectPositionLeft(int objectIndex)
    {
        return allObjectPositionsLeft[objectIndex];
    }

    public Vector3 GetObjectPositionRight(int objectIndex)
    {
        return allObjectPositionsRight[objectIndex];
    }

    public Quaternion GetObjectRotationLeft(int objectIndex)
    {
        return allObjectRotationsLeft[objectIndex];
    }

    public Quaternion GetObjectRotationRight(int objectIndex)
    {
        return allObjectRotationsRight[objectIndex];
    }
}
