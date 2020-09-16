// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NewRelic.Telemetry.Metrics
{
    /// <summary>
    /// Represents the aggregation values for a Summary Metric.
    /// </summary>
    public class MetricSummaryValue
    {
        /// <summary>
        /// The number of observations that were aggregated.
        /// Must be a positive number.
        /// </summary>
        public double Count { get; private set; }

        /// <summary>
        /// The sum of the values that were observed.
        /// </summary>
        public double Sum { get; private set; }

        /// <summary>
        /// The lowest value observed.
        /// </summary>
        public double? Min { get; private set; }

        /// <summary>
        /// The highest value observed.
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

    /// <summary>
    /// A metric represents the value of a measurement.
    /// </summary>
    public abstract class Metric
    {
        /// <summary>
        /// The name of the metric.  Must be less that 255 characters.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Identifies the type of value represented by this metric.
        /// Valid values: count, gauge, or summary
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// The Metric's start time.  Metrics often represent values collected
        /// over a period of time.  This value should be reported as the start time
        /// of the measurement window.  For point in time observations, this value
        /// should be the time at which the measurement was taken.
        /// </summary>
        public long? Timestamp { get; internal set; }

        /// <summary>
        /// Identifies the duration of the time window that the value represents.
        /// This is required for count and summary type metrics.
        /// </summary>
        [DataMember(Name = "interval.ms")]
        public long? IntervalMs { get; internal set; }

        /// <summary>
        /// The value being reported for this metric.  The layout of this value will
        /// vary depending on the implementation: count, summary, or gauge
        /// </summary>
        [DataMember(Name = "value")]
        public abstract object MetricValue { get; }

        /// <summary>
        /// A map of Key Value pairs identifying the dimensions of this metric.
        /// Keys are case sensitive and must be less than 255 characters.  Values may be
        /// strings, numbers, or booleans.
        /// </summary>
        public Dictionary<string, object> Attributes { get; internal set; }
    }

    public abstract class Metric<T> : Metric
    {
        [IgnoreDataMember]
        public T Value { get; internal set; }

        public override object MetricValue => Value;
    }

    /// <summary>
    /// Count Metrics measure the number of occurrences of an event within 
    /// a reporting window. The count should be reset to 0 for each window (i.e. every time the 
    /// metric is reported). 
    /// When using a CountMetric, IntervalMs must be reported.
    /// When using a CountMetric, the Value must be a positive value.
    /// </summary>
    /// <example>Cache hits per reporting interval.</example>
    /// <example>Number of threads created per reporting interval.</example>
    public class CountMetric : Metric<double>
    {
        public override string Type => "count";
    }

    /// <summary>
    /// Gauge Metrics represent a value that can increase or decrease with time.
    /// </summary>
    /// <example>The temperature, CPU usage, and memory.</example>
    public class GaugeMetric : Metric<double>
    {
        public override string Type => "gauge";
    }

    /// <summary>
    /// Summary Metrics represent pre-aggregated data, or information on aggregated 
    /// discrete events.
    /// When using a SummaryMetric, IntervalMs must be reported.
    /// </summary>
    public class SummaryMetric : Metric<MetricSummaryValue>
    {
        public override string Type => "summary";
    }
}
