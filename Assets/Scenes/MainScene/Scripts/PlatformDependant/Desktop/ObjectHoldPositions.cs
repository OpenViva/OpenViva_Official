using UnityEngine;

public class ObjectHoldPositions : MonoBehaviour
{
    private Vector3[] allObjectPositionsLeft = new Vector3[8]
    {
        new Vector3(0.0456f, 0.0211f, -0.0027f), // BAG
        new Vector3(0.0133f, 0.0137f, -0.007f), // RUBBER_DUCKY
        new Vector3(0.0025f,0.0158f, -0.0064f), // PEACH
        new Vector3(0.00077f, 0.03047f, -0.0053f), // STRAWBERRY
        new Vector3(0.0099f, 0.0175f, -0.008f), // CANTALOUPE
        new Vector3(-0.00514069f, 0.03108077f, -0.0057312f), // BLUEBERRY
        new Vector3(0.01392059f, 0.01097836f, 0.01483068f), // WHEAT
        new Vector3(0.0041f, 0.0192f, -0.0057f) // FLASHLIGHT
    };

    private Vector3[] allObjectPositionsRight = new Vector3[8]
    {
        new Vector3(-0.04559939f, 0.02110242f, -0.003600158f), // BAG
        new Vector3(-0.004600528f, 0.00939743f, -0.007199669f), // RUBBER_DUCKY
        new Vector3(-0.0025f, 0.0182f, -0.0062f), // PEACH
        new Vector3(-0.0003f, 0.0331f, -0.005f), // STRAWBERRY
        new Vector3(-0.0072f, 0.0196f, -0.0095f), // CANTALOUPE
        new Vector3(0.0051f, 0.0305f, -0.006f), // BLUEBERRY
        new Vector3(-0.01840058f, 0.008688283f, 0.01550032f), // WHEAT
        new Vector3(-0.004100041f, 0.01919305f, -0.005302102f) // FLASHLIGHT
    };

    private Quaternion[] allObjectRotationsLeft = new Quaternion[8]
    {
        Quaternion.Euler(0f, 90f, 0f), // BAG
        Quaternion.Euler(-15.865f, 22.223f, 34.353f), // RUBBER_DUCKY
        Quaternion.Euler(-5.64f, 4.661f, 90.595f), // PEACH
        Quaternion.Euler(0f, 0f, 90f), // STRAWBERRY
        Quaternion.Euler(0f, 0f, 0f), // CANTALOUPE
        Quaternion.Euler(4.003f, 24.241f, 91.8f), // BLUEBERRY
        Quaternion.Euler(16.234f, 33.554f, 100.504f), // WHEAT
        Quaternion.Euler(-180f, 90f, 180f) // FLASHLIGHT
    };

    private Quaternion[] allObjectRotationsRight = new Quaternion[8]
    {
        Quaternion.Euler(0f, -90f, 0f), // BAG
        Quaternion.Euler(0f, 159.498f, 9.144f), // RUBBER_DUCKY
        Quaternion.Euler(-5.64f, -175.339f, 90.595f), // PEACH
        Quaternion.Euler(0f, 180f, 90f), // STRAWBERRY
        Quaternion.Euler(0f, 0f, 0f), // CANTALOUPE
        Quaternion.Euler(183.722f, 147.97f, -87.67401f), // BLUEBERRY
        Quaternion.Euler(9.406f, -43.707f, 101.126f), // WHEAT
        Quaternion.Euler(-180f, 270f, 180f) // FLASHLIGHT
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
