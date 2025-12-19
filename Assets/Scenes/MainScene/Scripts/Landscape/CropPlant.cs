using System.Threading;
using UnityEngine;

// This class places crops in the correct positions and regrows them after a certain amount of time

public class CropPlant : MonoBehaviour
{

    [SerializeField] private CropTypes.CropType cropType; //The type of crop: Cantaloupe, Wheat, Blueberry, Peach, Strawberry
    [SerializeField] private GameObject cropPrefab; // The prefab belonging to the crop (type dependent)
    [SerializeField] private CropPositions cropPositions; // Script holding the positions for the crops

    private CropProduce[] crops; //  All crops of this type
    private Timer[] timers; // Keeps track of the regrow time for every crop
    private TimerCallback timerCallback = new TimerCallback((object o) => { timerElapsed(o); }); // Callback for when a timer elapses
    private Vector3[] positions; // Positions of the crops
    private Quaternion[] rotations; // Rotations of the crops
    private int growTime = 20 * 60; // This will later be changed to be dependent on the set DayNightCycle

    private void Start()
    {
        // Initialize the correct amount of crops depending on the crop type
        switch (cropType)
        {
            case CropTypes.CropType.Cantaloupe:
                initCrops(4, cropPositions);
                break;

            case CropTypes.CropType.Wheat:
                initCrops(7, cropPositions);
                break;

            case CropTypes.CropType.Blueberry:
                initCrops(12, cropPositions);
                break;

            case CropTypes.CropType.Peach:
                initCrops(8, cropPositions);
                break;

            case CropTypes.CropType.Strawberry:
                initCrops(5, cropPositions);
                break;
        }
    }

    // Create a crop and place it in the correct position
    private void initCrops(int count, CropPositions positions)
    {
        crops = new CropProduce[count]; // Create a new array to hold all crops of this type
        timers = new Timer[count]; // Create a new array to hold all timers for this crop type
        this.positions = positions.getPositions(cropType); // Get the positions for this crop type
        this.rotations = positions.getRotations(cropType); // Get the rotations for this crop type

        // For every crop of this type, create a new crop object and place it in the correct position
        for (int i = 0; i < count; i++)
        {
            timers[i] = new Timer(timerCallback, i, growTime, Timeout.Infinite);
            GameObject cropObject = Instantiate(cropPrefab);
            cropObject.transform.position = this.positions[positions.getCurrentPosition(cropType)];
            crops[i] = new CropProduce(cropType, positions.getCurrentPosition(cropType));
            positions.setCurrentPosition(cropType);
            cropObject.transform.rotation = this.rotations[positions.getCurrentRotation(cropType)];
            positions.setCurrentRotation(cropType);
        }
    }

    public void timerStart(GameObject crop)
    {

    }

    private static void timerElapsed(object position)
    {

    }

}
