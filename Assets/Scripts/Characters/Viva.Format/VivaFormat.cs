using System;
using System.IO;
using System.Security.Cryptography;

public static class VivaFormat
{
    public const string VivaBytes = "VIVA";
    public const int CurrentVersion = 1;
    public const int MaxScriptCount = 8;

    public struct Header
    {
        public string VivaKey;
        public int Version;
        public long BundleSize;
        public long MetadataSize;
        public int ScriptCount;
        public uint Checksum;
    }

    public static uint CalculateChecksum(byte[] data)
    {
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(data);

        return BitConverter.ToUInt32(hash, 0);
    }

    public static void WriteHeader(BinaryWriter writer, Header header)
    {
        writer.Write(header.VivaKey);
        writer.Write(header.Version);
        writer.Write(header.BundleSize);
        writer.Write(header.MetadataSize);
        writer.Write(header.ScriptCount);
        writer.Write(header.Checksum);
    }

    public static Header ReadHeader(BinaryReader reader)
    {
        Header header = new()
        {
            VivaKey = reader.ReadString(),
            Version = reader.ReadInt32(),
            BundleSize = reader.ReadInt64(),
            MetadataSize = reader.ReadInt64(),
            ScriptCount = reader.ReadInt32(),
            Checksum = reader.ReadUInt32(),
        };

        return header;
    }
}
