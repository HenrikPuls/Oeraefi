using System;
using System.Collections.Generic;

namespace Oerfi.Core
{
    /// <summary>
    /// The Old Icelandic month names in calendar order (concept doc section 7).
    /// ASCII-safe identifiers; the proper Icelandic spellings are noted per
    /// member and must be used in any player-facing display (UI milestone).
    /// </summary>
    public enum Month
    {
        Harpa,          // 1. Sommermonat
        Skerpla,
        Solmanudur,     // Sólmánuður
        Heyannir,       // Heuernte
        Tvimanudur,     // Tvímánuður
        Haustmanudur,   // Haustmánuður
        Gormanudur,     // Gormánuður — 1. Wintermonat
        Ylir,           // Ýlir
        Morsugur,       // Mörsugur
        Thorri,         // Þorri — Wintermodifikatoren, Versorgung kritisch
        Goa,            // Góa
        Einmanudur      // Einmánuður
    }

    /// <summary>
    /// Old Icelandic calendar model (concept doc section 7). Pure C# — the
    /// runtime driving/clock binding lives in the runtime module.
    ///
    /// Year structure (364-day base year):
    /// - 12 named months of 30 days each (Harpa..Einmanudur).
    /// - Aukanœtr: 4 intercalary days after Einmanudur at the end of every
    ///   year, counted to the Vetur season.
    /// - Sumarauki: 7 intercalary days after Solmanudur in leap years (every
    ///   6th year, first in 880), counted to the Sumar season.
    ///
    /// This is a deliberately simplified, deterministic approximation of the
    /// historical rím-rules intercalation (alternating 5/6-year pattern over
    /// an 11-year cycle); simplification decided in the M2 session and kept
    /// so season-coupled mechanics stay exactly testable.
    /// </summary>
    public class CalendarModel
    {
        public const int DAYS_PER_MONTH = 30;
        public const int MONTHS_PER_YEAR = 12;
        public const int AUKANET_DAYS = 4;
        public const int SUMARAUKI_DAYS = 7;
        public const int SUMARAUKI_INTERVAL_YEARS = 6;
        public const int FIRST_YEAR = 874;

        /// <summary>Days of the three summer months preceding Sumarauki.</summary>
        private const int DAYS_BEFORE_SUMARAUKI = 3 * DAYS_PER_MONTH;

        /// <summary>Base year length without intercalation (360 months + 4 aukanœtr).</summary>
        public const int BASE_YEAR_DAYS = MONTHS_PER_YEAR * DAYS_PER_MONTH + AUKANET_DAYS;

        private readonly List<ITickable> _tickables = new List<ITickable>();

        private int _year;
        private int _dayOfYear; // 0-based index within the calendar year

        /// <summary>Fires once per advanced day with the absolute day index (starts at 1).</summary>
        public event Action<int> DayChanged;

        /// <summary>Fires when the season flips (Sumar ↔ Vetur), including year rollover.</summary>
        public event Action<Season> SeasonChanged;

        public CalendarModel(int startYear = FIRST_YEAR)
        {
            _year = startYear;
            _dayOfYear = 0;
        }

        public int Year => _year;

        /// <summary>True while the year is inside the Sumarauki intercalary week.</summary>
        public bool IsSumarauki =>
            IsLeapYear(_year)
            && _dayOfYear >= DAYS_BEFORE_SUMARAUKI
            && _dayOfYear < DAYS_BEFORE_SUMARAUKI + SUMARAUKI_DAYS;

        /// <summary>True while the year is inside the end-of-year Aukanœtr.</summary>
        public bool IsAukanet => AdjustedDayOfYear >= MONTHS_PER_YEAR * DAYS_PER_MONTH;

