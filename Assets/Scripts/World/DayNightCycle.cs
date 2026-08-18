using IngameDebugConsole;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CycleSpeed 
{ 
    TwelveMinutes, 
    FourtyEightMinutes, 
    TwoHours,
    SixHours,
    TwelveHours,
    OneDay
}

// This class manages the day-night cycle in the game.
public class DayNightCycle : MonoBehaviour
{
    #region Transitioner

    [Serializable]
    public class SkyBoxTransition
    {
        [SerializeField] private Material _dawnToMorning;
        [SerializeField] private Material _morningToDay;
        [SerializeField] private Material _dayToAfternoon;
        [SerializeField] private Material _afternoonToDusk;
        [SerializeField] private Material _duskToNight;
        [SerializeField] private Material _nightToDawn;
        
        private DayNightCycle cycle; // Day cycle reference
        
        private float _transitionSpeed; // How fast the skybox transitions

        public void SetCycle(DayNightCycle dayCycle)
        {
            cycle = dayCycle;
        }
        
        public void SetTransitionSpeed(float speed)
        {
            _transitionSpeed = speed;
        }

        public void UpdateTransition()
        {
            var minutes = cycle.GetMinutes();

            // Transition to next skybox phase as the day progresses
            switch (minutes)
            {
                case 240:
                    cycle.StartCoroutine(NightToDawn());
                    break;
                case 360:
                    cycle.StartCoroutine(DawnToMorning());
                    break;
                case 480:
                    cycle.StartCoroutine(MorningToDay());
                    break;
                case 660:
                    cycle.StartCoroutine(DayToAfternoon());
                    break;
                case 960:
                    cycle.StartCoroutine(AfternoonToDusk());
                    break;
                case 1080:
                    cycle.StartCoroutine(DuskToNight());
                    break;
            }
        }

        #region Transitions 

        // Coroutines to transition skyboxes
        private IEnumerator NightToDawn()
        {
            // Set blend to 0 and gradually increase it over transition speed
            _nightToDawn.SetFloat("_Blend", 0);
            for (float i = 0; i < _transitionSpeed; i += Time.deltaTime)
            {
                _nightToDawn.SetFloat("_Blend", i / _transitionSpeed);
                yield return null;
            }
            _dawnToMorning.SetFloat("_Blend", 0);
            RenderSettings.skybox = _dawnToMorning;
            _nightToDawn.SetFloat("_Blend", 0);
        }

        private IEnumerator DawnToMorning()
        {
            _dawnToMorning.SetFloat("_Blend", 0);
            for (float i = 0; i < _transitionSpeed; i += Time.deltaTime)
            {
                _dawnToMorning.SetFloat("_Blend", i / _transitionSpeed);
                yield return null;
            }
            _morningToDay.SetFloat("_Blend", 0);
            RenderSettings.skybox = _morningToDay;
            _dawnToMorning.SetFloat("_Blend", 0);
        }

        private IEnumerator MorningToDay()
        {
            _morningToDay.SetFloat("_Blend", 0);
            for (float i = 0; i < _transitionSpeed; i += Time.deltaTime)
            {
                _morningToDay.SetFloat("_Blend", i / _transitionSpeed);
                yield return null;
            }
            _dayToAfternoon.SetFloat("_Blend", 0);
            RenderSettings.skybox = _dayToAfternoon;
            _morningToDay.SetFloat("_Blend", 0);
        }

        private IEnumerator DayToAfternoon()
        {
            _dayToAfternoon.SetFloat("_Blend", 0);
            for (float i = 0; i < _transitionSpeed; i += Time.deltaTime)
            {
                _dayToAfternoon.SetFloat("_Blend", i / _transitionSpeed);
                yield return null;
            }
            _afternoonToDusk.SetFloat("_Blend", 0);
            RenderSettings.skybox = _afternoonToDusk;
            _dayToAfternoon.SetFloat("_Blend", 0);
        }

