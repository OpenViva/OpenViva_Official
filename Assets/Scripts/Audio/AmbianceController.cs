using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AmbianceController : MonoBehaviour
{
    private List<AmbianceCollection> _ambianceCollections;
    private float[] _playPoints;
    private float _time = 0;

    private void Awake()
    {
        _ambianceCollections = FindObjectsByType<AmbianceCollection>(FindObjectsSortMode.None).ToList();
        _playPoints = new float[_ambianceCollections.Count];
    }

    private void Start()
    {
        for (int i = 0; i < _ambianceCollections.Count; i++) { SetPlayPoint(i); }
    }

    private void Update()
    {
        _time += Time.deltaTime;
        if (_time >= float.MaxValue) { _time = 0; }
        TryPlayAmb();
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
                Debug.Log($"Playing audio source {_ambianceCollections[i].gameObject.name}");
            }
        }
    }
}