        /// <summary>
        /// The current month. Intercalary periods keep the preceding month for
        /// display (Sumarauki → Solmanudur, Aukanœtr → Einmanudur); use
        /// <see cref="IsSumarauki"/>/<see cref="IsAukanet"/> to distinguish them.
        /// </summary>
        public Month CurrentMonth
        {
            get
            {
                if (IsSumarauki) return Month.Solmanudur;
                if (IsAukanet) return Month.Einmanudur;
                return (Month)(AdjustedDayOfYear / DAYS_PER_MONTH);
            }
        }

        /// <summary>
        /// Day within the current month (1..30) or intercalary period
        /// (Sumarauki 1..7, Aukanœtr 1..4).
        /// </summary>
        public int DayOfMonth
        {
            get
            {
                if (IsSumarauki) return _dayOfYear - DAYS_BEFORE_SUMARAUKI + 1;
                if (IsAukanet) return AdjustedDayOfYear - MONTHS_PER_YEAR * DAYS_PER_MONTH + 1;
                return AdjustedDayOfYear % DAYS_PER_MONTH + 1;
            }
        }

        public Season CurrentSeason
        {
            get
            {
                if (IsSumarauki) return Season.Sumar;
                if (IsAukanet) return Season.Vetur;
                return AdjustedDayOfYear / DAYS_PER_MONTH < MONTHS_PER_YEAR / 2 ? Season.Sumar : Season.Vetur;
            }
        }

        /// <summary>Absolute day count since FIRST_YEAR Harpa 1 (which is day 1).</summary>
        public int TotalDays => DaysBeforeYear(_year) + _dayOfYear + 1;

        /// <summary>Registers a tickable for day/season notifications.</summary>
        public void Register(ITickable tickable)
        {
            if (tickable != null && !_tickables.Contains(tickable)) _tickables.Add(tickable);
        }

        public void Unregister(ITickable tickable)
        {
            _tickables.Remove(tickable);
        }

        /// <summary>Advances the calendar by one day and notifies all tickables.</summary>
        public void AdvanceDay()
        {
            Season seasonBefore = CurrentSeason;
            _dayOfYear++;
            if (_dayOfYear >= DaysInYear(_year))
            {
                _year++;
                _dayOfYear = 0;
            }

            Season seasonAfter = CurrentSeason;
            if (seasonAfter != seasonBefore)
            {
                SeasonChanged?.Invoke(seasonAfter);
                foreach (ITickable tickable in _tickables) tickable.OnSeasonChange(seasonAfter);
            }

            DayChanged?.Invoke(TotalDays);
            foreach (ITickable tickable in _tickables) tickable.OnDayTick(TotalDays);
        }

        /// <summary>Leap years: every SUMARAUKI_INTERVAL_YEARS years after the first year (880, 886, …).</summary>
        public static bool IsLeapYear(int year)
        {
            return year > FIRST_YEAR
                && (year - FIRST_YEAR) % SUMARAUKI_INTERVAL_YEARS == 0;
        }

        /// <summary>Total days in the given year (371 in leap years, else 364).</summary>
        public static int DaysInYear(int year)
        {
            return BASE_YEAR_DAYS + (IsLeapYear(year) ? SUMARAUKI_DAYS : 0);
        }

        /// <summary>Day index within the year with the Sumarauki block removed.</summary>
        private int AdjustedDayOfYear
        {
            get
            {
                if (IsLeapYear(_year) && _dayOfYear >= DAYS_BEFORE_SUMARAUKI + SUMARAUKI_DAYS)
                    return _dayOfYear - SUMARAUKI_DAYS;
                return _dayOfYear;
            }
        }

        private static int DaysBeforeYear(int year)
        {
            // Leap years are FIRST_YEAR + k * INTERVAL with k >= 1, so the
            // count of leap years fully elapsed before `year` is:
            int leapYearsBefore = year - 1 - FIRST_YEAR >= SUMARAUKI_INTERVAL_YEARS
                ? (year - 1 - FIRST_YEAR) / SUMARAUKI_INTERVAL_YEARS
                : 0;
            return (year - FIRST_YEAR) * BASE_YEAR_DAYS + leapYearsBefore * SUMARAUKI_DAYS;
        }
    }
}
