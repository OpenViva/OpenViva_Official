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
    [SerializeField] private float _dynamicAmbMaxVolume = 0.25f;
    private float _timer = 0;
    private bool _fading = false;
    private List<AudioSource> _allMorningAmbiance = new();
    private List<AudioSource> _allDayAmbiance = new();
    private List<AudioSource> _allNightAmbiance = new();
    private List<AudioSource> _fadeIn;
    private List<AudioSource> _fadeOut;

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
    }

    private void Update()
    {
        CalcCurrentStaticAmbVolume();

        if (!_fading || _fadeIn == null || _fadeOut == null) { return; }
        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            _fading = false;
            StopFadedOut();
            return;
        }
        float volume = _timer / _fadeDuration * _dynamicAmbMaxVolume;
        foreach (AudioSource aS in _fadeOut) { aS.volume = volume; }
        foreach (AudioSource aS in _fadeIn) { aS.volume = _dynamicAmbMaxVolume - volume; }
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

    public void OnTimeOfDayChanged(StaticAmbianceManager.TimeOfDay timeOfDay)
    {
        _fadeOut = _fadeIn;
        switch (timeOfDay)
        {
            case TimeOfDay.Morning: _fadeIn = _allMorningAmbiance; break;
            case TimeOfDay.Day: _fadeIn = _allDayAmbiance; break;
            case TimeOfDay.Night: _fadeIn = _allNightAmbiance; break;
        }
        PlayFadingIn();
        _fading = true;
        _timer = _fadeDuration;
    }

    private void PlayFadingIn()
    {
        foreach (AudioSource aS in _fadeIn) { aS.Play(); }
    }

    private void StopFadedOut()
    {
        foreach (AudioSource aS in _fadeOut) { aS.Stop(); }
    }
}
