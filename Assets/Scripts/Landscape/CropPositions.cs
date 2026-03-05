using UnityEngine;

// This class holds the positions and rotations for all crops in the game.

public class CropPositions : MonoBehaviour
{
    // Remembers the position for crops so that they spawn in the right spot when they grow
    private int currentCantaloupePosition = 0;
    private int currentWheatPosition = 0;
    private int currentBlueberryPosition = 0;
    private int currentPeachPosition = 0;
    private int currentStrawberryPosition = 0;

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

    private Vector3[] wheatPositions = new Vector3[112]
    {
         new Vector3(-86.80966f, 97.78565f, -125.5942f),
         new Vector3(-86.83301f, 98.1288f, -125.8438f),
         new Vector3(-86.73935f, 97.53363f, -125.8945f),
         new Vector3(-86.83902f, 97.63669f, -125.9128f),
         new Vector3(-86.37848f, 97.807f, -125.4717f),
         new Vector3(-86.40436f, 98.02809f, -125.1311f),
         new Vector3(-86.66647f, 97.87498f, -125.3887f),

         new Vector3(-83.86331f, 97.59706f, -108.6566f),
         new Vector3(-83.88666f, 97.94021f, -108.9062f),
         new Vector3(-83.793f, 97.34503f, -108.9569f),
         new Vector3(-83.89267f, 97.4481f, -108.9752f),
         new Vector3(-83.43213f, 97.61841f, -108.5341f),
         new Vector3(-83.45801f, 97.8395f, -108.1935f),
         new Vector3(-83.72012f, 97.68639f, -108.4511f),

         new Vector3(-83.9457f, 97.41065f, -126.2278f),
         new Vector3(-83.96905f, 97.7538f, -126.4774f),
         new Vector3(-83.64273f, 97.15863f, -126.1702f),
         new Vector3(-83.7424f, 97.2617f, -126.2706f),
         new Vector3(-86.56482f, 96.79103f, -125.5524f),
         new Vector3(-84.04993f, 97.432f, -125.7919f),
         new Vector3(-84.39133f, 97.65309f, 97.65309f),

         new Vector3(-100.3368f, 98.46991f, -151.0074f),
         new Vector3(-100.33f, 98.1658f, -151.2645f),
         new Vector3(-100.4313f, -100.4313f, -151.2807f),
         new Vector3(-99.88956f, 98.49653f, -150.981f),
         new Vector3(-99.84457f, 98.77575f, -150.6885f),
         new Vector3(-100.1542f, 98.58908f, -150.855f),
         new Vector3(-100.4111f, 98.76356f, -151.3054f),

         new Vector3(-100.268f, 98.57833f, -150.4348f),
         new Vector3(-100.5022f, 98.92148f, -150.3455f),
         new Vector3(-100.5761f, 98.32631f, -150.4222f),
         new Vector3(-100.567f, 98.42937f, -150.3213f),
         new Vector3(-100.2653f, 98.59968f, -150.8831f),
         new Vector3(-99.93021f, 98.82077f, -150.9492f),
         new Vector3(-100.1082f, 98.66766f, -150.6278f),

         new Vector3(-96.19366f, 98.57833f, -153.3708f),
         new Vector3(-96.42783f, 98.92148f, -153.2815f),
         new Vector3(-96.50172f, 98.32631f, -153.3582f),
         new Vector3(-96.49261f, 98.42937f, -153.2573f),
         new Vector3(-96.19093f, 98.59968f, -153.8191f),
         new Vector3(-95.85583f, 98.82077f, -153.8852f),
         new Vector3(-96.03377f, 98.66766f, -153.5638f),

         new Vector3(-96.21524f, 98.57833f, 98.57833f),
         new Vector3(57.083f, 98.92148f, -153.2906f),
         new Vector3(-96.3075f, 98.32631f, -153.3673f),
         new Vector3(-96.39682f, 98.42937f, -153.3478f),
         new Vector3(-95.77927f, 98.59968f, -153.1888f),
         new Vector3(-95.63507f, 98.82077f, -152.8791f),
         new Vector3(-95.98972f, 98.66766f, -152.9753f),

         new Vector3(-99.6568f, 98.27362f, -145.4797f),
         new Vector3(-99.40945f, 98.61677f, -145.52f),
         new Vector3(-99.35245f, 98.0216f, -145.43f),
         new Vector3(-99.341f, 98.12466f, -145.5307f),
         new Vector3(-99.74957f, 98.29497f, -145.0411f),
         new Vector3(-100.0912f, 98.51605f, -145.0437f),
         new Vector3(-99.85211f, 98.36295f, -145.3228f),

         new Vector3(-99.17593f, 98.47865f, -145.3112f),
         new Vector3(-99.06387f, 98.8218f, -145.087f),
         new Vector3(-99.13281f, 98.22663f, -145.0058f),
         new Vector3(-99.03329f, 98.32969f, -145.0249f),
         new Vector3(-99.62222f, 98.5f, -145.2694f),
         new Vector3(-99.72134f, 98.72108f, -145.5963f),
         new Vector3(-99.38379f, 98.56799f, -145.451f),

         new Vector3(-99.55283f, 98.25662f, -145.6237f),
         new Vector3(-99.54306f, 98.59976f, -145.8742f),
         new Vector3(-99.4431f, 98.00459f, -145.9121f),
         new Vector3(-99.53992f, 98.10765f, -145.9434f),
         new Vector3(-99.14154f, 98.27797f, -145.4454f),
         new Vector3(-99.21213f, 98.49905f, -145.1112f),
         new Vector3(-99.43799f, 98.34595f, -145.4011f),

         new Vector3(-96.40189f, 98.38065f, -148.3552f),
         new Vector3(-96.28983f, 98.7238f, -148.131f),
         new Vector3(-96.35877f, 98.12863f, -148.0498f),
         new Vector3(-96.25925f, 98.23169f, -148.0689f),
         new Vector3(-96.84818f, 98.402f, -148.3134f),
         new Vector3(-96.9473f, 98.62308f, -148.6403f),
         new Vector3(-96.60973f, 98.46999f, -148.495f),

         new Vector3(-96.47552f, 98.38065f, -148.2433f),
         new Vector3(-96.71497f, 98.7238f, -148.1693f),
         new Vector3(-96.78384f, 98.12863f, -148.2505f),
         new Vector3(-96.78128f, 98.23169f, -148.1492f),
         new Vector3(-96.444f, 98.402f, -148.6904f),
         new Vector3(-96.10532f, 98.62309f, -148.7349f),
         new Vector3(-96.30365f, 98.46999f, -148.4255f),

         new Vector3(-85.40683f, 97.63365f, -125.1602f),
         new Vector3(-85.43018f, 97.9768f, -125.128f),
         new Vector3(-85.12946f, 97.31262f, -125.0254f),
         new Vector3(-85.08966f, 97.48469f, -125.1186f),
         new Vector3(-85.62137f, 97.655f, -124.7666f),
         new Vector3(-85.94791f, 87.87608f, -124.8669f),
         new Vector3(-85.63892f, 97.72298f, -125.0658f),

         new Vector3(-84.36774f, 97.63365f, -123.0983f),
         new Vector3(-84.24548f, 97.9768f, -122.8795f),
         new Vector3(-84.31061f, 97.38162f, -122.7952f),
         new Vector3(-84.21207f, 97.48469f, -122.8188f),
         new Vector3(-84.81165f, 97.655f, -123.0359f),
         new Vector3(-84/92569f, 97.87608f, -123.3579f),
         new Vector3(-84.58182f, 97.72298f, -123.2284f),

         new Vector3(-81.14764f, 97.38465f, -106.1437f),
         new Vector3(-81.12283f, 97.7278f, -105.8943f),
         new Vector3(-81.21619f, 97.13263f, -105.843f),
         new Vector3(-81.11639f, 97.23569f, -105.8254f),
         new Vector3(-81.57959f, 97.406f, -106.2637f),
         new Vector3(-81.55573f, 97.62709f, -106.6044f),
         new Vector3(-81.29205f, 97.474f, -106.3484f),

         new Vector3(-98.28973f, 98.57833f, -153.4936f),
         new Vector3(-98.20959f, 98.92148f, -153.2561f),
         new Vector3(-98.28903f, 98.32631f, -153.1852f),
         new Vector3(-98.18784f, 98.42937f, -153.1903f),
         new Vector3(-98.73758f, 98.59968f, -153.5135f),
         new Vector3(-98.79077f, 98.82077f, -153.8509f),
         new Vector3(-98.47641f, 98.66766f, -153.6606f)
    };

