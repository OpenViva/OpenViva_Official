using System;
using System.IO;
using System.Security.Cryptography;

public static class VivaFormat
{
    public const string VivaBytes = "VIVA";
    public const int CurrentVersion = 3;
    public const int MaxScriptCount = 8;

    public struct VivaHeader
    {
        public string VivaKey;
        public int Version;
        public int BundleSize;
        public int CharacterDataSize;
        public int ScriptCount;
        public uint Checksum;
    }

    public static void WriteHeader(BinaryWriter writer, VivaHeader header)
    {
        writer.Write(header.VivaKey);
        writer.Write(header.Version);
        writer.Write(header.BundleSize);
        writer.Write(header.CharacterDataSize);
        writer.Write(header.ScriptCount);
        writer.Write(header.Checksum);
    }

    public static VivaHeader ReadHeader(BinaryReader reader)
    {
        VivaHeader header = new()
        {
            VivaKey = reader.ReadString(),
            Version = reader.ReadInt32(),
            BundleSize = reader.ReadInt32(),
            CharacterDataSize = reader.ReadInt32(),
            ScriptCount = reader.ReadInt32(),
            Checksum = reader.ReadUInt32(),
        };

        return header;
    }

    public static uint CalculateChecksum(byte[] data)
    {
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(data);

        return BitConverter.ToUInt32(hash, 0);
    }
}
