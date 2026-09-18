namespace Oerfi.Core
{
    /// <summary>
    /// Consumes calendar-driven simulation ticks. Registered tickables are
    /// notified once per advanced in-game day (see <see cref="CalendarModel"/>).
    /// </summary>
    public interface ITickable
    {
        /// <summary>Called once per advanced day with the absolute day index.</summary>
        void OnDayTick(int dayIndex);

        /// <summary>Called when the season flips (Sumar ↔ Vetur).</summary>
        void OnSeasonChange(Season newSeason);
    }
}
