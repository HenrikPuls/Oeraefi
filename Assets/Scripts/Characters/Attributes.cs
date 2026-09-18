using System;

namespace Oerfi.Characters
{
    /// <summary>
    /// The six attributes (concept doc 11.2/11.6), scale 1-10 with a
    /// baseline of 5 for an average adult. Independently designed — no
    /// DSA affiliation. German display names for UI follow in M5:
    /// Kraft, Geschick, Zähigkeit, Wahrnehmung, Verstand, Wille.
    /// </summary>
    public enum AttributeId
    {
        Strength = 0,     // Kraft
        Dexterity = 1,    // Geschick
        Toughness = 2,    // Zähigkeit
        Perception = 3,   // Wahrnehmung
        Wits = 4,         // Verstand
        Willpower = 5     // Wille
    }

    /// <summary>
    /// Attribute block of one character. Struct with named value fields —
    /// struct copies are deep (an array-backed variant would alias the
    /// backing array across copies).
    /// </summary>
    [Serializable]
    public struct Attributes
    {
        public const int MIN = 1;
        public const int MAX = 10;
        public const int BASELINE = 5;
        public const int COUNT = 6;

        private int _strength;
        private int _dexterity;
        private int _toughness;
        private int _perception;
        private int _wits;
        private int _willpower;

        /// <summary>A fresh block at the baseline value for every attribute.</summary>
        public static Attributes CreateBaseline() => new Attributes
        {
            _strength = BASELINE,
            _dexterity = BASELINE,
            _toughness = BASELINE,
            _perception = BASELINE,
            _wits = BASELINE,
            _willpower = BASELINE
        };

        public int Get(AttributeId attribute)
        {
            return attribute switch
            {
                AttributeId.Strength => _strength,
                AttributeId.Dexterity => _dexterity,
                AttributeId.Toughness => _toughness,
                AttributeId.Perception => _perception,
                AttributeId.Wits => _wits,
                AttributeId.Willpower => _willpower,
                _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null)
            };
        }

        public void Set(AttributeId attribute, int value)
        {
            int clamped = value < MIN ? MIN : (value > MAX ? MAX : value);
            switch (attribute)
            {
                case AttributeId.Strength: _strength = clamped; break;
                case AttributeId.Dexterity: _dexterity = clamped; break;
                case AttributeId.Toughness: _toughness = clamped; break;
                case AttributeId.Perception: _perception = clamped; break;
                case AttributeId.Wits: _wits = clamped; break;
                case AttributeId.Willpower: _willpower = clamped; break;
                default: throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null);
            }
        }

        public void ApplyDelta(AttributeId attribute, int delta) => Set(attribute, Get(attribute) + delta);
    }
}
