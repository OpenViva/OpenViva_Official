#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct NearFilter : IEquatable<NearFilter>, IComparable<NearFilter>
    {
        public string key;
        public int value;

        public readonly int CompareTo(NearFilter other)
        {
            return key.CompareTo(other.key) == 0 ? value.CompareTo(other.value) : key.CompareTo(other.key);
        }

        public readonly bool Equals(NearFilter other)
        {
            return key.Equals(other.key) && value.Equals(other.value);
        }

        public override readonly bool Equals(object obj)
        {
            if (obj.GetType() == typeof(NearFilter)) return this == (NearFilter)obj;
            else if (obj.GetType() == typeof(MetadataTemplate))
            {
                MetadataTemplate metadataTemplate = (MetadataTemplate)obj;
                if(int.TryParse(metadataTemplate.value, out var nValue))
                    return key == metadataTemplate.key && value == nValue;
                else return false;
            }
            return false;
        }

        public override readonly int GetHashCode()
        {
            return key.GetHashCode() ^ value.GetHashCode();
        }

        public static bool operator ==(NearFilter l, NearFilter r) => l.key == r.key && l.value == r.value;
        public static bool operator !=(NearFilter l, NearFilter r) => l.key != r.key || l.value != r.value;
    }
}
#endif