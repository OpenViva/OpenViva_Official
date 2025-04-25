#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct NumericFilter : IEquatable<NumericFilter>, IComparable<NumericFilter>
    {
        public string key;
        public int value;
        public ELobbyComparison comparison;

        public readonly int CompareTo(NumericFilter other)
        {
            return key.CompareTo(other.key) == 0 ? value.CompareTo(other.value) : key.CompareTo(other.key);
        }

        public readonly bool Equals(NumericFilter other)
        {
            return key.Equals(other.key) && value.Equals(other.value);
        }

        public override readonly bool Equals(object obj)
        {
            if (obj.GetType() == typeof(NumericFilter)) return this == (NumericFilter)obj;
            else if (obj.GetType() == typeof(MetadataTemplate))
            {
                MetadataTemplate metadataTemplate = (MetadataTemplate)obj;
                if (int.TryParse(metadataTemplate.value, out var nValue))
                    return key == metadataTemplate.key && value == nValue;
                else return false;
            }
            return false;
        }

        public override readonly int GetHashCode()
        {
            return key.GetHashCode() ^ value.GetHashCode();
        }

        public static bool operator ==(NumericFilter l, NumericFilter r) => l.key == r.key && l.value == r.value && l.comparison == r.comparison;
        public static bool operator !=(NumericFilter l, NumericFilter r) => l.key != r.key || l.value != r.value || l.comparison != r.comparison;
    }
}
#endif