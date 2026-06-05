using UnityEngine;

public class ObjectHoldPositions : MonoBehaviour
{
    // the position and rotation to set different items to when they are held in either hand

    private Vector3[] allObjectPositionsLeft = new Vector3[21]
    {
        new Vector3(0.0424f, 0.0134f, -0.0307f), // BAG
        new Vector3(0.0033f, 0.0239f, -0.0041f), // RUBBER_DUCKY
        new Vector3(0.0025f,0.0158f, -0.0064f), // PEACH
        new Vector3(0.00077f, 0.03047f, -0.0053f), // STRAWBERRY
        new Vector3(0.0099f, 0.0175f, -0.008f), // CANTALOUPE
        new Vector3(-0.00514069f, 0.03108077f, -0.0057312f), // BLUEBERRY
        new Vector3(0.01392059f, 0.01097836f, 0.01483068f), // WHEAT
        new Vector3(0.00093f, 0.01507f, -0.0061f), // FLASHLIGHT
        new Vector3(0f, 0.0208f, -0.00887f), // EGG
        new Vector3(0.01714588f, 0.01800881f, 0.0001286522f), // FLOUR_JAR
        new Vector3(1.983345e-05f, 0.02659328f, -0.004195444f), // KNIFE
        new Vector3(-0.00038f, 0.01314f, -0.00221f), // LANTERN
        new Vector3(0.021f, 0.0156f, -0.018f), // MILK_CANISTER
        new Vector3(0.0216f, 0.016f, 0.0059f), // MIXING_BOWL
        new Vector3(-0.00547f, 0.01417f, -0.00738f), // MIXING_SPOON
        new Vector3(0.01442f, 0.01936f, 0.00331f), // MORTAR
        new Vector3(0.00066f, 0.01621f, -0.00778f), // PESTLE
        new Vector3(0.02351847f, 0.01467687f, 0.00121516f), // POT
        new Vector3(0.00144f, 0.01741f, -0.00344f), // SOAP
        new Vector3(0f, 0.0147f, -0.0041f), // TOWEL
        new Vector3(0, 0, 0)
    };

    private Vector3[] allObjectPositionsRight = new Vector3[21]
    {
        new Vector3(-0.0373f, 0.0241f, -0.0336f), // BAG
        new Vector3(-0.0075f, 0.024f, -0.0023f), // RUBBER_DUCKY
        new Vector3(-0.0025f, 0.0182f, -0.0062f), // PEACH
        new Vector3(-0.0003f, 0.0331f, -0.005f), // STRAWBERRY
        new Vector3(-0.0072f, 0.0196f, -0.0095f), // CANTALOUPE
        new Vector3(0.0051f, 0.0305f, -0.006f), // BLUEBERRY
        new Vector3(-0.01840058f, 0.008688283f, 0.01550032f), // WHEAT
        new Vector3(0.0008140024f, 0.01238399f, -0.006554195f), // FLASHLIGHT
        new Vector3(0.0013f, 0.02243f, -0.00793f), // EGG
        new Vector3(-0.0182f, 0.0204f, -0.0023f), // FLOUR_JAR
        new Vector3(0f, 0.0255f, -0.004f), // KNIFE
        new Vector3(-0.0011f, 0.0098f, -0.003f), // LANTERN
        new Vector3(-0.019f, 0.0127f, -0.02f), // MILK_CANISTER
        new Vector3(-0.0232f, 0.0229f, 0.0049f), // MIXING_BOWL
        new Vector3(0.00705f, 0.01507f, -0.00916f), // MIXING_SPOON
        new Vector3(-0.01161f, 0.02078f, 0.00617f), // MORTAR
        new Vector3(0f, 0.01641f, -0.00632f), // PESTLE
        new Vector3(-0.023f, 0.0154f, -0.0019f), // POT
        new Vector3(-0.00181f, 0.01307f, -0.00302f), // SOAP
        new Vector3(0.0015f, 0.0121f, -0.0021f), // TOWEL
        new Vector3(0, 0, 0)
    };