    private Vector3[] blueberryPositions = new Vector3[48]
    {
        new Vector3(-0.0407104f, 101.411f, 41.6765f),
        new Vector3(-0.1026001f, 101.5235f, 41.8911f),
        new Vector3(0.06311035f, 101.2479f, 41.79939f),
        new Vector3(-0.08731079f, 101.3533f, 41.90819f),
        new Vector3(-0.2443848f, 100.6324f, 41.95979f),
        new Vector3(0.1824951f, 100.5493f, 41.89632f),
        new Vector3(0.3417969f, 100.6767f, 41.9973f),
        new Vector3(0.0223999f, 100.713f, 41.5381f),
        new Vector3(-0.0920105f, 100.8673f, 41.4149f),
        new Vector3(0.1881104f, 100.9248f, 41.2632f),
        new Vector3(0.4158936f, 101.2248f, 42.7892f),
        new Vector3(-0.1982117f, 101.2175f, 42.10371f),

        new Vector3(0.3832703f, 101.165f, 40.73152f),
        new Vector3(0.3213806f, 101.2775f, 40.94612f),
        new Vector3(0.4870911f, 101.0019f, 40.85442f),
        new Vector3(0.3366699f, 101.1073f, 40.96321f),
        new Vector3(0.1795959f, 100.3864f, 41.01482f),
        new Vector3(0.6064758f, 100.3033f, 40.95134f),
        new Vector3(0.7657776f, 100.4307f, 41.05232f),
        new Vector3(0.4463806f, 100.467f, 40.59312f),
        new Vector3(0.3319702f, 100.6213f, 40.46992f),
        new Vector3(0.6120911f, 100.6788f, 40.31822f),
        new Vector3(0.8398743f, 100.9788f, 40.84422f),
        new Vector3(0.225769f, 100.9715f, 41.15874f),

        new Vector3(0.2554626f, 101.275f, 40.31813f),
        new Vector3(0.193573f, 101.3875f, 40.53273f),
        new Vector3(0.3592834f, 101.1119f, 40.44102f),
        new Vector3(0.2088623f, 101.2173f, 40.54982f),
        new Vector3(0.05178833f, 100.4964f, 40.60143f),
        new Vector3(0.4786682f, 100.4133f, 40.53795f),
        new Vector3(0.63797f, 100.5407f, 40.63893f),
        new Vector3(0.318573f, 100.577f, 40.17973f),
        new Vector3(0.2041626f, 100.7313f, 40.05653f),
        new Vector3(0.4842834f, 100.7888f, 39.90483f),
        new Vector3(0.7120667f, 101.0888f, 40.43083f),
        new Vector3(0.09796143f, 101.0815f, 40.74535f),

        new Vector3(0.4518433f, 101.2759f, 39.68733f),
        new Vector3(0.3899536f, 101.3884f, 39.90193f),
        new Vector3(0.5556641f, 101.1128f, 39.81023f),
        new Vector3(0.4052429f, 101.2182f, 39.91902f),
        new Vector3(0.2481689f, 100.4973f, 39.97063f),
        new Vector3(0.6750488f, 100.4142f, 39.90715f),
        new Vector3(0.8343506f, 100.5416f, 40.00813f),
        new Vector3(0.5149536f, 100.5779f, 39.54893f),
        new Vector3(0.4005432f, 100.7322f, 39.42574f),
        new Vector3(0.6806641f, 100.7897f, 39.27403f),
        new Vector3(0.9084473f, 101.0897f, 39.80003f),
        new Vector3(0.294342f, 101.0824f, 40.11455f)
    };

