using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StaticAmbianceManager : MonoBehaviour
{
    public static StaticAmbianceManager Instance;

    private List<StaticAmbianceCollection> _ambianceCollections;
    private float[] _playPoints;
    private float _time = 0;

    [SerializeField] private float _fadeDuration;
    [SerializeField] private float _areaAmbMaxVolume = 0.25f;
    private float _fadeEndPoint;
    private bool _fading = false;
    [SerializeField] private List<AudioSource> _allMorningAmbiance;
    [SerializeField] private List<AudioSource> _allDayAmbiance;
    [SerializeField] private List<AudioSource> _allNightAmbiance;
    private List<AudioSource> _fadeIn;
    private List<AudioSource> _fadeOut;

    public enum TimeOfDay
    {
        Morning,
        Day,
        Night
    }
    public TimeOfDay CurrentTimeOfDay
    {
        get { return _currentTimeOfDay; }
        set
        {
            if (value == _currentTimeOfDay) { return; }
            _currentTimeOfDay = value;
            OnTimeOfDayChanged();
            DynamicAmbianceController.Instance.OnTimeOfDayChanged(_currentTimeOfDay);
        }
    }
    private TimeOfDay _currentTimeOfDay = TimeOfDay.Day;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        _ambianceCollections = FindObjectsByType<StaticAmbianceCollection>(FindObjectsSortMode.None).ToList();
        _playPoints = new float[_ambianceCollections.Count];
    }

    private void Start()
    {
        for (int i = 0; i < _ambianceCollections.Count; i++) { SetPlayPoint(i); }
        _fadeIn = _allDayAmbiance;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        if (_time >= float.MaxValue) { _time = 0; }
        TryPlayAmb();

        // Area Ambiance Transition (must be done last in Update)
        if (!_fading || _fadeIn == null || _fadeOut == null) { return; }
        if (_time >= _fadeEndPoint)
        {
            _fading = false;
            StopFadedOut();
            return;
        }
        float volume = (_fadeEndPoint - _time) / _fadeDuration * _areaAmbMaxVolume;
        foreach (AudioSource aS in _fadeOut) { aS.volume = volume; }
        foreach (AudioSource aS in _fadeIn) { aS.volume = _areaAmbMaxVolume - volume; }
    }

    private void SetPlayPoint(int index)
    {
        float specifiedDelay = _ambianceCollections[index].DelayBetweenPlays;
        float randomizedTime = Random.Range(specifiedDelay - (specifiedDelay * 0.2f), specifiedDelay + (specifiedDelay * 0.2f));
        _playPoints[index] = _time + randomizedTime;
    }

    private void TryPlayAmb()
    {
        for (int i = 0; i < _ambianceCollections.Count; i++)
        {
            if (_time >= _playPoints[i])
            {
                _ambianceCollections[i].PlayAll();
                SetPlayPoint(i);
                // Debug.Log($"Playing audio source {_ambianceCollections[i].gameObject.name}");
            }
        }
    }

    private void OnTimeOfDayChanged()
    {
        _fadeOut = _fadeIn;
        switch (_currentTimeOfDay)
        {
            case TimeOfDay.Morning: _fadeIn = _allMorningAmbiance; break;
            case TimeOfDay.Day: _fadeIn = _allDayAmbiance; break;
            case TimeOfDay.Night: _fadeIn = _allNightAmbiance; break;
        }
        PlayFadingIn();
        _fading = true;
        _fadeEndPoint = _time + _fadeDuration;
        // Debug.Log($"Switching to '{_currentTimeOfDay}' ambiance");
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
