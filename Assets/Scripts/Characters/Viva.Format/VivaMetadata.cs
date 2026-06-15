using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class VivaMetadata
{
    public int Version = 1;
    public List<ScriptData> Scripts = new();

    [Serializable]
    public class ScriptData
    {
        public string TypeName;
        public string GameObjectPath;
        public byte[] SerializedData;
        public int DataSize;
        public uint Checksum;
    }

    public void AddScriptData(string typeName, GameObject targetGameObject, byte[] serializedData)
    {
        if (Scripts.Count >= VivaFormat.MaxScriptCount)
        {
            Debug.LogError("[VivaMetadata] Maximum script count reached!");
            return;
        }

        string gameOebjectPath = GenerateGameObjectPath(targetGameObject);

        ScriptData scriptData = new()
        {
            TypeName = typeName,
            GameObjectPath = gameOebjectPath,
            SerializedData = serializedData,
            DataSize = serializedData.Length,
            Checksum = VivaFormat.CalculateChecksum(serializedData)
        };

        Scripts.Add(scriptData);
    }

    private string GenerateGameObjectPath(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return "";
        }

        List<string> pathParts = new List<string>();
        Transform current = gameObject.transform;

        while (current != null)
        {
            pathParts.Insert(0, current.name);
            current = current.parent;
        }

        return string.Join("/", pathParts);
    }

    public byte[] Serialize()
    {
        using MemoryStream memoryStream = new();
        using (BinaryWriter binaryWriter = new(memoryStream))
        {
            binaryWriter.Write(Version);
            binaryWriter.Write(Scripts.Count);

            foreach (ScriptData script in Scripts)
            {
                binaryWriter.Write(script.TypeName);
                binaryWriter.Write(script.GameObjectPath);
                binaryWriter.Write(script.DataSize);
                binaryWriter.Write(script.Checksum);
                binaryWriter.Write(script.SerializedData);
            }
        }

        return memoryStream.ToArray();
    }

    public void Deserialize(byte[] data)
    {
        using MemoryStream memoryStream = new(data);
        using BinaryReader binaryReader = new(memoryStream);

        Version = binaryReader.ReadInt32();
        int scriptCount = binaryReader.ReadInt32();

        Scripts.Clear();

        for (int i = 0; i < scriptCount; i++)
        {
            ScriptData script = new()
            {
                TypeName = binaryReader.ReadString(),
                GameObjectPath = binaryReader.ReadString(),
                DataSize = binaryReader.ReadInt32(),
                Checksum = binaryReader.ReadUInt32(),
            };

            script.SerializedData = binaryReader.ReadBytes(script.DataSize);
            Scripts.Add(script);
        }
    }

    public bool ValidateChecksums()
    {
        foreach (ScriptData script in Scripts)
        {
            if (script.SerializedData == null) continue;

            uint calculatedChecksum = VivaFormat.CalculateChecksum(script.SerializedData);
            if (calculatedChecksum != script.Checksum)
            {
                Debug.LogError($"[VivaMetadata] Checksum mismatch for script: {script.TypeName}");
                return false;
            }
        }

        return true;
    }
}
