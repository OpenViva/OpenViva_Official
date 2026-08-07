using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StaticAmbianceCollection : MonoBehaviour
{
    public float DelayBetweenPlays = 10f;
    private List<AudioSource> _collection;

    private void Awake()
    {
        _collection = GetComponentsInChildren<AudioSource>().ToList();
    }

    public void PlayAll() { foreach (AudioSource aS in _collection) { aS.Play(); } }
}
