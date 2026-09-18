using Oerfi.Core;

namespace Oerfi.Household
{
    /// <summary>
    /// Supply requirements per thrall (concept doc 4.5), mirroring free
    /// household members: thralls are a real resource binding, not a free
    /// labor bonus. Lodging binds building capacity (no running consumption)
    /// — modeled with buildings in M6, not here.
    /// </summary>
    public class ThrallSupplyNeed
    {
        /// <summary>Food per day in supply units (VE), base.</summary>
        public const float FOOD_BASE = 1f;
        /// <summary>Winter (Þorri/Góa) +50 % food — modeled per season here.</summary>
        public const float WINTER_MODIFIER = 0.5f;
        /// <summary>Heavy labor (forest clearing, peat cutting) +20 % food.</summary>
        public const float HEAVY_WORK_MODIFIER = 0.2f;
        /// <summary>One full clothing set per year.</summary>
        public const float CLOTHING_SETS_PER_YEAR = 1f;

        /// <summary>
        /// Daily food need; modifiers are ADDITIVE (winter + heavy work
        /// stack to +70 %, matching the predecessor values 1.0/1.2/1.5/1.7).
        /// Season is a parameter — a thrall's need follows the season each
        /// tick, it is not fixed at construction.
        /// </summary>
        public static float FoodPerDay(Season season, bool heavyWork = false)
        {
            float modifier = 0f;
            if (season == Season.Vetur) modifier += WINTER_MODIFIER;
            if (heavyWork) modifier += HEAVY_WORK_MODIFIER;
            return FOOD_BASE * (1f + modifier);
        }

        /// <summary>
        /// Daily clothing-sets fraction, anchored to the ACTUAL year length
        /// of the calendar (364 days; the predecessor hardcoded 360).
        /// </summary>
        public static float ClothingPerDay()
            => CLOTHING_SETS_PER_YEAR / CalendarModel.BASE_YEAR_DAYS;
    }
}
