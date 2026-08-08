using UnityEngine;

public class AmbianceTrigger : MonoBehaviour
{
    [SerializeField] private DynamicAmbianceController.IndoorArea areaID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TagConstants.MainPlayer)) 
        {
            StopCoroutine(DynamicAmbianceController.Instance.OnAreaChanged(areaID));
            StartCoroutine(DynamicAmbianceController.Instance.OnAreaChanged(areaID));
        }
    }
}
