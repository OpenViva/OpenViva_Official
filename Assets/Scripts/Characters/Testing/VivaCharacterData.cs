using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VivaCharacterData
{
    public int Version = 3;
    public string CharacterName;
    public string PrefabName;
    public int ScriptCount;

    // Script data
    public CharacterInfo Info = new();
    public List<PhysicsBoneData> PhysicsBones = new();
}

[Serializable]
public class CharacterInfo
{
    public string Name = "Fox Girl #9000";
    public string AuthorName = "Unknown";
    public int Version = 1;
    public string HeadBonePath;
    public int PersonalityType = 0;
    public VoicePack VoicePack;
    public string Description = "Return to hot spring when found.";
    public string Tags = "fox, girl";
    public Color CustomColor = Color.orange;
    public int Aux1;
    public int Aux2;
    public int Aux3;
    public string AuxString1;
    public string AuxString2;
    public string AuxString3;
}

[Serializable]
public class PhysicsBoneData
{
    public string GameObjectPath;
    public string BonePath;
    public string BoneName;

    public float Gravity = 2f;
    public float Damping = 0.05f;
    public float DistanceCompression = 0.5f;
    public float StiffnessValue = 0.15f;
    public bool UseStiffnessCurve = true;
    public float StiffnessCurveStart = 1f;
    public float StiffnessCurveEnd = 0.15f;
    public float VelocityAttenuation = 0.6f;
    public bool UseLimit = true;
    public float SpeedLimit = 3f;
}
