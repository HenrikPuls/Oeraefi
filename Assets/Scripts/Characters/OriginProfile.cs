using System;
using UnityEngine;

namespace Oerfi.Characters
{
    /// <summary>The four settlement-period origins (concept doc 11.1).</summary>
    public enum Origin
    {
        NorwegianFarmer = 0,   // Norwegischer Bauer (Norðmaðr)
        HebrideanGael = 1,     // Hebridisch-gälischer Siedler (Vestmaðr)
        FaroeseShetland = 2,   // Färöisch/Shetland-Herkunft
        Freedman = 3           // Freigelassener/einfacher Siedler
    }

    /// <summary>
    /// Data-driven starting profile of one origin (concept doc 11.6):
    /// attribute deltas against the baseline of 5 and skill start values
    /// 0-4. One asset per origin under Assets/Data/Characters/Origins
    /// (ADR-004) — values are authored data, generated initially by
    /// Oerfi.Editor.Characters.CharacterAssetGenerator and never overwritten
    /// by code afterwards. Named per-value fields instead of arrays so the
    /// Unity serializer persists them (the predecessor's jagged int[][] was
    /// silently dropped on save) and the Inspector shows each entry.
    /// Display names stay German by design (game content, not code).
    /// </summary>
    [CreateAssetMenu(fileName = "OriginProfile", menuName = "Oerfi/Origin Profile")]
    public class OriginProfile : ScriptableObject
    {
        [SerializeField] private Origin _origin;
        [SerializeField] private string _displayName;

        [Header("Attribute deltas vs. baseline 5")]
        [SerializeField] private int _strengthDelta;
        [SerializeField] private int _dexterityDelta;
        [SerializeField] private int _toughnessDelta;
        [SerializeField] private int _perceptionDelta;
        [SerializeField] private int _witsDelta;
        [SerializeField] private int _willpowerDelta;

        [Header("Skill start values 0-4")]
        [SerializeField] private int _farmingStart;
        [SerializeField] private int _animalHusbandryStart;
        [SerializeField] private int _smithingStart;
        [SerializeField] private int _weavingStart;
        [SerializeField] private int _woodworkingStart;
        [SerializeField] private int _stonemasonryStart;
        [SerializeField] private int _leatherworkingStart;
        [SerializeField] private int _cooperingStart;
        [SerializeField] private int _fishingStart;
        [SerializeField] private int _fowlingStart;
        [SerializeField] private int _sealHuntingStart;
        [SerializeField] private int _coastalSailingStart;
        [SerializeField] private int _helmsmanshipStart;
        [SerializeField] private int _tradingStart;
        [SerializeField] private int _combatStart;
        [SerializeField] private int _thingOratoryStart;

        public Origin Origin => _origin;

        /// <summary>Player-facing origin name (German by design).</summary>
        public string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;

        public int GetAttributeDelta(AttributeId attribute)
        {
            return attribute switch
            {
                AttributeId.Strength => _strengthDelta,
                AttributeId.Dexterity => _dexterityDelta,
                AttributeId.Toughness => _toughnessDelta,
                AttributeId.Perception => _perceptionDelta,
                AttributeId.Wits => _witsDelta,
                AttributeId.Willpower => _willpowerDelta,
                _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null)
            };
        }

        public int GetSkillStart(SkillId skill)
        {
            return skill switch
            {
                SkillId.Farming => _farmingStart,
                SkillId.AnimalHusbandry => _animalHusbandryStart,
                SkillId.Smithing => _smithingStart,
                SkillId.Weaving => _weavingStart,
                SkillId.Woodworking => _woodworkingStart,
                SkillId.Stonemasonry => _stonemasonryStart,
                SkillId.Leatherworking => _leatherworkingStart,
                SkillId.Coopering => _cooperingStart,
                SkillId.Fishing => _fishingStart,
                SkillId.Fowling => _fowlingStart,
                SkillId.SealHunting => _sealHuntingStart,
                SkillId.CoastalSailing => _coastalSailingStart,
                SkillId.Helmsmanship => _helmsmanshipStart,
                SkillId.Trading => _tradingStart,
                SkillId.Combat => _combatStart,
                SkillId.ThingOratory => _thingOratoryStart,
                _ => throw new ArgumentOutOfRangeException(nameof(skill), skill, null)
            };
        }

        /// <summary>
        /// Creates an OriginProfile instance at runtime/editor time without
        /// an asset file (tests, procedurally constructed scenarios). The
        /// instance is NOT persisted — do not save references to it or ship
        /// it in builds.
        /// </summary>
        public static OriginProfile CreateRuntimeOnly(
            Origin origin,
            string displayName,
            int[] attributeDeltas,
            int[] skillStarts)
        {
            if (attributeDeltas == null || attributeDeltas.Length != Attributes.COUNT)
                throw new ArgumentException($"attributeDeltas requires exactly {Attributes.COUNT} values.");
            if (skillStarts == null || skillStarts.Length != SkillIdCount())
                throw new ArgumentException($"skillStarts requires exactly {SkillIdCount()} values.");

            OriginProfile profile = CreateInstance<OriginProfile>();
            profile._origin = origin;
            profile._displayName = displayName;
            profile._strengthDelta = attributeDeltas[(int)AttributeId.Strength];
            profile._dexterityDelta = attributeDeltas[(int)AttributeId.Dexterity];
            profile._toughnessDelta = attributeDeltas[(int)AttributeId.Toughness];
            profile._perceptionDelta = attributeDeltas[(int)AttributeId.Perception];
            profile._witsDelta = attributeDeltas[(int)AttributeId.Wits];
            profile._willpowerDelta = attributeDeltas[(int)AttributeId.Willpower];
            profile._farmingStart = skillStarts[(int)SkillId.Farming];
            profile._animalHusbandryStart = skillStarts[(int)SkillId.AnimalHusbandry];
            profile._smithingStart = skillStarts[(int)SkillId.Smithing];
            profile._weavingStart = skillStarts[(int)SkillId.Weaving];
            profile._woodworkingStart = skillStarts[(int)SkillId.Woodworking];
            profile._stonemasonryStart = skillStarts[(int)SkillId.Stonemasonry];
            profile._leatherworkingStart = skillStarts[(int)SkillId.Leatherworking];
            profile._cooperingStart = skillStarts[(int)SkillId.Coopering];
            profile._fishingStart = skillStarts[(int)SkillId.Fishing];
            profile._fowlingStart = skillStarts[(int)SkillId.Fowling];
            profile._sealHuntingStart = skillStarts[(int)SkillId.SealHunting];
            profile._coastalSailingStart = skillStarts[(int)SkillId.CoastalSailing];
            profile._helmsmanshipStart = skillStarts[(int)SkillId.Helmsmanship];
            profile._tradingStart = skillStarts[(int)SkillId.Trading];
            profile._combatStart = skillStarts[(int)SkillId.Combat];
            profile._thingOratoryStart = skillStarts[(int)SkillId.ThingOratory];
            profile.name = origin.ToString();
            return profile;
        }

        private static int SkillIdCount() => Enum.GetValues(typeof(SkillId)).Length;
    }
}
