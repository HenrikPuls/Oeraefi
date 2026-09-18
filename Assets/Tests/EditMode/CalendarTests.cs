using NUnit.Framework;
using Oerfi.Core;

namespace Oerfi.Tests.EditMode
{
    /// <summary>
    /// Calendar tests: ported from the predecessor project's
    /// CalendarServiceTests (pure model now, no GameObject boilerplate) plus
    /// new coverage for the 364-day year, Aukanœtr, and the Sumarauki leap week.
    /// </summary>
    public class CalendarTests
    {
        [Test]
        public void AdvanceDay_IncrementsDayOfMonth()
        {
            var calendar = new CalendarModel();
            calendar.AdvanceDay();
            Assert.That(calendar.DayOfMonth, Is.EqualTo(2));
            Assert.That(calendar.CurrentMonth, Is.EqualTo(Month.Harpa));
        }

        [Test]
        public void AdvanceDay_MonthRollover_ResetsDayAndAdvancesMonth()
        {
            var calendar = new CalendarModel();
            for (int i = 0; i < CalendarModel.DAYS_PER_MONTH; i++) calendar.AdvanceDay();
            Assert.That(calendar.DayOfMonth, Is.EqualTo(1));
            Assert.That(calendar.CurrentMonth, Is.EqualTo(Month.Skerpla));
        }

        [Test]
        public void Season_SwitchesBetweenSumarAndVetur()
        {
            var calendar = new CalendarModel();
            Assert.That(calendar.CurrentSeason, Is.EqualTo(Season.Sumar));

            // Sumar = months 0..5 (180 days); day 181 (day-of-year index 180) is Vetur.
            for (int i = 0; i < 6 * CalendarModel.DAYS_PER_MONTH; i++) calendar.AdvanceDay();
            Assert.That(calendar.CurrentSeason, Is.EqualTo(Season.Vetur));
            Assert.That(calendar.CurrentMonth, Is.EqualTo(Month.Gormanudur));

            // After the full year (364 days) the next Harpa is Sumar again.
            for (int i = 6 * CalendarModel.DAYS_PER_MONTH; i < CalendarModel.BASE_YEAR_DAYS; i++)
                calendar.AdvanceDay();
            Assert.That(calendar.CurrentSeason, Is.EqualTo(Season.Sumar));
            Assert.That(calendar.Year, Is.EqualTo(875));
        }

        [Test]
        public void SeasonChanged_EventFiresExactlyOnSeasonBoundary()
        {
            var calendar = new CalendarModel();
            int fires = 0;
            calendar.SeasonChanged += _ => fires++;

            for (int i = 0; i < 179; i++) calendar.AdvanceDay();
            Assert.That(fires, Is.EqualTo(0), "no season change before day 180");

            calendar.AdvanceDay();
            Assert.That(fires, Is.EqualTo(1), "exactly one Sumar->Vetur flip on day 180");
        }

        [Test]
        public void YearLength_NormalYear_Is364Days()
        {
            var calendar = new CalendarModel();
            for (int i = 0; i < CalendarModel.BASE_YEAR_DAYS; i++) calendar.AdvanceDay();
            Assert.That(calendar.Year, Is.EqualTo(875));
            Assert.That(calendar.CurrentMonth, Is.EqualTo(Month.Harpa));
            Assert.That(calendar.DayOfMonth, Is.EqualTo(1));
        }

        [Test]
        public void YearLength_LeapYear_Is371Days()
        {
            var calendar = new CalendarModel(880); // leap year: (880-874) % 6 == 0
            for (int i = 0; i < CalendarModel.BASE_YEAR_DAYS + CalendarModel.SUMARAUKI_DAYS; i++)
                calendar.AdvanceDay();
            Assert.That(calendar.Year, Is.EqualTo(881));
            Assert.That(calendar.CurrentMonth, Is.EqualTo(Month.Harpa));
            Assert.That(calendar.DayOfMonth, Is.EqualTo(1));
        }

        [Test]
        public void Aukanet_AfterEinmanudur_InVetur()
        {
            var calendar = new CalendarModel();
            for (int i = 0; i < 12 * CalendarModel.DAYS_PER_MONTH; i++) calendar.AdvanceDay();

            Assert.That(calendar.IsAukanet, Is.True);
            Assert.That(calendar.IsSumarauki, Is.False);
            Assert.That(calendar.CurrentSeason, Is.EqualTo(Season.Vetur));
            Assert.That(calendar.DayOfMonth, Is.EqualTo(1));

            calendar.AdvanceDay();
            calendar.AdvanceDay();
            calendar.AdvanceDay();
            Assert.That(calendar.DayOfMonth, Is.EqualTo(CalendarModel.AUKANET_DAYS));
        }

        [Test]
        public void Sumarauki_AfterSolmanudur_OnlyInLeapYears()
        {
            var calendar = new CalendarModel(880);
            for (int i = 0; i < 3 * CalendarModel.DAYS_PER_MONTH; i++) calendar.AdvanceDay();

            Assert.That(calendar.IsSumarauki, Is.True);
            Assert.That(calendar.CurrentSeason, Is.EqualTo(Season.Sumar));
            Assert.That(calendar.DayOfMonth, Is.EqualTo(1));

            calendar.AdvanceDay();
            Assert.That(calendar.DayOfMonth, Is.EqualTo(2));

            for (int i = 2; i <= CalendarModel.SUMARAUKI_DAYS; i++) calendar.AdvanceDay();
            Assert.That(calendar.IsSumarauki, Is.False);
            Assert.That(calendar.CurrentMonth, Is.EqualTo(Month.Heyannir));
            Assert.That(calendar.DayOfMonth, Is.EqualTo(1));
        }

        [Test]
        public void LeapYears_FollowSixYearInterval()
        {
            Assert.That(CalendarModel.IsLeapYear(874), Is.False);
            Assert.That(CalendarModel.IsLeapYear(879), Is.False);
            Assert.That(CalendarModel.IsLeapYear(880), Is.True);
            Assert.That(CalendarModel.IsLeapYear(881), Is.False);
            Assert.That(CalendarModel.IsLeapYear(886), Is.True);
        }

        [Test]
        public void TotalDays_CountsAbsoluteDaysAcrossYears()
        {
            var calendar = new CalendarModel();
            Assert.That(calendar.TotalDays, Is.EqualTo(1));

            for (int i = 0; i < CalendarModel.BASE_YEAR_DAYS; i++) calendar.AdvanceDay();
            Assert.That(calendar.TotalDays, Is.EqualTo(CalendarModel.BASE_YEAR_DAYS + 1));

            // Advance into leap year 880: 875..879 (5 × 364) + 874 (364) = 2184.
            var leapCalendar = new CalendarModel(880);
            Assert.That(leapCalendar.TotalDays, Is.EqualTo(2184 + 1));
        }
    }
}
