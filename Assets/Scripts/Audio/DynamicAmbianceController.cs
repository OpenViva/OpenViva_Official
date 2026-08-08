using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using static StaticAmbianceManager;

public class DynamicAmbianceController : MonoBehaviour
{
    public static DynamicAmbianceController Instance;

    [SerializeField] AudioMixerGroup _dynamicSFXGroup;
    private const string EXPOSED_PARAM_NAME = "DynamicAmbVolume";

    [SerializeField] private GameObject _outdoorAmbiance;
    [SerializeField] private GameObject _houseAmbiance;

    private List<AudioSource> _outdoorAmbList = new();
    private List<AudioSource> _houseAmbList = new();
    private List<AudioSource> _allAmbList = new();

    [SerializeField] private Transform _playerKB;
    [SerializeField] private List<Transform> _allAmbPositions;

    [SerializeField] private float _fadeDuration;
    [SerializeField] private float _dynamicAmbMaxVolume = 0.5f;

    private List<AudioSource> _morningAmbList = new();
    private List<AudioSource> _dayAmbList = new();
    private List<AudioSource> _nightAmbList = new();

    private bool _timeTransitioning;
    private float _timeTransitionTimer;

    private TimeOfDay _currentTimeOfDay = TimeOfDay.Day;
    private TimeOfDay _targetTimeOfDay = TimeOfDay.Day;

    private Dictionary<AudioSource, float> _timeWeights = new();
    private Dictionary<AudioSource, float> _timeTransitionFrom = new();
    private Dictionary<AudioSource, float> _timeTransitionTo = new();

    public enum IndoorArea
    {
        None, // Outdoors
        House
    }

    private IndoorArea _areaPlayerIsIn = IndoorArea.House;
    private IndoorArea _areaCurrentlyPlaying = IndoorArea.House;
    private IndoorArea _areaTarget = IndoorArea.House;

    private bool _areaTransitioning;
    private float _areaTransitionTimer;

    private Dictionary<AudioSource, float> _areaWeights = new();
    private Dictionary<AudioSource, float> _areaTransitionFrom = new();
    private Dictionary<AudioSource, float> _areaTransitionTo = new();

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        _outdoorAmbList = _outdoorAmbiance.GetComponentsInChildren<AudioSource>().ToList();
        _houseAmbList = _houseAmbiance.GetComponentsInChildren<AudioSource>().ToList();
        _allAmbList.AddRange(_outdoorAmbList);
        _allAmbList.AddRange(_houseAmbList);

        foreach (AudioSource aS in _allAmbList) { PopulateTimedLists(aS); }

