using System.Collections;
using UnityEngine;

public class SkyboxTransition : MonoBehaviour
{
    // Skybox materials for different times of day
    [SerializeField] private Material _dawnToMorning;
    [SerializeField] private Material _morningToDay;
    [SerializeField] private Material _dayToAfternoon;
    [SerializeField] private Material _afternoonToDusk;
    [SerializeField] private Material _duskToNight;
    [SerializeField] private Material _nightToDawn;

    private DayNightCycle _time; // Script to track how many minutes have passed in the day and the cycle speed

    private int _minutes; // How many minutes have passed in the day
    private CycleSpeeds.Speed _speed; // The set speed of the day-night cycle
    private float _transitionSpeed; // How fast the skybox transitions

    private void Start()
    {
        // Get reference to DayNightCycle script and retrieve current minutes and speed
        _time = GetComponent<DayNightCycle>();
        _minutes = _time.GetMinutes();
        _speed = _time.GetSpeed();

        // Determine increment and skybox transition speed based on selected cycle speed
        switch (_speed)
        {
            case CycleSpeeds.Speed.FiveMinutes:
                _transitionSpeed = 25f;
                break;

            case CycleSpeeds.Speed.TwentyMinutes:
                _transitionSpeed = 100f;
                break;

            case CycleSpeeds.Speed.OneHour:
                _transitionSpeed = 300f;
                break;

            case CycleSpeeds.Speed.ThreeHours:
                _transitionSpeed = 900f;
                break;

            case CycleSpeeds.Speed.SixHours:
                _transitionSpeed = 1800f;
                break;

            case CycleSpeeds.Speed.TwelveHours:
                _transitionSpeed = 3600f;
                break;

            case CycleSpeeds.Speed.OneDay:
                _transitionSpeed = 7200f;
                break;
        }
    }

    private void Update()
    {
        _minutes = _time.GetMinutes();

        // Transition to next skybox phase as the day progresses
        switch (_minutes)
        {
            case 300:
                StartCoroutine(NightToDawn());
                break;
            case 420:
                StartCoroutine(DawnToMorning());
                break;
            case 600:
                StartCoroutine(MorningToDay());
                break;
            case 840:
                StartCoroutine(DayToAfternoon());
                break;
            case 1020:
                StartCoroutine(AfternoonToDusk());
                break;
            case 1140:
                StartCoroutine(DuskToNight());
                break;
        }
    }

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
}
