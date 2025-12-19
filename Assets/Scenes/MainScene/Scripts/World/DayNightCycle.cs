using System;
using System.Collections;
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
    private int _minutes { get { return minutes; } set { minutes = value; OnMinutesChange(value);  } } // Call the method every minute
    [SerializeField] private int hours; // Total hours passed in the current day.
    private int _hours { get { return hours; } set { hours = value; OnHourChange(value); } } // Call the method every hour
    private int days; // Total days passed in the game.
    private int _days { get { return days; } set { days = value; } }

    // Skybox textures for different times of the day.
    [SerializeField] Texture2D morningSkybox;
    [SerializeField] Texture2D daySkybox;
    [SerializeField] Texture2D afternoonSkybox;
    [SerializeField] Texture2D eveningSkybox;
    [SerializeField] Texture2D nightSkybox;
    private float transitionSpeed = 0f; // Speed of skybox transition based on cycle speed.

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
                transitionSpeed = 25f;
                break;

            case CycleSpeeds.Speed.TwentyMinutes:
                increment = 360f / (20 * 60);
                transitionSpeed = 100f;
                break;

            case CycleSpeeds.Speed.OneHour:
                increment = 360f / (1 * 60 * 60);
                transitionSpeed = 300f;
                break;

            case CycleSpeeds.Speed.ThreeHours:
                increment = 360f / (3 * 60 * 60);
                transitionSpeed = 900f;
                break;

            case CycleSpeeds.Speed.SixHours:
                increment = 360f / (6 * 60 * 60);
                transitionSpeed = 1800f;
                break;

            case CycleSpeeds.Speed.TwelveHours:
                increment = 360f / (12 * 60 * 60);
                transitionSpeed = 3600f;
                break;

            case CycleSpeeds.Speed.OneDay:
                increment = 360f / (24 * 60 * 60);
                transitionSpeed = 7200f;
                break;
        }

        // Set initial skybox based on starting time
        switch (_minutes)
        {
            case int n when (n >= 0 && n < 300):
                RenderSettings.skybox.SetTexture("_Texture1", nightSkybox);
                break;
            case int n when (n >= 300 && n < 420):
                RenderSettings.skybox.SetTexture("_Texture1", morningSkybox);
                break;
            case int n when (n >= 420 && n < 720):
                RenderSettings.skybox.SetTexture("_Texture1", daySkybox);
                break;
            case int n when (n >= 720 && n < 1020):
                RenderSettings.skybox.SetTexture("_Texture1", afternoonSkybox);
                break;
            case int n when (n >= 1020 && n < 1140):
                RenderSettings.skybox.SetTexture("_Texture1", eveningSkybox);
                break;
            case int n when (n >= 1140 && n < 1440):
                RenderSettings.skybox.SetTexture("_Texture1", nightSkybox);
                break;
        }
    }

    void Update()
    {
        // Increment minutes every second based on the cycle speed
        // NOTE: Since this is method is only called every frame, accuracy worsens over time, especially at lower cycle speeds.
        temp += Time.deltaTime;
        if (temp >= 1f)
        {
            _minutes += (int)(increment * 4);
            temp = 0f;
        }
    }

    private void OnMinutesChange(int value)
    {
        revolutionPoint.transform.Rotate(increment, 0, 0); // Rotate the sun based on the increment

        _hours = minutes / 60; // Update hours based on total minutes

        changeSunIntensity(value); // Update sun intensity based on time of day

        if (value >= 1440)
        {
            minutes = 0; // Reset minutes after a full day
        }

        // Transition to next skybox phase as the day progresses
        if (value == 300)
        {
            StartCoroutine(PhaseTransition(nightSkybox, morningSkybox));
        }
        else if (value == 420)
        {
            StartCoroutine(PhaseTransition(morningSkybox, daySkybox));
        }
        else if (value == 720)
        {
            StartCoroutine(PhaseTransition(daySkybox, afternoonSkybox));
        }
        else if (value == 1020)
        {
            StartCoroutine(PhaseTransition(afternoonSkybox, eveningSkybox));
        }
        else if (value == 1140)
        {
            StartCoroutine(PhaseTransition(eveningSkybox, nightSkybox));
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

    // Transitions to the next skybox phase
    private IEnumerator PhaseTransition(Texture2D from, Texture2D to)
    {
        RenderSettings.skybox.SetTexture("_Texture1", from);
        RenderSettings.skybox.SetTexture("_Texture2", to);
        RenderSettings.skybox.SetFloat("_Blend", 0);

        // Smoothly blend between the two skyboxes over the transition speed duration
        for (float i = 0; i < transitionSpeed; i += Time.deltaTime)
        {
            RenderSettings.skybox.SetFloat("_Blend", i / transitionSpeed);
            yield return null;
        }

        RenderSettings.skybox.SetTexture("_Texture1", to); // Make sure the new skybox is set at the end
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
}