    private Vector3[] peachPositions = new Vector3[56]
    {
        new Vector3(-452.454f, 98.713f, -175.271f),
        new Vector3(-451.466f, 98.756f, -174.092f),
        new Vector3(-450.2487f, 98.3647f, -174.0449f),
        new Vector3(-450.003f, 98.44f, -173.483f),
        new Vector3(-450.656f, 98.5266f, -176.8895f),
        new Vector3(-449.4855f, 98.8244f, -185.2074f),
        new Vector3(-450.7924f, 98.1232f, -176.0263f),
        new Vector3(-452.0379f, 98.4914f, -176.3829f),

        new Vector3(-457.0491f, 98.713f, -164.5515f),
        new Vector3(-456.0611f, 98.756f, -163.3725f),
        new Vector3(-454.8438f, 98.3647f, -163.3254f),
        new Vector3(-454.5981f, 98.44f, -162.7635f),
        new Vector3(-455.2511f, 98.5266f, -166.17f),
        new Vector3(-454.0806f, 98.8244f, -164.4879f),
        new Vector3(-455.3875f, 98.1232f, -155.3068f),
        new Vector3(-456.623f, 98.4914f, -165.6634f),

        new Vector3(-401.9093f, 102.5261f, -106.658f),
        new Vector3(-400.9213f, 102.5691f, -105.479f),
        new Vector3(-399.704f, 102.1778f, -105.4319f),
        new Vector3(-399.4583f, 102.2531f, -104.87f),
        new Vector3(-400.1113f, 102.3397f, -108.2765f),
        new Vector3(-398.9408f, 102.6375f, -106.5944f),
        new Vector3(-400.2477f, 101.9363f, -107.4133f),
        new Vector3(-401.4832f, 102.3045f, -107.7699f),

        new Vector3(3.393066f, 98.27007f, -136.948f),
        new Vector3(4.381073f, 98.31306f, -135.769f),
        new Vector3(5.598389f, 97.92177f, -135.7219f),
        new Vector3(5.844086f, 97.99707f, -135.16f),
        new Vector3(5.191071f, 98.08367f, -138.5665f),
        new Vector3(6.361572f, 98.38147f, -136.8844f),
        new Vector3(5.054688f, 97.68027f, -137.7033f),
        new Vector3(3.819183f, 98.04847f, -138.0599f),

        new Vector3(8.781769f, 98.63974f, -131.8151f),
        new Vector3(9.769775f, 98.68274f, -130.6361f),
        new Vector3(10.98709f, 98.29144f, -130.589f),
        new Vector3(11.23279f, 98.36674f, -130.0271f),
        new Vector3(10.57977f, 98.45335f, -133.4336f),
        new Vector3(11.75027f, 98.75114f, -131.7515f),
        new Vector3(10.44339f, 98.04994f, -132.5704f),
        new Vector3(9.207886f, 98.41815f, -132.927f),

        new Vector3(-303.9341f, 102.6691f, 44.57906f),
        new Vector3(-302.946f, 102.7121f, 45.75807f),
        new Vector3(-301.7287f, 102.3208f, 45.80516f),
        new Vector3(-301.483f, 102.3961f, 46.36708f),
        new Vector3(-302.136f, 102.4827f, 42.96056f),
        new Vector3(-300.9655f, 102.7805f, 44.64265f),
        new Vector3(-302.2724f, 102.0793f, 43.82375f),
        new Vector3(-303.5079f, 102.4475f, 43.46715f),

        new Vector3(-224.6293f, 103.2291f, 37.68208f),
        new Vector3(-223.6413f, 103.2721f, 38.8611f),
        new Vector3(-222.424f, 102.8808f, 38.90819f),
        new Vector3(-222.1783f, 102.9561f, 39.47008f),
        new Vector3(-222.8313f, 103.0427f, 36.06358f),
        new Vector3(-221.6608f, 103.3405f, 37.74568f),
        new Vector3(-222.9677f, 102.6393f, 36.92677f),
        new Vector3(-224.2032f, 103.0075f, 36.57018f)
    };

