using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CycleSpeed 
{ 
    FiveMinutes, 
    TwentyMinutes, 
    OneHour,
    ThreeHours,
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
        [SerializeField] private static Material _dawnToMorning;
        [SerializeField] private static Material _morningToDay;
        [SerializeField] private static Material _dayToAfternoon;
        [SerializeField] private static Material _afternoonToDusk;
        [SerializeField] private static Material _duskToNight;
        [SerializeField] private static Material _nightToDawn;
        
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
                case 300:
                    cycle.StartCoroutine(NightToDawn());
                    break;
                case 420:
                    cycle.StartCoroutine(DawnToMorning());
                    break;
                case 600:
                    cycle.StartCoroutine(MorningToDay());
                    break;
                case 840:
                    cycle.StartCoroutine(DayToAfternoon());
                    break;
                case 1020:
                    cycle.StartCoroutine(AfternoonToDusk());
                    break;
                case 1140:
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
            RenderSettings.skybox = _nightToDawn;
            _duskToNight.SetFloat("_Blend", 0);
        }

        public static void ResetBlends()
        {
            _dawnToMorning.SetFloat("_Blend", 0);
            _morningToDay.SetFloat("_Blend", 0);
            _dayToAfternoon.SetFloat("_Blend", 0);
            _afternoonToDusk.SetFloat("_Blend", 0);
            _duskToNight.SetFloat("_Blend", 0);
            _nightToDawn.SetFloat("_Blend", 0);
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
    private int _hours { get { return hours; } set { hours = value; OnHourChange(value); } }            // Call the method every hour
    
    private int days;                                                                                   // Total days passed in the game.
    private int _days { get { return days; } set { days = value; } }

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
    }

    void Update()
    {
        // Update the day night cycle
        UpdateCycle();

    }

    private void UpdateCycle()
    {
        // Increment minutes every second based on the cycle speed                                                              
        // NOTE: Since this is method is only called every frame, accuracy worsens over time, especially at lower cycle speeds. 
                                                                                                                        
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
            CycleSpeed.FiveMinutes => 360f / (5 * 60),
            CycleSpeed.TwentyMinutes => 360f / (20 * 60),
            CycleSpeed.OneHour => 360f / (1 * 60 * 60),
            CycleSpeed.ThreeHours => 360f / (3 * 60 * 60),
            CycleSpeed.SixHours => 360f / (6 * 60 * 60),
            CycleSpeed.TwelveHours => 360f / (12 * 60 * 60),
            CycleSpeed.OneDay => 360f / (24 * 60 * 60),
            _ => rotateSpeed
        };
        
        var transitionSpeed = currentCycleSpeed switch
        {
            CycleSpeed.FiveMinutes => 25f,
            CycleSpeed.TwentyMinutes => 100f,
            CycleSpeed.OneHour => 300f,
            CycleSpeed.ThreeHours => 900f,
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

    }

    // Adjusts the sun's intensity based on the current time of day
    private void ChangeSunIntensity(int value)
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

    public CycleSpeed GetSpeed()
    {
        return currentCycleSpeed;
    }

    private void OnDestroy()
    {
        SkyBoxTransition.ResetBlends();
    }

}
