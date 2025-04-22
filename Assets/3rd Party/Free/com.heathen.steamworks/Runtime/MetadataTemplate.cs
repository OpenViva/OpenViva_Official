#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct MetadataTemplate : IEquatable<MetadataTemplate>
    {
        /// <summary>
        /// The key or field name to be used. names will not be duplicated, if you add another field of the same name it will overwrite, not duplicate
        /// </summary>
        [UnityEngine.Tooltip("The key or field name to be used. names will not be duplicated, if you add another field of the same name it will overwrite, not duplicate")]
        public string key;
        /// <summary>
        /// The value of the field to be applied, empty values are ignored
        /// </summary>
        [UnityEngine.Tooltip("The value of the field to be applied, empty values are ignored")]
        public string value;

        public readonly bool Equals(MetadataTemplate other)
        {
            return key == other.key && value == other.value;
        }

        public override readonly bool Equals(object obj)
        {
            if (obj == null) return false;
            else if (obj.GetType() != typeof(MetadataTemplate)) return false;
            else return Equals((MetadataTemplate) obj);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(key, value);
        }

        public override readonly string ToString()
        {
            return key + ":" + value;
        }

        public static bool operator ==(MetadataTemplate l, MetadataTemplate r) => l.key == r.key && l.value == r.value;
        public static bool operator !=(MetadataTemplate l, MetadataTemplate r) => l.key != r.key && l.value != r.value;
    }

    [Obsolete("Please use MetadataTemplate")]
    public struct MetadataTempalate : IEquatable<MetadataTemplate>
    {
        public string key;
        public string value;

        public bool Equals(MetadataTemplate other)
        {
            return key == other.key && value == other.value;
        }

        public static implicit operator MetadataTempalate(MetadataTemplate other) => new() { key = other.key, value = other.value };
        public static implicit operator MetadataTemplate(MetadataTempalate other) => new() { key = other.key, value = other.value };
    }
}
#endif