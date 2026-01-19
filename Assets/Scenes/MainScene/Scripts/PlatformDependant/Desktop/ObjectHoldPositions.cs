using UnityEngine;

public class ObjectHoldPositions : MonoBehaviour
{
    private Vector3[] allObjectPositionsLeft = new Vector3[14]
    {
        new Vector3(0.0456f, 0.0211f, -0.0027f), // BAG
        new Vector3(0.0133f, 0.0137f, -0.007f), // RUBBER_DUCKY
        new Vector3(0.0025f,0.0158f, -0.0064f), // PEACH
        new Vector3(0.00077f, 0.03047f, -0.0053f), // STRAWBERRY
        new Vector3(0.0099f, 0.0175f, -0.008f), // CANTALOUPE
        new Vector3(-0.00514069f, 0.03108077f, -0.0057312f), // BLUEBERRY
        new Vector3(0.01392059f, 0.01097836f, 0.01483068f), // WHEAT
        new Vector3(0.0041f, 0.0192f, -0.0057f), // FLASHLIGHT
        new Vector3(0.0003204332f, 0.0162733f, -0.006679615f), // EGG
        new Vector3(0.01903992f, 0.02049595f, -0.02319194f), // FLOUR_JAR
        new Vector3(1.983345e-05f, 0.02659328f, -0.004195444f), // KNIFE
        new Vector3(-0.00038f, 0.01314f, -0.00221f), // LANTERN
        new Vector3(0.0253f, 0.0135f, -0.0092f), // MILK_CANISTER
        new Vector3(0.008180236f, 0.02287848f, -0.0223377f) // MIXING_BOWL
    };

    private Vector3[] allObjectPositionsRight = new Vector3[14]
    {
        new Vector3(-0.04559939f, 0.02110242f, -0.003600158f), // BAG
        new Vector3(-0.004600528f, 0.00939743f, -0.007199669f), // RUBBER_DUCKY
        new Vector3(-0.0025f, 0.0182f, -0.0062f), // PEACH
        new Vector3(-0.0003f, 0.0331f, -0.005f), // STRAWBERRY
        new Vector3(-0.0072f, 0.0196f, -0.0095f), // CANTALOUPE
        new Vector3(0.0051f, 0.0305f, -0.006f), // BLUEBERRY
        new Vector3(-0.01840058f, 0.008688283f, 0.01550032f), // WHEAT
        new Vector3(-0.004100041f, 0.01919305f, -0.005302102f), // FLASHLIGHT
        new Vector3(-0.0003005998f, 0.01638018f, -0.008376584f), // EGG
        new Vector3(-0.01903993f, 0.0204898f, -0.02080939f), // FLOUR_JAR
        new Vector3(0f, 0.0255f, -0.004f), // KNIFE
        new Vector3(-0.00221f, 0.0097f, -0.0018f), // LANTERN
        new Vector3(-0.0253f, 0.0154f, -0.0103f), // MILK_CANISTER
        new Vector3(-0.008180243f, 0.02286628f, -0.02166189f) // MIXING_BOWL
    };

    private Quaternion[] allObjectRotationsLeft = new Quaternion[14]
    {
        Quaternion.Euler(0f, 90f, 0f), // BAG
        Quaternion.Euler(-15.865f, 22.223f, 34.353f), // RUBBER_DUCKY
        Quaternion.Euler(-5.64f, 4.661f, 90.595f), // PEACH
        Quaternion.Euler(0f, 0f, 90f), // STRAWBERRY
        Quaternion.Euler(0f, 0f, 0f), // CANTALOUPE
        Quaternion.Euler(4.003f, 24.241f, 91.8f), // BLUEBERRY
        Quaternion.Euler(16.234f, 33.554f, 100.504f), // WHEAT
        Quaternion.Euler(-180f, 90f, 180f), // FLASHLIGHT
        Quaternion.Euler(-107.063f, 91.058f, -1.106018f), // EGG
        Quaternion.Euler(-15.914f, -180f, -90f), // FLOUR_JAR
        Quaternion.Euler(-90f, 0, 90f), // KNIFE
        Quaternion.Euler(0f, -90f, 90f), // LANTERN
        Quaternion.Euler(90f, 0f, 90f), // MILK_CANISTER
        Quaternion.Euler(0f, 0f, 90f) // MIXING_BOWL
    };

    private Quaternion[] allObjectRotationsRight = new Quaternion[14]
    {
        Quaternion.Euler(0f, -90f, 0f), // BAG
        Quaternion.Euler(0f, 159.498f, 9.144f), // RUBBER_DUCKY
        Quaternion.Euler(-5.64f, -175.339f, 90.595f), // PEACH
        Quaternion.Euler(0f, 180f, 90f), // STRAWBERRY
        Quaternion.Euler(0f, 0f, 0f), // CANTALOUPE
        Quaternion.Euler(183.722f, 147.97f, -87.67401f), // BLUEBERRY
        Quaternion.Euler(9.406f, -43.707f, 101.126f), // WHEAT
        Quaternion.Euler(-180f, 270f, 180f), // FLASHLIGHT
        Quaternion.Euler(-107.063f, -88.94202f, -1.106018f), // EGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG
        Quaternion.Euler(-15.914f, 0f, -90f), // FLOUR_JAR
        Quaternion.Euler(-80f, 180f, 90f), // KNIFE
        Quaternion.Euler(0f, 90f, 90f), // LANTERN
        Quaternion.Euler(90f, -180f, 90f), // MILK_CANISTER
        Quaternion.Euler(0f, 180f, 90f) // MIXING_BOWL
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
