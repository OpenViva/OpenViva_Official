using System;
using System.Collections;
using UnityEngine;

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

    [SerializeField] Texture2D morningSkybox;
    [SerializeField] Texture2D daySkybox;
    [SerializeField] Texture2D afternoonSkybox;
    [SerializeField] Texture2D eveningSkybox;
    [SerializeField] Texture2D nightSkybox;
    private Boolean isTransitioning = false;
    private float transitionSpeed = 0f;

    private void Start()
    {
        _minutes = 60 * hours;
        changeSunIntensity(_minutes);
        revolutionPoint.transform.Rotate(hours * 15f, 0, 0);

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
        if (value >= 24)
        {
            _hours = 0;
            _days++;
            revolutionPoint.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

    }

    private IEnumerator PhaseTransition(Texture2D from, Texture2D to)
    {
        isTransitioning = true;

        RenderSettings.skybox.SetTexture("_Texture1", from);
        RenderSettings.skybox.SetTexture("_Texture2", to);
        RenderSettings.skybox.SetFloat("_Blend", 0);

        for (float i = 0; i < transitionSpeed; i += Time.deltaTime)
        {
            RenderSettings.skybox.SetFloat("_Blend", i / transitionSpeed);
            yield return null;
        }

        RenderSettings.skybox.SetTexture("_Texture1", to);
        isTransitioning = false;
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
