// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

#if NET452
using System;

namespace NewRelic.OpenTelemetry
{
    /// <summary>
    /// Provides DateTime conversions to Unix Timestamps.
    /// This supports backwards compatibility with .NET v4.5.
    /// </summary>
    internal static class DateTimeExtensions
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

        public static long ToUnixTimeMilliseconds(this DateTimeOffset dateTimeOffset)
        {
            return (dateTimeOffset.ToUniversalTime().Ticks / TicksPerMillisecond) - UnixEpochMilliseconds;
        }
    }
}
#endif
