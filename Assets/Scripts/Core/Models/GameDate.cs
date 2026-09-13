using System;

namespace Nation.Core.Models
{
    /// <summary>
    /// A calendar day in the simulation, stored as a day count from the game epoch (1 January 2026).
    /// Immutable and engine-free. Display formatting belongs to the localization layer.
    /// </summary>
    public readonly struct GameDate : IEquatable<GameDate>, IComparable<GameDate>
    {
        private static readonly DateTime EpochDateTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        public static readonly GameDate Epoch = new GameDate(0);

        public int DayNumber { get; }

        public GameDate(int dayNumber)
        {
            DayNumber = dayNumber;
        }

        public static GameDate FromYearMonthDay(int year, int month, int day)
        {
            var dateTime = new DateTime(year, month, day);
            return new GameDate((int)(dateTime - EpochDateTime).TotalDays);
        }

        public int Year => ToDateTime().Year;
        public int Month => ToDateTime().Month;
        public int Day => ToDateTime().Day;
        public DayOfWeek DayOfWeek => ToDateTime().DayOfWeek;

        public GameDate AddDays(int days) => new GameDate(DayNumber + days);

        public int DaysUntil(GameDate other) => other.DayNumber - DayNumber;

        public bool IsFirstDayOfMonth => Day == 1;
        public bool IsFirstDayOfYear => Month == 1 && Day == 1;

        /// <summary>ISO 8601 representation, used for logs and save metadata, never for player-facing text.</summary>
        public string ToIsoString()
        {
            var dateTime = ToDateTime();
            return dateTime.Year.ToString("D4") + "-" + dateTime.Month.ToString("D2") + "-" + dateTime.Day.ToString("D2");
        }

        public override string ToString() => ToIsoString();

        private DateTime ToDateTime() => EpochDateTime.AddDays(DayNumber);

        public bool Equals(GameDate other) => DayNumber == other.DayNumber;
        public override bool Equals(object obj) => obj is GameDate other && Equals(other);
        public override int GetHashCode() => DayNumber;
        public int CompareTo(GameDate other) => DayNumber.CompareTo(other.DayNumber);

        public static bool operator ==(GameDate left, GameDate right) => left.Equals(right);
        public static bool operator !=(GameDate left, GameDate right) => !left.Equals(right);
        public static bool operator <(GameDate left, GameDate right) => left.DayNumber < right.DayNumber;
        public static bool operator >(GameDate left, GameDate right) => left.DayNumber > right.DayNumber;
        public static bool operator <=(GameDate left, GameDate right) => left.DayNumber <= right.DayNumber;
        public static bool operator >=(GameDate left, GameDate right) => left.DayNumber >= right.DayNumber;
    }
}