        private IEnumerator AfternoonToDusk()
        {
            _afternoonToDusk.SetFloat("_Blend", 0);
            for (float i = 0; i < _transitionSpeed; i += Time.deltaTime)
            {
                _afternoonToDusk.SetFloat("_Blend", i / _transitionSpeed);
                yield return null;
            }
            _duskToNight.SetFloat("_Blend", 0);
            RenderSettings.skybox = _duskToNight;
            _afternoonToDusk.SetFloat("_Blend", 0);
        }

        private IEnumerator DuskToNight()
        {
            _duskToNight.SetFloat("_Blend", 0);
            for (float i = 0; i < _transitionSpeed; i += Time.deltaTime)
            {
                _duskToNight.SetFloat("_Blend", i / _transitionSpeed);
                yield return null;
            }
            _nightToDawn.SetFloat("_Blend", 0);
            RenderSettings.skybox = _nightToDawn;
            _duskToNight.SetFloat("_Blend", 0);
        }

        #endregion
        
    }
    

    #endregion
    
    [SerializeField] 
    private SkyBoxTransition transition;
    
    
    // The set speed of the day-night cycle.
    private float rotateSpeed;   
    
    [field: SerializeField]
    public CycleSpeed currentCycleSpeed { get; private set; }

                                                                           // How far the sun moves each second.
    [Space(20)]
    [SerializeField] GameObject revolutionPoint;                                                        // The point around which the sun revolves.
    [SerializeField] Light sun;                                                                         // The sun light source.
    
                                                                                  
    private float minutesAccumulated;                                                                   // Total minutes passed in the current day.

    private int minutes;  
    private int _minutes { get { return minutes; } set { minutes = value; OnMinutesChange(value);  } }  // Call the method every minute
    
    [SerializeField] private int hours;                                                                 // Total hours passed in the current day.
    private int _hours { get { return hours; } 
        set 
        {
            if (_hours == value) { return; }
            hours = value; 
            OnHourChange(value); 
        } }            // Call the method every hour
    
    private int days;                                                                                   // Total days passed in the game.
    private int _days { get { return days; } set { days = value; } }

    private List<DaylightSensor> _daylightSensors;

    private void OnValidate()
    {
        //Update the rotation speed when inspector values get changed.
        UpdateSpeed();
    }

    private void Start()
    {
        transition.SetCycle(this);
        
        // Initialize the minutes based on the starting hour
        _minutes = 60 * hours; 
        
        // Set initial sun intensity
        ChangeSunIntensity(_minutes); 
        
        // Rotate sun to starting position based on hours
        revolutionPoint.transform.rotation = Quaternion.Euler(hours * 15f, 0, 0); 

        // Determine rotationSpeed and skybox transition speed based on selected cycle speed
        SetCycleSpeed(currentCycleSpeed);

        _daylightSensors = FindObjectsByType<DaylightSensor>(FindObjectsSortMode.None).ToList();

        DebugLogConsole.AddCommandInstance("SetTime", "Change the time, a value between 0 and 1 (as percentage of hours between 0 and 24)", nameof(SetTime), this);
    }

    void Update()
    {
        // Update the day night cycle
        UpdateCycle();
    }

    private void UpdateCycle()
    {
        float minutesPerSecond = 1440f / (360f / rotateSpeed);                                                                  
                                                                                                                        
        float delta = minutesPerSecond * Time.deltaTime;                                                                        
        minutesAccumulated += delta;                                                                                            
                                                                                                                        
        if (minutesAccumulated >= 1f)                                                                                           
        {                                                                                                                       
            int wholeMinutes = (int)minutesAccumulated;                                                                         
            _minutes += wholeMinutes;                                                                                           
            minutesAccumulated -= wholeMinutes;                                                                                 
        }                                                                                                                       
                                                                                                                        
        revolutionPoint.transform.Rotate(rotateSpeed * Time.deltaTime, 0f, 0f);              
        
        // Update the skybox transition.
        transition.UpdateTransition();
    }

