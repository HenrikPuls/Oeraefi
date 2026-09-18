namespace Oerfi.Characters
{
    /// <summary>
    /// The sixteen skills (concept doc 11.3/11.6), grouped by the core
    /// loops from concept doc 9. German display names for UI follow in M5.
    /// </summary>
    public enum SkillId
    {
        // Agriculture
        Farming = 0,            // Ackerbau
        AnimalHusbandry = 1,    // Viehzucht

        // Crafts (one skill per trade)
        Smithing = 2,           // Schmieden
        Weaving = 3,            // Weberei
        Woodworking = 4,        // Holzbau
        Stonemasonry = 5,       // Steinbau
        Leatherworking = 6,     // Lederverarbeitung
        Coopering = 7,          // Böttcherei

        // Food procurement
        Fishing = 8,            // Fischfang
        Fowling = 9,            // Vogelfang
        SealHunting = 10,       // Robbenjagd

        // Seafaring
        CoastalSailing = 11,    // Küstenschifffahrt
        Helmsmanship = 12,      // Steuermannskunst

        // Trade
        Trading = 13,           // Handel

        // Combat (secondary)
        Combat = 14,            // Kampf

        // Social/law
        ThingOratory = 15       // Thing-Rede/Führung
    }
}