    private Quaternion[] allObjectRotationsLeft = new Quaternion[21]
    {
        Quaternion.Euler(0f, 124.092f, -91.166f), // BAG
        Quaternion.Euler(-26.159f, -128.685f, 142.79f), // RUBBER_DUCKY
        Quaternion.Euler(-5.64f, 4.661f, 90.595f), // PEACH
        Quaternion.Euler(0f, 0f, 90f), // STRAWBERRY
        Quaternion.Euler(0f, 0f, 0f), // CANTALOUPE
        Quaternion.Euler(4.003f, 24.241f, 91.8f), // BLUEBERRY
        Quaternion.Euler(16.234f, 33.554f, 100.504f), // WHEAT
        Quaternion.Euler(216f, 127.409f, 95.321f), // FLASHLIGHT
        Quaternion.Euler(-106.285f, 28.78799f, 49.843f), // EGG
        Quaternion.Euler(6.166f, -99.767f, 96.115f), // FLOUR_JAR
        Quaternion.Euler(-90f, 0, 90f), // KNIFE
        Quaternion.Euler(-6.73f, -54.462f, 2.129f), // LANTERN
        Quaternion.Euler(94.6f, -50.718f, 2.505f), // MILK_CANISTER
        Quaternion.Euler(0f, -98.908f, 90f), // MIXING_BOWL
        Quaternion.Euler(186.077f, 136.709f, 270.69f), // MIXING_SPOON
        Quaternion.Euler(112.405f, -113.551f, 63.413f), // MORTAR
        Quaternion.Euler(0f, -48.381f, 90f), // PESTLE
        Quaternion.Euler(0f, -93.053f, 90f), // POT
        Quaternion.Euler(-192.477f, 130.278f, 85f), // SOAP
        Quaternion.Euler(0f, 34.82f, 100.165f), // TOWEL
        Quaternion.Euler(0, 0, 0)
    };

    private Quaternion[] allObjectRotationsRight = new Quaternion[21]
    {
        Quaternion.Euler(-11.989f, -129.545f, 90f), // BAG
        Quaternion.Euler(155.574f, 141.103f, 346.339f), // RUBBER_DUCKY
        Quaternion.Euler(-5.64f, -175.339f, 90.595f), // PEACH
        Quaternion.Euler(0f, 180f, 90f), // STRAWBERRY
        Quaternion.Euler(0f, 0f, 0f), // CANTALOUPE
        Quaternion.Euler(183.722f, 147.97f, -87.67401f), // BLUEBERRY
        Quaternion.Euler(9.406f, -43.707f, 101.126f), // WHEAT
        Quaternion.Euler(298.122f, 30.88f, 90.377f), // FLASHLIGHT
        Quaternion.Euler(-80.206f, -91.88f, 1.908f), // EGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG
        Quaternion.Euler(-8.071f, -96.902f, 92.164f), // FLOUR_JAR
        Quaternion.Euler(-80f, 180f, 90f), // KNIFE
        Quaternion.Euler(8.833f, 48.026f, -1.891f), // LANTERN
        Quaternion.Euler(86.158f, 48.708f, 0f), // MILK_CANISTER
        Quaternion.Euler(0f, -90.336f, 87.669f), // MIXING_BOWL
        Quaternion.Euler(182.481f, 44.647f, 260.3f), // MIXING_SPOON
        Quaternion.Euler(-110.608f, -21.64899f, 33.53799f), // MORTAR
        Quaternion.Euler(-180f, 39.15199f, -90f), // PESTLE
        Quaternion.Euler(0f, -91.117f, 87.762f), // POT
        Quaternion.Euler(5.078f, 48.841f, 90.273f), // SOAP
        Quaternion.Euler(0f, 138.14f, 92.2f), // TOWEL
        Quaternion.Euler(0, 0, 0)
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
