// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

namespace NewRelic.Telemetry.Metrics
{
    /// <summary>
    /// Represents the aggregation values for a Summary Metric.
    /// </summary>
    public class MetricSummaryValue
    {
        /// <summary>
        /// Gets the number of observations that were aggregated.
        /// Must be a positive number.
        /// </summary>
        public double Count { get; private set; }

        /// <summary>
        /// Gets the sum of the values that were observed.
        /// </summary>
        public double Sum { get; private set; }

        /// <summary>
        /// Gets the lowest value observed.
        /// </summary>
        public double? Min { get; private set; }

        /// <summary>
        /// Gets the highest value observed.
        /// </summary>
        public double? Max { get; private set; }

        private MetricSummaryValue()
        {
        }

        /// <summary>
        /// Creates a summary value.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="sum"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public static MetricSummaryValue Create(double count, double sum, double min, double max)
        {
            var result = Create(count, sum);

            result.Min = min;
            result.Max = max;

            return result;
        }

        /// <summary>
        /// Creates a summary value.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="sum"></param>
        public static MetricSummaryValue Create(double count, double sum)
        {
            return new MetricSummaryValue()
            {
                Count = count,
                Sum = sum,
            };
        }
    }
}
