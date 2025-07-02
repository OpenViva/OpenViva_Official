using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New/SpawnablePrefabs")]
public class SpawnablePrefabs : ScriptableObject
{
    public GameObject[] prefabsToSpawn;
}
