using System;

namespace Oerfi.AI
{
    /// <summary>
    /// The labour roles a worker can be assigned to (concept doc 4.5:
    /// "Aufgabenzuweisung statt Mikromanagement" — Frostpunk principle).
    /// English identifiers; the German game-content names live in the per-
    /// member XML docs and surface in the UI milestone (M5).
    /// Member order follows the predecessor's enum (int values carry no
    /// compatibility weight — the enum is new).
    /// </summary>
    public enum TaskRole
    {
        /// <summary>Wollverarbeitung.</summary>
        WoolProcessing,
        /// <summary>Fischerei.</summary>
        Fishing,
        /// <summary>Torfstechen.</summary>
        PeatCutting,
        /// <summary>Ackerbau.</summary>
        Farming,
        /// <summary>Vogelfang (Brutzeit ca. Mai–August).</summary>
        Fowling,
        /// <summary>Robbenjagd.</summary>
        SealHunting,
        /// <summary>Steinbruch.</summary>
        Quarrying,
        /// <summary>Holzbau.</summary>
        WoodConstruction,
        /// <summary>Schmieden.</summary>
        Smithing,
        /// <summary>Weberei.</summary>
        Weaving,
        /// <summary>Böttcherei.</summary>
        Coopering,
        /// <summary>Lederverarbeitung.</summary>
        Leatherworking,
        /// <summary>Kochen (haushaltsnahe Arbeit).</summary>
        Cooking,
        /// <summary>Handeln (Seegelsaison Mai–September).</summary>
        Trading,
        /// <summary>Bau-Arbeiter: Zimmermann, Steinmetz, Torfstecher für Wände.</summary>
        Construction,
        /// <summary>Zimmermann — Holzbau, Dachstuhl.</summary>
        Carpenter,
        /// <summary>Steinmetz — Fundament, Ofen.</summary>
        Stonemason,
        /// <summary>Freie Zeit / unzugewiesen.</summary>
        Idle
    }

    /// <summary>
    /// Inclusive month window in which a seasonal role is available.
    /// Month indices are 0-based calendar months (0 = Harpa … 11 =
    /// Einmanudur). A window with start &gt; end wraps across the year
    /// boundary.
    /// </summary>
    [Serializable]
    public struct SeasonalWindow
    {
        public int StartMonth;
        public int EndMonth;

        public SeasonalWindow(int startMonth, int endMonth)
        {
            StartMonth = startMonth;
            EndMonth = endMonth;
        }

        public bool IsAvailable(int monthIndex)
        {
            if (StartMonth <= EndMonth)
                return monthIndex >= StartMonth && monthIndex <= EndMonth;
            return monthIndex >= StartMonth || monthIndex <= EndMonth; // year wrap
        }
    }

    /// <summary>
    /// Seasonal availability per role (concept doc 7 couplings): only
    /// Fowling (breeding season), Trading (sailing season) and Farming
    /// (growing season) are gated to the summer months; every other role is
    /// year-round. Windows are scale facts, not balancing — code constants.
    /// </summary>
    public static class TaskRoleDefinitions
    {
        public const int FIRST_SUMMER_MONTH = 0; // Harpa
        public const int LAST_SUMMER_MONTH = 5;  // Haustmanudur

        /// <summary>
        /// Returns the seasonal window for a gated role; false means the
        /// role is available year-round.
        /// </summary>
        public static bool TryGetWindow(TaskRole role, out SeasonalWindow window)
        {
            switch (role)
            {
                case TaskRole.Fowling:
                    // Breeding season ca. May-August.
                    window = new SeasonalWindow(FIRST_SUMMER_MONTH, LAST_SUMMER_MONTH);
                    return true;
                case TaskRole.Trading:
                    // Sailing season May-September.
                    window = new SeasonalWindow(FIRST_SUMMER_MONTH, LAST_SUMMER_MONTH);
                    return true;
                case TaskRole.Farming:
                    // Growing/harvest season.
                    window = new SeasonalWindow(FIRST_SUMMER_MONTH, LAST_SUMMER_MONTH);
                    return true;
                default:
                    window = default;
                    return false;
            }
        }

        public static bool IsYearRound(TaskRole role) => !TryGetWindow(role, out _);
    }
}
