using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DaylightSensor : MonoBehaviour
{
    private List<Light> _lights;

    private void Awake()
    {
        _lights = GetComponentsInChildren<Light>().ToList();
    }

    public void TurnLightsOff()
    {
        foreach (Light light in _lights) { light.enabled = false; }
    }

    public void TurnLightsOn()
    {
        foreach (Light light in _lights) { light.enabled = true; }
    }
}
