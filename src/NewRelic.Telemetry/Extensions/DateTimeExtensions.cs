using System;

namespace NewRelic.Telemetry.Extensions
{
    /// <summary>
    /// Provides DateTime conversions to Unix Timestamps.
    /// This supports backwards compatibility with .NET v4.5
    /// </summary>
    public static class DateTimeExtensions
    {
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * 24;
        private const int DaysPerYear = 365;                            // Number of days in a non-leap year
        private const int DaysPer4Years = DaysPerYear * 4 + 1;          // 1461, Number of days in 4 years
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;     // 36524, Number of days in 100 years
        private const int DaysPer400Years = DaysPer100Years * 4 + 1;    // 146097, Number of days in 400 years
        private const int DaysTo1970 = DaysPer400Years * 4 + DaysPer100Years * 3 + DaysPer4Years * 17 + DaysPerYear; // 719,162, Number of days from 1/1/0001 to 12/31/1969
        private const long UnixEpochTicks = DaysTo1970 * TicksPerDay;
        private const long UnixEpochMilliseconds = UnixEpochTicks / TicksPerMillisecond; // 62,135,596,800,000

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTimeMilliseconds(this DateTimeOffset dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks / TicksPerMillisecond) - UnixEpochMilliseconds;
        }

        public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks / TicksPerMillisecond) - UnixEpochMilliseconds;
        }

    }
}
