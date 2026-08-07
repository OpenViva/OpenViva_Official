using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;

public class DynamicAmbianceController : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup _dynamicSFXGroup;
    private const string EXPOSED_PARAM_NAME = "DynamicAmbVolume";

    [SerializeField] private GameObject _outdoorAmbiance;
    [SerializeField] private GameObject _houseAmbiance;

    private List<AudioSource> _allOutdoorAmbiance;
    private List<AudioSource> _allHouseAmbiance;

    [SerializeField] private Transform _playerKB;
    [SerializeField] private List<Transform> _allAreaAmbiance;

    private void Awake()
    {
        _allOutdoorAmbiance = _outdoorAmbiance.GetComponentsInChildren<AudioSource>().ToList();
        _allHouseAmbiance = _houseAmbiance.GetComponentsInChildren<AudioSource>().ToList();
    }

    private void Update()
    {
        CalcCurrentStaticAmbVolume();
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
        float volume = rollOffCurve.Evaluate(normalizedDistance) * -10;

        _dynamicSFXGroup.audioMixer.SetFloat(EXPOSED_PARAM_NAME, volume);
    }
}