    private Vector3[] strawberryPositions = new Vector3[20]
    {
        new Vector3(-277.762f, 100.041f, -187.02f),
        new Vector3(-277.4257f, 100.0352f, -186.718f),
        new Vector3(-277.2932f, 100.0335f -187.012f),
        new Vector3(-277.175f, 100.0661f, -187.3237f),
        new Vector3(-277.428f, 100.1009f, -187.38f),

        new Vector3(-278.294f, 100.041f, -185.734f),
        new Vector3(-277.9577f, 100.0352f, -185.432f),
        new Vector3(-277.8252f, 100.0335f, -185.726f),
        new Vector3(-277.707f, 100.0661f, -186.0377f),
        new Vector3(-277.96f, 100.1009f, -186.094f),

        new Vector3(-277.98f, 100.041f, -184.959f),
        new Vector3(-277.6437f, 100.0352f, -184.657f),
        new Vector3(-277.5112f, 100.0335f, -184.951f),
        new Vector3(-277.393f, 100.0661f, -185.2627f),
        new Vector3(-277.646f, 100.1009f, -185.319f),

        new Vector3(-277.5711f, 100.041f, -185.5987f),
        new Vector3(-277.2348f, 100.0352f, -185.2967f),
        new Vector3(-277.1023f, 100.0335f, -185.5907f),
        new Vector3(-276.9841f, 100.0661f, -185.9019f),
        new Vector3(-277.2371f, 100.1009f, -185.9582f)
    };

