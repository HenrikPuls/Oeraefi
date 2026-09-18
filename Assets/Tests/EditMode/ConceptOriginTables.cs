using System.Collections.Generic;
using Oerfi.Characters;

namespace Oerfi.Tests.EditMode
{
    /// <summary>
    /// The concept-doc 11.6 origin tables as canonical test data — single
    /// source of truth for the factory tests, the generated-asset equality
    /// test, and the household tests. Mirrors the historical values of the
    /// predecessor project exactly.
    /// </summary>
    public static class ConceptOriginTables
    {
        // Attribute order: Strength, Dexterity, Toughness, Perception, Wits, Willpower.
        public static readonly (Origin origin, string displayName, int[] attributeDeltas)[] ATTRIBUTE_DELTAS =
        {
            (Origin.NorwegianFarmer, "Norwegischer Bauer (Norðmaðr)", new[] { 1, 0, 1, 0, 0, 0 }),
            (Origin.HebrideanGael, "Hebridisch-gälischer Siedler (Vestmaðr)", new[] { 0, 1, 0, 1, 0, 0 }),
            (Origin.FaroeseShetland, "Färöisch/Shetland-Herkunft", new[] { 0, 0, 1, 1, 0, 0 }),
            (Origin.Freedman, "Freigelassener/einfacher Siedler", new[] { 0, 0, 0, 0, 0, 1 }),
        };

        // Skill order: Farming, AnimalHusbandry, Smithing, Weaving, Woodworking,
        // Stonemasonry, Leatherworking, Coopering, Fishing, Fowling, SealHunting,
        // CoastalSailing, Helmsmanship, Trading, Combat, ThingOratory.
        public static readonly (Origin origin, int[] skillStarts)[] SKILL_STARTS =
        {
            (Origin.NorwegianFarmer, new[] { 4, 4, 1, 2, 3, 1, 2, 2, 2, 1, 1, 3, 1, 3, 1, 2 }),
            (Origin.HebrideanGael, new[] { 2, 2, 1, 3, 1, 4, 1, 1, 3, 3, 2, 2, 1, 1, 2, 1 }),
            (Origin.FaroeseShetland, new[] { 1, 2, 0, 1, 1, 2, 2, 1, 4, 4, 3, 4, 2, 1, 1, 1 }),
            (Origin.Freedman, new[] { 2, 2, 1, 2, 2, 2, 2, 2, 2, 1, 1, 1, 0, 1, 1, 0 }),
        };

        public static int[] AttributeDeltasOf(Origin origin)
        {
            foreach (var entry in ATTRIBUTE_DELTAS)
                if (entry.origin == origin) return entry.attributeDeltas;
            throw new System.Collections.Generic.KeyNotFoundException(origin.ToString());
        }

        public static int[] SkillStartsOf(Origin origin)
        {
            foreach (var entry in SKILL_STARTS)
                if (entry.origin == origin) return entry.skillStarts;
            throw new System.Collections.Generic.KeyNotFoundException(origin.ToString());
        }

        public static string DisplayNameOf(Origin origin)
        {
            foreach (var entry in ATTRIBUTE_DELTAS)
                if (entry.origin == origin) return entry.displayName;
            throw new System.Collections.Generic.KeyNotFoundException(origin.ToString());
        }

        /// <summary>Runtime-only profiles built straight from the tables.</summary>
        public static OriginProfile CreateProfile(Origin origin)
            => OriginProfile.CreateRuntimeOnly(origin, DisplayNameOf(origin),
                AttributeDeltasOf(origin), SkillStartsOf(origin));
    }
}
