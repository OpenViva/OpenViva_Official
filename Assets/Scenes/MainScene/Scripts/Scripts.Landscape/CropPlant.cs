using System.Threading;
using UnityEngine;

public class CropPlant : MonoBehaviour
{
    // This class manages all crops in the game.
    // Code by Saien

    [SerializeField] private CropTypes.CropType cropType;
    [SerializeField] private GameObject cropPrefab;
    private CropProduce[] crops;
    private Timer[] timers;

    private Vector3[] positions;
    private int growTime = 20 * 60; // This will later be changed to be dependent on the set DayNightCycle

    private TimerCallback timerCallback = new TimerCallback((object o) => { timerElapsed(o); });

    private void Start()
    {
        switch (cropType)
        {
            case CropTypes.CropType.Cantaloupe:

                positions = new Vector3[4] 
                    {
                    new Vector3(1, 0, 1),
                    new Vector3(1, 0, -1),
                    new Vector3(-1, 0, 1),
                    new Vector3(-1, 0, -1)
                    };
                
                initCrops(4);

                break;

            case CropTypes.CropType.Wheat:

                positions = new Vector3[7]
                    {
                    new Vector3(3, 0, 1),
                    new Vector3(3, 0, -1),
                    new Vector3(1, 0, 1),
                    new Vector3(1, 0, -1),
                    new Vector3(-1, 0, 1),
                    new Vector3(-1, 0, -1),
                    new Vector3(-3, 0, 0)
                    };
                
                initCrops(7);

                break;

            case CropTypes.CropType.Blueberry:

                positions = new Vector3[12]
                    {
                    new Vector3(3, 0, 3),
                    new Vector3(3, 0, 1),
                    new Vector3(3, 0, -1),
                    new Vector3(3, 0, -3),
                    new Vector3(-3, 0, 3),
                    new Vector3(-3, 0, 1),
                    new Vector3(-3, 0, -1),
                    new Vector3(-3, 0, -3),
                    new Vector3(1, 0, 0),
                    new Vector3(-1, 0, 0),
                    new Vector3(0, 0, 1),
                    new Vector3(0, 0, -1)
                    };
                
                initCrops(12);

                break;

            case CropTypes.CropType.Peach:

                positions = new Vector3[16]
                    {
                    new Vector3(3, 0, 3),
                    new Vector3(3, 0, 1),
                    new Vector3(3, 0, -1),
                    new Vector3(3, 0, -3),
                    new Vector3(1, 0, 3),
                    new Vector3(1, 0, -3),
                    new Vector3(-1, 0, 3),
                    new Vector3(-1, 0, -3),
                    new Vector3(-3, 0, 3),
                    new Vector3(-3, 0, 1),
                    new Vector3(-3, 0, -1),
                    new Vector3(-3, 0, -3),
                    new Vector3(0, 0, 1),
                    new Vector3(0, 0, -1),
                    new Vector3(1, 0, 0),
                    new Vector3(-1, 0, 0)
                    };
                
                initCrops(16);

                break;

            case CropTypes.CropType.Strawberry:

                positions = new Vector3[8]
                    {
                    new Vector3(2, 0, 2),
                    new Vector3(2, 0, -2),
                    new Vector3(-2, 0, 2),
                    new Vector3(-2, 0, -2),
                    new Vector3(0, 0, 2),
                    new Vector3(0, 0, -2),
                    new Vector3(2, 0, 0),
                    new Vector3(-2, 0, 0)
                    };

                initCrops(8);

                break;

        }
    }

    private void initCrops(int count)
    {
        crops = new CropProduce[count];
        timers = new Timer[count];
        for (int i = 0; i < count; i++)
        {
            timers[i] = new Timer(timerCallback, i, growTime, Timeout.Infinite);
            GameObject cropObject = Instantiate(cropPrefab);
            cropObject.transform.position = positions[i];
            crops[i] = cropObject.GetComponent<CropProduce>();
        }
    }

    public void timerStart(GameObject crop)
    {

    }

    private static void timerElapsed(object position)
    {

    }

}
