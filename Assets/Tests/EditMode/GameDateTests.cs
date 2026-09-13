using System;
using Nation.Core.Models;
using NUnit.Framework;

namespace Nation.Tests
{
    public sealed class GameDateTests
    {
        [Test]
        public void Epoch_Is_First_Of_January_2026()
        {
            var date = GameDate.Epoch;

            Assert.AreEqual(2026, date.Year);
            Assert.AreEqual(1, date.Month);
            Assert.AreEqual(1, date.Day);
            Assert.AreEqual("2026-01-01", date.ToIsoString());
        }

        [Test]
        public void AddDays_Rolls_Over_Month_And_Year()
        {
            var endOfJanuary = GameDate.Epoch.AddDays(30);
            Assert.AreEqual("2026-01-31", endOfJanuary.ToIsoString());

            var firstOfFebruary = endOfJanuary.AddDays(1);
            Assert.AreEqual("2026-02-01", firstOfFebruary.ToIsoString());

            var nextYear = GameDate.Epoch.AddDays(365);
            Assert.AreEqual("2027-01-01", nextYear.ToIsoString());
        }

        [Test]
        public void Leap_Day_Is_Counted_In_2028()
        {
            var february28 = GameDate.FromYearMonthDay(2028, 2, 28);
            var next = february28.AddDays(1);

            Assert.AreEqual(2, next.Month);
            Assert.AreEqual(29, next.Day);
            Assert.AreEqual("2028-03-01", next.AddDays(1).ToIsoString());
        }

        [Test]
        public void FromYearMonthDay_Round_Trips_Through_DayNumber()
        {
            var date = GameDate.FromYearMonthDay(2031, 7, 14);

            Assert.AreEqual(date, new GameDate(date.DayNumber));
            Assert.AreEqual(2031, date.Year);
            Assert.AreEqual(7, date.Month);
            Assert.AreEqual(14, date.Day);
        }

        [Test]
        public void Comparison_Follows_Day_Order()
        {
            var earlier = GameDate.Epoch;
            var later = GameDate.Epoch.AddDays(10);

            Assert.IsTrue(earlier < later);
            Assert.IsTrue(later > earlier);
            Assert.IsTrue(earlier <= later);
            Assert.AreEqual(10, earlier.DaysUntil(later));
            Assert.AreNotEqual(earlier, later);
        }

        [Test]
        public void First_Day_Flags_Are_Correct()
        {
            Assert.IsTrue(GameDate.Epoch.IsFirstDayOfMonth);
            Assert.IsTrue(GameDate.Epoch.IsFirstDayOfYear);
            Assert.IsFalse(GameDate.Epoch.AddDays(1).IsFirstDayOfMonth);
            Assert.IsTrue(GameDate.FromYearMonthDay(2026, 3, 1).IsFirstDayOfMonth);
            Assert.IsFalse(GameDate.FromYearMonthDay(2026, 3, 1).IsFirstDayOfYear);
        }
    }
}