        InitAudio();
    }

    private void Start()
    {
        UpdateVolumes();
    }

    private void Update()
    {
        CalcCurrentStaticAmbVolume();
        TryTransitionTime();
        TryTransitionArea();
        UpdateVolumes();
    }

    private void PopulateTimedLists(AudioSource aS)
    {
        if (aS.gameObject.name.Contains("Morning")) { _morningAmbList.Add(aS); }
        if (aS.gameObject.name.Contains("Day")) { _dayAmbList.Add(aS); }
        if (aS.gameObject.name.Contains("Night")) { _nightAmbList.Add(aS); }
    }

    private void InitAudio()
    {
        foreach (AudioSource aS in _allAmbList)
        {
            if (_dayAmbList.Contains(aS)) { _timeWeights[aS] = 1f; }
            else { _timeWeights[aS] = 0f; }

            if (_houseAmbList.Contains(aS)) { _areaWeights[aS] = 1f; }
            else { _areaWeights[aS] = 0f; }

            aS.volume = 0f;
            aS.Stop();
        }

        foreach (AudioSource aS in _dayAmbList)
        {
            if (_houseAmbList.Contains(aS)) { aS.Play(); }
        }
    }

    private void UpdateVolumes()
    {
        foreach (AudioSource aS in _allAmbList)
        {
            aS.volume = _timeWeights[aS] * _areaWeights[aS] * _dynamicAmbMaxVolume;
        }
    }

    private void CalcCurrentStaticAmbVolume()
    {
        Transform player;
        if (Globals.isDesktopMode) { player = _playerKB; }
        else { player = _playerKB; } // Will change later

        float distanceFromNearestAreaAmb = Mathf.Infinity;
        AudioSource closestAudioSource = _allAmbList[0].gameObject.GetComponent<AudioSource>();
        foreach (Transform t in _allAmbPositions)
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

    public void OnTimeOfDayChanged(TimeOfDay time)
    {
        if (_targetTimeOfDay == time && !_timeTransitioning) { return; }
        _targetTimeOfDay = time;

        foreach (AudioSource aS in _allAmbList)
        {
            float weight;
            if (_timeWeights.TryGetValue(aS, out float value)) { weight = value; }
            else { weight = 0f; }
            _timeTransitionFrom[aS] = weight;

            _timeTransitionTo[aS] = SetTimeWeightTarget(aS, time);
        }

        PlayTimeTransitionSources();
        _timeTransitionTimer = 0f;
        _timeTransitioning = true;
    }

    private void TryTransitionTime()
    {
        if (!_timeTransitioning) { return; }

        _timeTransitionTimer += Time.deltaTime;

        float transitionProgress = Mathf.Clamp01(_timeTransitionTimer / _fadeDuration);
        foreach (AudioSource aS in _allAmbList) 
        {
            _timeWeights[aS] = Mathf.Lerp(_timeTransitionFrom[aS], _timeTransitionTo[aS], transitionProgress);
        }

        if (transitionProgress >= 1f)
        {
            _timeTransitioning = false;
            _currentTimeOfDay = _targetTimeOfDay;

            foreach (AudioSource aS in _allAmbList) { _timeWeights[aS] = _timeTransitionTo[aS]; }
        }
    }

    private float SetTimeWeightTarget(AudioSource aS, TimeOfDay time)
    {
        switch (time)
        {
            case TimeOfDay.Morning:
                if (_morningAmbList.Contains(aS)) { return 1f; }
                else { return 0f; }

            case TimeOfDay.Day:
                if (_dayAmbList.Contains(aS)) { return 1f; }
                else { return 0f; }

            case TimeOfDay.Night:
                if (_nightAmbList.Contains(aS)) { return 1f; }
                else { return 0f; }

            default: return 0f;
        }
    }

    private void PlayTimeTransitionSources()
    {
        foreach (AudioSource aS in _allAmbList)
        {
            if ((_timeTransitionFrom[aS] > 0f || _timeTransitionTo[aS] > 0f) && !aS.isPlaying)
            {
                aS.Play();
            }
        }
    }

    public IEnumerator OnAreaChanged(IndoorArea area)
    {
        if (_areaPlayerIsIn == area) { yield break; }

        _areaPlayerIsIn = area;
        _areaTarget = area;

        foreach (AudioSource aS in _allAmbList)
        {
            float weight;
            if (_areaWeights.TryGetValue(aS, out float value)) { weight = value; }
            else { weight = 0f; }
            _areaTransitionFrom[aS] = weight;

            _areaTransitionTo[aS] = SetAreaWeightTarget(aS, _areaTarget);
        }

        PlayAreaTransitionSources();
        _areaTransitionTimer = 0f;
        _areaTransitioning = true;

        yield break;
    }

    private void TryTransitionArea()
    {
        if (!_areaTransitioning) { return; }

        _areaTransitionTimer += Time.deltaTime;

        float transitionProgress = Mathf.Clamp01(_areaTransitionTimer / 1f);
        foreach (AudioSource aS in _allAmbList)
        {
            _areaWeights[aS] = Mathf.Lerp(_areaTransitionFrom[aS], _areaTransitionTo[aS], transitionProgress);
        }

        if (transitionProgress >= 1f)
        {
            _areaTransitioning = false;
            _areaCurrentlyPlaying = _areaTarget;

            foreach (AudioSource aS in _allAmbList) { _areaWeights[aS] = _areaTransitionTo [aS]; }

            StopSources();
        }
    }

    private float SetAreaWeightTarget(AudioSource aS, IndoorArea area)
    {
        switch (area)
        {
            case IndoorArea.House:
                if (_houseAmbList.Contains(aS)) { return 1f; }
                else { return 0f; }

            default:
                if (_outdoorAmbList.Contains(aS)) { return 1f; }
                else { return 0f; }
        }
    }

    private void PlayAreaTransitionSources()
    {
        foreach (AudioSource aS in _allAmbList)
        {
            if ((_areaTransitionFrom[aS] > 0f || _areaTransitionTo[aS] > 0f) && !aS.isPlaying)
            {
                aS.Play();
            }
        }
    }

    private void StopSources()
    {
        foreach (AudioSource aS in _allAmbList)
        {
            if (_timeWeights[aS] <= 0f && _areaWeights[aS] <= 0 && aS.isPlaying) { aS.Stop(); }
        }
    }
}