    // Remembers the rotation for the crop being initialized
    private int currentCantaloupeRotaton = 0;
    private int currentWheatRotation = 0;
    private int currentBlueberryRotaton = 0;
    private int currentPeachRotation = 0;
    private int currentStrawberryRotation = 0;

    private Quaternion[] cantaloupeRotations = new Quaternion[12]
    {
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),

        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),

        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0)
    };

    private Quaternion[] wheatRotations = new Quaternion[112]
    {
        Quaternion.Euler(77.556f, 65.628f, -122.917f),
        Quaternion.Euler(53.213f, 51.862f, -133.917f),
        Quaternion.Euler(24.144f, 21.117f, -147.299f),
        Quaternion.Euler(37.144f, 49.551f, -128.603f),
        Quaternion.Euler(42.221f, -99.474f, 58.186f),
        Quaternion.Euler(37.439f, -155.796f, 0.869f),
        Quaternion.Euler(82.539f, 168.302f, -15.952f),

        Quaternion.Euler(77.556f, 65.628f, -122.917f),
        Quaternion.Euler(53.213f, 51.862f, -133.917f),
        Quaternion.Euler(24.14f, 21.117f, -147.299f),
        Quaternion.Euler(37.144f, 49.551f, -128.603f),
        Quaternion.Euler(42.221f, -99.474f, 58.186f),
        Quaternion.Euler(37.439f, -155.796f, 0.869f),
        Quaternion.Euler(82.539f, 168.302f, -15.952f),

        Quaternion.Euler(77.556f, -21.956f, -122.917f),
        Quaternion.Euler(53.213f, -35.722f, -133.917f),
        Quaternion.Euler(24.144f, -66.467f, -147.299f),
        Quaternion.Euler(37.144f, -38.033f, -128.603f),
        Quaternion.Euler(-8.182f, 91.14f, -2.918f),
        Quaternion.Euler(137.779f, -7.057983f, -121.814f),
        Quaternion.Euler(37.439f, 116.62f, 0.869f),

        Quaternion.Euler(107.901f, -136.8f, 21.61f),
        Quaternion.Euler(15.213f, 31.033f, -153.223f),
        Quaternion.Euler(31.608f, 55.317f, -139.465f),
        Quaternion.Euler(40.913f, -78.257f, 72.162f),
        Quaternion.Euler(45.631f, -137.983f, 9.727f),
        Quaternion.Euler(86.935f, -0.534f, 163.297f),
        Quaternion.Euler(47.651f, 52.891f, -147.997f),

        Quaternion.Euler(102.444f, -8.848999f, 57.083f),
        Quaternion.Euler(126.787f, -22.616f, 46.08299f),
        Quaternion.Euler(155.856f, -53.361f, 32.70099f),
        Quaternion.Euler(142.856f, -24.927f, 51.39699f),
        Quaternion.Euler(42.221f, 6.048f, 58.186f),
        Quaternion.Euler(37.439f, -50.274f, 0.869f),
        Quaternion.Euler(82.539f, -86.176f, -15.952f),

        Quaternion.Euler(102.444f, -8.848999f, 57.083f),
        Quaternion.Euler(126.787f, -22.616f, 46.08299f),
        Quaternion.Euler(155.856f, -53.361f, 32.70099f),
        Quaternion.Euler(142.856f, -24.927f, 51.39699f),
        Quaternion.Euler(42.221f, 6.048f, 58.186f),
        Quaternion.Euler(37.439f, -50.274f, 0.869f),
        Quaternion.Euler(82.539f, -86.176f, -15.952f),

        Quaternion.Euler(102.444f, -85.05099f, 57.083f),
        Quaternion.Euler(53.213f, 81.183f, -133.917f),
        Quaternion.Euler(24.144f, 50.438f, -147.299f),
        Quaternion.Euler(37.144f, 78.872f, -128.603f),
        Quaternion.Euler(42.221f, -70.153f, 58.186f),
        Quaternion.Euler(37.439f, -126.475f, 0.869f),
        Quaternion.Euler(82.539f, -162.377f, -15.952f),

        Quaternion.Euler(77.556f, -20.452f, -122917f),
        Quaternion.Euler(53.213f, -34.218f, -133.917f),
        Quaternion.Euler(24.144f, -64.963f, -147.299f),
        Quaternion.Euler(37.144f, -36.529f, -128.603f),
        Quaternion.Euler(137.779f, -5.554993f, -121.814f),
        Quaternion.Euler(37.439f, 118.123f, 0.869f),
        Quaternion.Euler(82.539f, 82.221f, -15.952f),

        Quaternion.Euler(102.444f, 86.841f, 57.083f),
        Quaternion.Euler(126.787f, 73.074f, 46.08299f),
        Quaternion.Euler(155.856f, 42.329f, 32.70099f),
        Quaternion.Euler(142.856f, 70.763f, 51.39699f),
        Quaternion.Euler(42.221f, 101.739f, 58.186f),
        Quaternion.Euler(37.439f, 45.417f, 0.869f),
        Quaternion.Euler(82.539f, 9.515f, -15.952f),

        Quaternion.Euler(77.556f, 58.05f, -122.917f),
        Quaternion.Euler(53.213f, 44.284f, -133.917f),
        Quaternion.Euler(24.144f, 13.539f, -147.299f),
        Quaternion.Euler(37.144f, 41.973f, -128.603f),
        Quaternion.Euler(42.221f, -107.052f, 58.186f),
        Quaternion.Euler(37.439f, -163.374f, 0.869f),
        Quaternion.Euler(82.539f, 160.724f, -15.952f),

        Quaternion.Euler(102.444f, 86.841f, 57.083f),
        Quaternion.Euler(126.787f, 73.074f, 46.08299f),
        Quaternion.Euler(155.856f, 42.329f, 32.70099f),
        Quaternion.Euler(142.856f, 70.763f, 51.39699f),
        Quaternion.Euler(42.221f, 101.739f, 58.186f),
        Quaternion.Euler(37.439f, 45.417f, 0.869f),
        Quaternion.Euler(82.539f, 9.515f, -15.952f),

        Quaternion.Euler(102.444f, -12.543f, 57.083f),
        Quaternion.Euler(126.787f, -26.30902f, 46.08299f),
        Quaternion.Euler(155.856f, -57.05302f, 32.70099f),
        Quaternion.Euler(142.856f, -28.62002f, 51.39699f),
        Quaternion.Euler(42.221f, 2.356995f, 58.186f),
        Quaternion.Euler(37.439f, -53.967f, 0.869f),
        Quaternion.Euler(82.539f, -89.869f, -15.952f),

        Quaternion.Euler(77.556f, -37.104f, -122.917f),
        Quaternion.Euler(53.213f, -50.87f, -133.917f),
        Quaternion.Euler(24.144f, -81.615f, -147.299f),
        Quaternion.Euler(37.144f, -53.181f, -128.603f),
        Quaternion.Euler(42.221f, 157.794f, 58.186f),
        Quaternion.Euler(37.439f, 101.472f, 0.869f),
        Quaternion.Euler(82.539f, 65.57f, -15.952f),

        Quaternion.Euler(102.444f, 89.481f, 57.083f),
        Quaternion.Euler(126.787f, 75.714f, 46.08299f),
        Quaternion.Euler(155.856f, 44.969f, 32.70099f),
        Quaternion.Euler(142.856f, 73.403f, 51.39699f),
        Quaternion.Euler(42.221f, 104.379f, 58.186f),
        Quaternion.Euler(37.439f, 48.057f, 0.869f),
        Quaternion.Euler(82.539f, 12.155f, -15.952f),

        Quaternion.Euler(102.444f, 65.971f, 57.083f),
        Quaternion.Euler(126.787f, 52.205f, 46.08299f),
        Quaternion.Euler(155.856f, 21.46f, 32.70099f),
        Quaternion.Euler(142.856f, 49.894f, 51.39699f),
        Quaternion.Euler(42.221f, 80.87f, 58.186f),
        Quaternion.Euler(37.439f, 24.548f, 0.869f),
        Quaternion.Euler(82.539f, -11.356f, -15.952f),

        Quaternion.Euler(102.444f, 78.938f, 57.083f),
        Quaternion.Euler(126.787f, 65.172f, 46.08299f),
        Quaternion.Euler(155.856f, 34.42799f, 32.70099f),
        Quaternion.Euler(142.856f, 62.86199f, 51.39699f),
        Quaternion.Euler(42.221f, 93.836f, 58.186f),
        Quaternion.Euler(37.439f, 37.514f, 0.869f),
        Quaternion.Euler(82.539f, 1.612f, -15.952f)

    };

    private Quaternion[] blueberryRotations = new Quaternion[48]
    {
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),

        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),

        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),

        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
    };

    private Quaternion[] peachRotations = new Quaternion[56]
    {
        Quaternion.Euler(32.888f, 0, 0),
        Quaternion.Euler(-39.42f, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(-29.67f, 0, 0),
        Quaternion.Euler(45.1f, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(19.5f, 0, 0),
        Quaternion.Euler(0, 0, 0),

        Quaternion.Euler(32.888f, 0, 0),
        Quaternion.Euler(-39.42f, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(-29.67f, 0, 0),
        Quaternion.Euler(45.1f, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(19.5f, 0, 0),
        Quaternion.Euler(0, 0, 0),

        Quaternion.Euler(32.888f, 0, 0),
        Quaternion.Euler(-39.42f, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(-29.67f, 0, 0),
        Quaternion.Euler(45.1f, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(19.5f, 0, 0),
        Quaternion.Euler(0, 0, 0),

        Quaternion.Euler(32.888f, 0, 0),
        Quaternion.Euler(-39.42f, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(-29.67f, 0, 0),
        Quaternion.Euler(45.1f, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(19.5f, 0, 0),
        Quaternion.Euler(0, 0, 0),

        Quaternion.Euler(32.888f, 0, 0),
        Quaternion.Euler(-39.42f, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(-29.67f, 0, 0),
        Quaternion.Euler(45.1f, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(19.5f, 0, 0),
        Quaternion.Euler(0, 0, 0),

        Quaternion.Euler(32.888f, 0, 0),
        Quaternion.Euler(-39.42f, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(-29.67f, 0, 0),
        Quaternion.Euler(45.1f, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(19.5f, 0, 0),
        Quaternion.Euler(0, 0, 0),

        Quaternion.Euler(32.888f, 0, 0),
        Quaternion.Euler(-39.42f, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(-29.67f, 0, 0),
        Quaternion.Euler(45.1f, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(19.5f, 0, 0),
        Quaternion.Euler(0, 0, 0),
    };

    private Quaternion[] strawberryRotations = new Quaternion[20]
    {
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),

        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),

        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),

        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
    };

    // Sends the positions array based on the crop type
    public Vector3[] getPositions(CropTypes.CropType cropType)
    {
        switch (cropType)
        {
            case CropTypes.CropType.Cantaloupe:
                return cantaloupePositions;
            
            case CropTypes.CropType.Wheat:
                return wheatPositions;

            case CropTypes.CropType.Blueberry:
                return blueberryPositions;

            case CropTypes.CropType.Peach:
                return peachPositions;

            case CropTypes.CropType.Strawberry:
                return strawberryPositions;

            default:
                return null;
        }
    }

    // Sends the rotations array based on the crop type
    public Quaternion[] getRotations(CropTypes.CropType cropType)
    {
        switch (cropType)
        {
            case CropTypes.CropType.Cantaloupe:
                return cantaloupeRotations;

            case CropTypes.CropType.Wheat:
                return wheatRotations;

            case CropTypes.CropType.Blueberry:
                return blueberryRotations;

            case CropTypes.CropType.Peach:
                return peachRotations;

            case CropTypes.CropType.Strawberry:
                return strawberryRotations;

            default:
                return null;
        }
    }

    // Get the position of the crop being initialized
    public int getCurrentPosition(CropTypes.CropType cropType)
    {
        switch (cropType)
        {
            case CropTypes.CropType.Cantaloupe:
                return currentCantaloupePosition;

            case CropTypes.CropType.Wheat:
                return currentWheatPosition;

            case CropTypes.CropType.Blueberry:
                return currentBlueberryPosition;

            case CropTypes.CropType.Peach:
                return currentPeachPosition;

            case CropTypes.CropType.Strawberry:
                return currentStrawberryPosition;

            default:
                return -1;
        }
    }

    // Set the position to that of the next crop being initialized
    public void setCurrentPosition(CropTypes.CropType cropType)
    {
        switch (cropType)
        {
            case CropTypes.CropType.Cantaloupe:
                currentCantaloupePosition++;
                break;

            case CropTypes.CropType.Wheat:
                currentWheatPosition++;
                break;

            case CropTypes.CropType.Blueberry:
                currentBlueberryPosition++;
                break;

            case CropTypes.CropType.Peach:
                currentPeachPosition++;
                break;

            case CropTypes.CropType.Strawberry:
                currentStrawberryPosition++;
                break;
        }
    }

    // Get the rotation of the crop being initialized
    public int getCurrentRotation(CropTypes.CropType cropType)
    {
        switch (cropType)
        {
            case CropTypes.CropType.Cantaloupe:
                return currentCantaloupeRotaton;

            case CropTypes.CropType.Wheat:
                return currentWheatRotation;

            case CropTypes.CropType.Blueberry:
                return currentBlueberryRotaton;

            case CropTypes.CropType.Peach:
                return currentPeachRotation;

            case CropTypes.CropType.Strawberry:
                return currentStrawberryRotation;

            default:
                return -1;
        }
    }

    // Set the rotation to that of the next crop being initialized
    public void setCurrentRotation(CropTypes.CropType cropType)
    {
        switch (cropType)
        {
            case CropTypes.CropType.Cantaloupe:
                currentCantaloupeRotaton++;
                break;

            case CropTypes.CropType.Wheat:
                currentWheatRotation++;
                break;

            case CropTypes.CropType.Blueberry:
                currentBlueberryRotaton++;
                break;

            case CropTypes.CropType.Peach:
                currentPeachRotation++;
                break;

            case CropTypes.CropType.Strawberry:
                currentStrawberryRotation++;
                break;
        }
    }
}
