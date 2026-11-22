using System;
using UnityEngine;
using static Viva.CharacterFramework.CharacterEnums;

namespace Viva.CharacterFramework
{
    /// <summary>
    /// A personality trait.
    /// All modifiers are percentage changes, 0 means unchanged.
    /// Modifiers can be restricted to situations with <see cref="ConditionMask">
    /// (for example: Workaholic - Energy decay -25% while working)
    /// </summary>
    [CreateAssetMenu(fileName = "Trait_New", menuName = "Character Framework/Trait", order = 10)]
    public sealed class TraitSO : ScriptableObject
    {
        [Serializable]
        public struct NeedModifier
        {
            public NeedType need;

            [Tooltip("+50 = decays 50% faster, -30 = decays 30% slower")]
            [Range(-100, 300f)] public float decayChangePercent;

            [Tooltip("Only applies when ALL ticked conditions are active. None = always")]
            public ConditionMask requiredConditions;
        }

        [Serializable]
        public struct StatModifier
        {
            public CharacterStat stat;

            [Tooltip("+30 = 30% higher, -10 = 10% lower")]
            [Range(-100, 300f)] public float changePercent;

            [Tooltip("Only applies when ALL ticked conditions are active. None = always")]
            public ConditionMask requiredConditions;
        }

        [Serializable]
        public struct InteractionModifier
        {
            public InteractionType interaction;
            [Range(-100, 300f)] public float gainChangePercent;
            [Range(-100, 300f)] public float lossChangePercent;
        }

        [Header("Identity")]
        [SerializeField] private string traitId;
        [SerializeField] private string displayName = "New Trait";
        [SerializeField, TextArea(2, 4)] private string description;
        private Sprite icon; // TODO: Add icons for traits if needed
        [SerializeField] private TraitSO[] conflictingTraits = Array.Empty<TraitSO>();

        [Header("Needs")]
        [SerializeField] private NeedModifier[] needModifiers = Array.Empty<NeedModifier>();

        [Header("Stats")]
        [SerializeField] private StatModifier[] statModifiers = Array.Empty<StatModifier>();

        [Header("Affinity")]
        [Tooltip("Applies to every interaction's positive affinity delta")]
        [SerializeField, Range(-100f, 300f)] private float globalGainChangePercent;
        [Tooltip("Applies to every interaction's negative affinity delta")]
        [SerializeField, Range(-100f, 300f)] private float globalLossChangePercent;
        [SerializeField] private InteractionModifier[] interactionModifiers = Array.Empty<InteractionModifier>();

        public string TraitId => traitId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;

        /// <summary>
        /// True when either trait lists the other as conflicting.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool ConflictsWith(TraitSO other)
        {
            if (other == null || other == this) return false;
            return ListContains(conflictingTraits, other) || other.ListContains(other.conflictingTraits, this);
        }

        #region PUBLIC GETTERS
        /// <summary>
        /// Multiplier for a need's decay rate under the active conditions.
        /// </summary>
        /// <param name="need"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public float GetNeedDecayMultiplier(NeedType need, ConditionMask active)
        {
            float multiplier = 1f;
            for (int i = 0; i < needModifiers.Length; i++)
            {
                NeedModifier modifier = needModifiers[i];
                if (modifier.need != need) continue;
                if ((active & modifier.requiredConditions) != modifier.requiredConditions) continue;
                multiplier *= 1f + modifier.decayChangePercent * 0.01f;
            }
            return Mathf.Max(0f, multiplier);
        }

        /// <summary>
        /// Multipliers for a core stat under the active conditions.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public float GetStatMultiplier(CharacterStat stat, ConditionMask active)
        {
            float multiplier = 1f;
            for (int i = 0; i < statModifiers.Length; i++)
            {
                StatModifier modifier = statModifiers[i];
                if (modifier.stat != stat) continue;
                if ((active & modifier.requiredConditions) != modifier.requiredConditions) continue;
                multiplier *= 1f + modifier.changePercent * 0.01f;
            }
            return Mathf.Max(0f, multiplier);
        }

        /// <summary>
        /// Multiplier for an affinity delta of the given interaction.
        /// isGain selects the gain or loss percent.
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="isGain"></param>
        /// <returns></returns>
        public float GetAffinityMultiplier(InteractionType interaction, bool isGain)
        {
            float multiplier = 1f + (isGain ? globalGainChangePercent : globalLossChangePercent) * 0.01f;
            for (int i = 0; i < interactionModifiers.Length; i++)
            {
                if (interactionModifiers[i].interaction != interaction) continue;
                float percent = isGain ? interactionModifiers[i].gainChangePercent
                                       : interactionModifiers[i].lossChangePercent;
                multiplier *= 1f + percent * 0.01f;
            }
            return Mathf.Max(0f, multiplier);
        }

        #endregion

        private bool ListContains(TraitSO[] list, TraitSO trait)
        {
            if (list == null) return false;
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == trait) return true;
            }
            return false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(traitId))
                traitId = name.Replace("Trait_", string.Empty).ToLowerInvariant();
            conflictingTraits ??= Array.Empty<TraitSO>();
            needModifiers ??= Array.Empty<NeedModifier>();
            statModifiers ??= Array.Empty<StatModifier>();
            interactionModifiers ??= Array.Empty<InteractionModifier>();
        }
#endif
    }
}
