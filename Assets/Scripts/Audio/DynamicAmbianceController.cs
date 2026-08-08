using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using static StaticAmbianceManager;

public class DynamicAmbianceController : MonoBehaviour
{
    public static DynamicAmbianceController Instance;

    [SerializeField] private AudioMixerGroup _dynamicSFXGroup;
    private const string EXPOSED_PARAM_NAME = "DynamicAmbVolume";

    [SerializeField] private GameObject _outdoorAmbiance;
    [SerializeField] private GameObject _houseAmbiance;

    private List<AudioSource> _allOutdoorAmbiance;
    private List<AudioSource> _allHouseAmbiance;

    [SerializeField] private Transform _playerKB;
    [SerializeField] private List<Transform> _allAreaAmbiance;

    [SerializeField] private float _fadeDuration;
    [SerializeField] private float _dynamicAmbMaxVolume = 0.5f;
    private float _fadeTimer = 0;
    private bool _fading = false;
    private List<AudioSource> _allMorningAmbiance = new();
    private List<AudioSource> _allDayAmbiance = new();
    private List<AudioSource> _allNightAmbiance = new();
    private List<AudioSource> _fadeIn;
    private List<AudioSource> _fadeOut;

    private float _changeTimer = 0;
    private List<AudioSource> _changingTo;
    private List<AudioSource> _changingFrom;

    public enum IndoorArea
    {
        None, // Outdoors
        House
    }
    private IndoorArea _areaPlayerIsIn = IndoorArea.House;
    private IndoorArea _areaAmbiancePlaying = IndoorArea.House;
    private IndoorArea _areaTransitioningTo = IndoorArea.None;

    private TimeOfDay _currentTimeOfDay = TimeOfDay.Day;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        _allOutdoorAmbiance = _outdoorAmbiance.GetComponentsInChildren<AudioSource>().ToList();
        _allHouseAmbiance = _houseAmbiance.GetComponentsInChildren<AudioSource>().ToList();