    private void SetCycleSpeed(CycleSpeed cycleSpeed)
    {
        currentCycleSpeed = cycleSpeed;
        
        //Update the current speed when the cycle speed changes
        UpdateSpeed();
    }

    private void UpdateSpeed()
    {
        rotateSpeed = currentCycleSpeed switch
        {
            CycleSpeed.TwelveMinutes => 360f / (12 * 60),
            CycleSpeed.FourtyEightMinutes => 360f / (48 * 60),
            CycleSpeed.TwoHours => 360f / (2 * 60 * 60),
            CycleSpeed.SixHours => 360f / (6 * 60 * 60),
            CycleSpeed.TwelveHours => 360f / (12 * 60 * 60),
            CycleSpeed.OneDay => 360f / (24 * 60 * 60),
            _ => rotateSpeed
        };
        
        var transitionSpeed = currentCycleSpeed switch
        {
            CycleSpeed.TwelveMinutes => 60f,
            CycleSpeed.FourtyEightMinutes => 240f,
            CycleSpeed.TwoHours => 600f,
            CycleSpeed.SixHours => 1800f,
            CycleSpeed.TwelveHours => 3600f,
            CycleSpeed.OneDay => 7200f,
            _ => 0
        };
        
        transition.SetTransitionSpeed(transitionSpeed);
    }

    private void OnMinutesChange(int value)
    {
        // Update hours based on total minutes
        _hours = minutes / 60; 

        // Update sun intensity based on time of day
        ChangeSunIntensity(value); 

        if (value >= 1440)
        {
            // Reset minutes after a full day
            minutes = 0;
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

        StartCoroutine(ChurchTimer.Instance.TryRingBells(value));

        switch (value)
        {
            case 3: StaticAmbianceManager.Instance.CurrentTimeOfDay = StaticAmbianceManager.TimeOfDay.Morning; break;
            case 5: foreach (DaylightSensor sensor in _daylightSensors) { sensor.TurnLightsOff(); } break;
            case 6: MusicManager.Instance.IsDay = true; break;
            case 9: StaticAmbianceManager.Instance.CurrentTimeOfDay = StaticAmbianceManager.TimeOfDay.Day; break;
            case 17: foreach (DaylightSensor sensor in _daylightSensors) { sensor.TurnLightsOn(); } break;
            case 18: 
                StaticAmbianceManager.Instance.CurrentTimeOfDay = StaticAmbianceManager.TimeOfDay.Night;
                MusicManager.Instance.IsDay = false;
                break;

        }
    }

    // Adjusts the sun's intensity based on the current time of day
    private void ChangeSunIntensity(int value)
    {
        float intensity;

        if (value >= 300 && value <= 500)
        {
            intensity = (value - 300) / 200f * 1.5f;
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

    public CycleSpeed GetSpeed()
    {
        return currentCycleSpeed;
    }

    #region Public Methods

    [Button("Set Day", EButtonEnableMode.Playmode)]
    public void SetDay()
    {
        SetTimeOfDay(0.4f);
    }

    [Button("Set Night", EButtonEnableMode.Playmode)]
    public void SetNight()
    {
        SetTimeOfDay(0.8f);
    }

    public void SetTime(float newTime)
    {
        SetTimeOfDay(Mathf.Clamp(newTime, 0, 1));

        Debug.Log("Changed time of day to: " + newTime);
    }

    public void SetTimeOfDay(float timeOfDay)
    {
        timeOfDay = Mathf.Clamp01(timeOfDay);

        int targetMinutes = (int)(timeOfDay * 1440f);

        minutesAccumulated = 0;

        _minutes = targetMinutes;

        float sunAngle = targetMinutes / 1440f * 360f;
        revolutionPoint.transform.rotation = Quaternion.Euler(sunAngle, 0, 0);

        transition.UpdateTransition();
    }

    #endregion
}
