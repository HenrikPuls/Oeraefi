using System;

namespace Oerfi.Characters
{
    /// <summary>
    /// Creates characters from an origin profile (concept doc 11.6):
    /// baseline attributes shifted by the profile's deltas, skills seeded
    /// from the profile's start values.
    /// </summary>
    public static class CharacterFactory
    {
        public static Character Create(OriginProfile profile, string name = null)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            Attributes attributes = Attributes.CreateBaseline();
            foreach (AttributeId attribute in Enum.GetValues(typeof(AttributeId)))
                attributes.ApplyDelta(attribute, profile.GetAttributeDelta(attribute));

            var skills = new SkillSystem();
            foreach (SkillId skill in Enum.GetValues(typeof(SkillId)))
                skills.Set(skill, profile.GetSkillStart(skill));

            return new Character(
                string.IsNullOrEmpty(name) ? profile.DisplayName : name,
                profile.Origin,
                attributes,
                skills);
        }
    }
}
