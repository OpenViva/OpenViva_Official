using System.Threading;
using UnityEngine;

public class CropPlant : MonoBehaviour
{
    // This class manages all crops in the game.
    // Code by Saien

    [SerializeField] private static CropTypes.CropType cropType;
    [SerializeField] private GameObject cropPrefab;
    [SerializeField] private CropPositions cropPositions;

    private CropProduce[] crops;
    private Timer[] timers;
    private TimerCallback timerCallback = new TimerCallback((object o) => { timerElapsed(o); });
    private Vector3[] positions;
    private int growTime = 20 * 60; // This will later be changed to be dependent on the set DayNightCycle

    private void Start()
    {
        switch (cropType)
        {
            case CropTypes.CropType.Cantaloupe:
                initCrops(4, cropPositions);
                break;
                // To do: Add more crop types here
        }
    }

    private void initCrops(int count, CropPositions positions)
    {
        crops = new CropProduce[count];
        timers = new Timer[count];
        this.positions = positions.getPositions(cropType);

        for (int i = 0; i < count; i++)
        {
            timers[i] = new Timer(timerCallback, i, growTime, Timeout.Infinite);
            GameObject cropObject = Instantiate(cropPrefab);
            cropObject.transform.position = this.positions[positions.getCurrentPosition()];
            positions.setCurrentPosition();
            crops[i] = new CropProduce(cropType, positions.getCurrentPosition());
        }
    }

    public void timerStart(GameObject crop)
    {

    }

    private static void timerElapsed(object position)
    {

    }

}
