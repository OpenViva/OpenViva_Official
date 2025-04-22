#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct LobbyChatMsg
    {
        public LobbyData lobby;
        public EChatEntryType type;
        public UserData sender;
        public byte[] data;
        public DateTime receivedTime;
        public string Message => ToString();
        public override string ToString()
        {
            return System.Text.Encoding.UTF8.GetString(data);
        }

        public T FromJson<T>()
        {
            return UnityEngine.JsonUtility.FromJson<T>(ToString());
        }

        public bool TryFromJson<T>(out T result)
        {
            try
            {
                result = UnityEngine.JsonUtility.FromJson<T>(ToString());
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}
#endif