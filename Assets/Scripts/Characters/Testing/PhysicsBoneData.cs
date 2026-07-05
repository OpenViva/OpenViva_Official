using UnityEngine;

[CreateAssetMenu(fileName = "PhysBoneData", menuName = "Viva/Physics Bone Data")]
public class PhysicsBoneData : ScriptableObject
{
    // TODO: Edit this to fit the game needs
    public string boneName;
    public string preset;
    public float gravity;
    public float damping;
    public float distanceCompression;
    public float stiffnessValue;
    public bool useStiffnessCurve;
    public float stiffnessCurveStart;
    public float stiffnessCurveEnd;
    public float velocityAttenuation;
    public bool useLimit;
    public float speedLimit;
}
