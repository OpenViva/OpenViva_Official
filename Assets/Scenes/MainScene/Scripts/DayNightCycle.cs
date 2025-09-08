using System;
using Unity.XR.CoreUtils;
using UnityEditor.Search;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class DayNightCycle : MonoBehaviour
{
    // This class manages the day-night cycle in the game.

    // Code by Saien

    [SerializeField] CycleSpeeds.Speed speed;
    private float increment;

    [SerializeField] GameObject revolutionPoint;
    [SerializeField] Light sun;

    private float temp;

    private int minutes;
    private int _minutes { get { return minutes; } set { minutes = value; OnMinutesChange(value);  } }
    [SerializeField] private int hours;
    private int _hours { get { return hours; } set { hours = value; OnHourChange(value); } } // Will add skyBoxes
    private int days;
    private int _days { get { return days; } set { days = value; } }

    private void Start()
    {
        _minutes = 60 * hours;
        changeSunIntensity(_minutes);
        revolutionPoint.transform.Rotate(hours * 15, 0, 0);

        switch (speed)
        {
            case CycleSpeeds.Speed.FiveMinutes:
                increment = 360f / (5 * 60);
                break;

            case CycleSpeeds.Speed.TwentyMinutes:
                increment = 360f / (20 * 60);
                break;

            case CycleSpeeds.Speed.OneHour:
                increment = 360f / (1 * 60 * 60);
                break;

            case CycleSpeeds.Speed.ThreeHours:
                increment = 360f / (3 * 60 * 60);
                break;

            case CycleSpeeds.Speed.SixHours:
                increment = 360f / (6 * 60 * 60);
                break;

            case CycleSpeeds.Speed.TwelveHours:
                increment = 360f / (12 * 60 * 60);
                break;

            case CycleSpeeds.Speed.OneDay:
                increment = 360f / (24 * 60 * 60);
                break;
        }
    }

    void Update()
    {
        temp += Time.deltaTime;
        if (temp >= 1f)
        {
            _minutes += (int)(increment * 4);
            temp = 0f;
        }
    }

    private void OnMinutesChange(int value)
    {
        revolutionPoint.transform.Rotate(increment, 0, 0);

        _hours = minutes / 60;

        changeSunIntensity(value);

        if (value >= 1440)
        {
            minutes = 0;
        }
    }

    private void OnHourChange(int value)
    {
        if (value >= 24)
        {
            _hours = 0;
            _days++;
            revolutionPoint.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    private void changeSunIntensity(int value)
    {
        float intensity;

        if (value >= 300 && value <= 500)
        {
            intensity = ((value - 300) / 200f) * 1.5f;
            sun.intensity = intensity;
        }

        if (value > 500 && value < 950)
        {
            sun.intensity = 1.5f;
        }

        if (value >= 900 && value <= 1110)
        {
            intensity = ((1110 - value) / 200f) * 1.5f;
            sun.intensity = intensity;
        }

        if ((value > 1150 && value < 1440) || (value >= 0 && value < 300))
        {
            sun.intensity = 0f;
        }
    }
}