        foreach (AudioSource aS in _allOutdoorAmbiance) { PopulateTimedLists(aS); }
        foreach (AudioSource aS in _allHouseAmbiance) { PopulateTimedLists(aS); }
    }

    private void Start()
    {
        _fadeIn = _allDayAmbiance;
        _changingTo = _allHouseAmbiance;
    }

    private void Update()
    {
        CalcCurrentStaticAmbVolume();
        TryPerformFade();
        TryPerformChange();
    }

    private void CalcCurrentStaticAmbVolume()
    {
        Transform player;
        if (Globals.isDesktopMode) { player = _playerKB; }
        else { player = _playerKB; } // Will change later

        float distanceFromNearestAreaAmb = Mathf.Infinity;
        AudioSource closestAudioSource = _allAreaAmbiance[0].gameObject.GetComponent<AudioSource>();
        foreach (Transform t in _allAreaAmbiance)
        {
            float temp = Vector3.Distance(t.position, player.position);
            if (temp < distanceFromNearestAreaAmb)
            {
                distanceFromNearestAreaAmb = temp;
                closestAudioSource = t.gameObject.GetComponent<AudioSource>();
            }
        }

        float normalizedDistance = Mathf.InverseLerp(closestAudioSource.minDistance, closestAudioSource.maxDistance, distanceFromNearestAreaAmb);
        AnimationCurve rollOffCurve = closestAudioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
        float volume = rollOffCurve.Evaluate(normalizedDistance) * -15;

        _dynamicSFXGroup.audioMixer.SetFloat(EXPOSED_PARAM_NAME, volume);
    }

    private void PopulateTimedLists(AudioSource aS)
    {
        if (aS.gameObject.name.Contains("Morning")) { _allMorningAmbiance.Add(aS); }
        if (aS.gameObject.name.Contains("Day")) { _allDayAmbiance.Add(aS); }
        if (aS.gameObject.name.Contains("Night")) { _allNightAmbiance.Add(aS); }
    }

    public void OnTimeOfDayChanged(TimeOfDay timeOfDay)
    {
        _fadeOut = _fadeIn;
        switch (timeOfDay)
        {
            case TimeOfDay.Morning: _fadeIn = _allMorningAmbiance; break;
            case TimeOfDay.Day: _fadeIn = _allDayAmbiance; break;
            case TimeOfDay.Night: _fadeIn = _allNightAmbiance; break;
        }
        _currentTimeOfDay = timeOfDay;
        PlayFadingIn();
        _fading = true;
        _fadeTimer = _fadeDuration;
    }

    private void PlayFadingIn()
    {
        foreach (AudioSource aS in _fadeIn) 
        { 
            switch (_areaAmbiancePlaying)
            {
                case IndoorArea.House: if (_allHouseAmbiance.Contains(aS)) { aS.Play(); } break;
                default: if (_allOutdoorAmbiance.Contains(aS)) { aS.Play(); } break;
            }
        }
    }

    private void StopFadedOut()
    {
        foreach (AudioSource aS in _fadeOut) { aS.Stop(); }
    }

    private void TryPerformFade()
    {
        if (!_fading || _fadeIn == null || _fadeOut == null) { return; }
        _fadeTimer -= Time.deltaTime;
        if (_fadeTimer <= 0)
        {
            _fading = false;
            StopFadedOut();
            return;
        }
        float volume = _fadeTimer / _fadeDuration * _dynamicAmbMaxVolume;
        foreach (AudioSource aS in _fadeOut) { aS.volume = volume; }
        foreach (AudioSource aS in _fadeIn) { aS.volume = _dynamicAmbMaxVolume - volume; }
    }

    public IEnumerator OnAreaChanged(IndoorArea newArea)
    {
        if (_areaPlayerIsIn == newArea) { yield break; }
        _areaPlayerIsIn = newArea;

        if (_changeTimer > 0) { yield return new WaitUntil(() => _changeTimer <= 0); }
        if (_areaAmbiancePlaying == _areaPlayerIsIn) { yield break; }
        _changeTimer = 1f;
        _areaTransitioningTo = _areaPlayerIsIn;

        _changingFrom = _changingTo;
        switch (_areaTransitioningTo)
        {
            case IndoorArea.House: _changingTo = _allHouseAmbiance; break;
            default: _changingTo = _allOutdoorAmbiance; break;
        }
        PlayChangingTo();
    }

    private void PlayChangingTo()
    {
        foreach (AudioSource aS in _changingTo) 
        {
            switch (_currentTimeOfDay)
            {
                case TimeOfDay.Morning: if (_allMorningAmbiance.Contains(aS)) { aS.Play(); }; break;
                case TimeOfDay.Day: if (_allDayAmbiance.Contains(aS)) { aS.Play(); }; break;
                case TimeOfDay.Night: if (_allNightAmbiance.Contains(aS)) { aS.Play(); }; break;
            }
        }
    }

    private void StopChangedFrom()
    {
        foreach (AudioSource aS in _changingFrom) { aS.Stop(); }
    }

    private void TryPerformChange()
    {
        if (_changeTimer <= 0) { return; }

        _changeTimer -= Time.deltaTime;
        if (_changeTimer <= 0)
        {
            _areaAmbiancePlaying = _areaTransitioningTo;
            StopChangedFrom();
        }

        float volume = _changeTimer * _dynamicAmbMaxVolume;
        foreach (AudioSource aS in _changingFrom) { aS.volume = volume; }
        foreach (AudioSource aS in _changingTo) 
        { 
            switch (_currentTimeOfDay)
            {
                case TimeOfDay.Morning: 
                    if (_allMorningAmbiance.Contains(aS)) { aS.volume = _dynamicAmbMaxVolume - volume; }
                    break;

                case TimeOfDay.Day:
                    if (_allDayAmbiance.Contains(aS)) { aS.volume = _dynamicAmbMaxVolume - volume; }
                    break;

                case TimeOfDay.Night:
                    if (_allNightAmbiance.Contains(aS)) { aS.volume = _dynamicAmbMaxVolume - volume; }
                    break;
            }
        }
    }
}
