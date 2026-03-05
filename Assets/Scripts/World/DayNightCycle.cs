using System;
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

// This class manages the day-night cycle in the game.

public class DayNightCycle : MonoBehaviour
{

    [SerializeField] CycleSpeeds.Speed speed; // The set speed of the day-night cycle.
    private float increment; // How far the sun moves each second.

    [SerializeField] GameObject revolutionPoint; // The point around which the sun revolves.
    [SerializeField] Light sun; // The sun light source.

    private float temp; // Tracks how many frames have passed since the last second.

    private int minutes; // Total minutes passed in the current day.
    private float minutesAccumulated;

    private int _minutes { get { return minutes; } set { minutes = value; OnMinutesChange(value);  } } // Call the method every minute
    [SerializeField] private int hours; // Total hours passed in the current day.
    private int _hours { get { return hours; } set { hours = value; OnHourChange(value); } } // Call the method every hour
    private int days; // Total days passed in the game.
    private int _days { get { return days; } set { days = value; } }

    private void Start()
    {
        _minutes = 60 * hours; // Initialize the minutes based on the starting hour
        changeSunIntensity(_minutes); // Set initial sun intensity
        revolutionPoint.transform.Rotate(hours * 15f, 0, 0); // Rotate sun to starting position based on hours

        // Determine increment and skybox transition speed based on selected cycle speed
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
        // Increment minutes every second based on the cycle speed
        // NOTE: Since this is method is only called every frame, accuracy worsens over time, especially at lower cycle speeds.

        float minutesPerSecond = 1440f / (360f / increment);

        float delta = minutesPerSecond * Time.deltaTime;
        minutesAccumulated += delta;

        if (minutesAccumulated >= 1f)
        {
            int wholeMinutes = (int)minutesAccumulated;
            _minutes += wholeMinutes;
            minutesAccumulated -= wholeMinutes;
        }

        revolutionPoint.transform.Rotate(increment * Time.deltaTime, 0f, 0f);
    }

    private void OnMinutesChange(int value)
    {
        _hours = minutes / 60; // Update hours based on total minutes

        changeSunIntensity(value); // Update sun intensity based on time of day

        if (value >= 1440)
        {
            minutes = 0; // Reset minutes after a full day
        }
    }

    private void OnHourChange(int value)
    {
        // When a full day passes, reset hours, increment days, and reset sun position.
        if (value >= 24)
        {
            _hours = 0;
            _days++;
            revolutionPoint.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

    }

    // Adjusts the sun's intensity based on the current time of day
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

    public int GetMinutes()
    {
        return minutes;
    }

    public CycleSpeeds.Speed GetSpeed()
    {
        return speed;
    }   
}
