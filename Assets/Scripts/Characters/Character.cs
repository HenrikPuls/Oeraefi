using System;

namespace Oerfi.Characters
{
    /// <summary>
    /// A single character: origin, attributes, and grown skills. Plain data
    /// class (not a MonoBehaviour) — runtime spawning/persistence comes
    /// later (M5/M7).
    /// </summary>
    public class Character
    {
        public Character(string name, Origin origin, Attributes attributes, SkillSystem skills)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Origin = origin;
            Attributes = attributes;
            Skills = skills ?? throw new ArgumentNullException(nameof(skills));
        }

        public string Name { get; set; }

        public Origin Origin { get; }

        public Attributes Attributes { get; }

        public SkillSystem Skills { get; }
    }
}
