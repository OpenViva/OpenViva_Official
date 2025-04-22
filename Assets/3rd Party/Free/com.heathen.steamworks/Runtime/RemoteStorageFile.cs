#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;
using CloudAPI = Heathen.SteamworksIntegration.API.RemoteStorage.Client;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct RemoteStorageFile : IEquatable<RemoteStorageFile>
    {
        public string name;
        public int size;
        public DateTime timestamp;

        public bool Equals(RemoteStorageFile other)
        {
            return name.Equals(other.name) && size.Equals(other.size) && timestamp.Equals(other.timestamp);
        }

        public override bool Equals(object obj)
        {
            if(obj.GetType() == typeof(RemoteStorageFile))
            {
                return Equals((RemoteStorageFile)obj);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode() ^ size.GetHashCode() ^ timestamp.GetHashCode();
        }

        public byte[] Data => CloudAPI.FileRead(name);

        public override string ToString()
        {
            return CloudAPI.FileReadString(name, System.Text.Encoding.UTF8);
        }

        public string ToString(System.Text.Encoding encoding)
        {
            return CloudAPI.FileReadString(name, encoding);
        }

        public T ToJson<T>()
        {
            return CloudAPI.FileReadJson<T>(name, System.Text.Encoding.UTF8);
        }

        public T ToJson<T>(System.Text.Encoding encoding)
        {
            return CloudAPI.FileReadJson<T>(name, encoding);
        }

        public static bool operator ==(RemoteStorageFile l, RemoteStorageFile r) => l.Equals(r);
        public static bool operator !=(RemoteStorageFile l, RemoteStorageFile r) => !l.Equals(r);
    }
}
#endif