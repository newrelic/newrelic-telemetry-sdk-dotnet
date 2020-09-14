using NewRelic.Telemetry.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NewRelic.Telemetry.Metrics
{
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
        /// A map of Key Value pairs identifying the dimensions of this metric.
        /// Keys are case sensitive and must be less than 255 characters.  Values may be
        /// strings, numbers, or booleans.
        /// </summary>
        public Dictionary<string, object>? Attributes { get; private set; }

        private Dictionary<string,object> EnsureAttributes()
        {
            return Attributes ??= new Dictionary<string, object>();
        }

        public abstract object Value { get; }

        protected Metric(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Identifies the metrics start time for all of the metrics that are part 
        /// of this batch.
        /// </summary>
        /// <param name="timestamp">Should be reported in UTC.</param>
        public Metric WithTimestamp(DateTime timestamp)
        {
            if (timestamp == default)
            {
                return this;
            }

            Timestamp = DateTimeExtensions.ToUnixTimeMilliseconds(timestamp);

            return this;
        }

        /// <summary>
        /// Used in conjunction with <see cref="WithTimestamp(DateTime)"/>, identifies the duration of 
        /// the observation window for this metric batch.
        /// </summary>
        /// <param name="intervalMs">The number of milliseconds</param>
        public Metric WithIntervalMs(long intervalMs)
        {
            if (intervalMs == default)
            {
                return this;
            }

            IntervalMs = intervalMs;

            return this;
        }

        /// <summary>
        /// Used to set the value of a custom attribute that is common to all metrics being reported 
        /// as part of this MetricBatch.
        /// </summary>
        /// <param name="attribName">Required: The name of the attribute.  If the name is already used, this operation will overwrite any existing value.</param>
        /// <param name="attribValue">The value of the attribute.  A NULL value will NOT be reported to the New Relic endpoint.</param>
        public Metric WithAttribute(string attribName, object attribValue)
        {
            if (string.IsNullOrWhiteSpace(attribName))
            {
                throw new InvalidOperationException($"{nameof(attribName)} cannot be empty.");
            }

            EnsureAttributes()[attribName] = attribValue;

            return this;
        }

        /// <summary>
        /// Used to apply a set of custom attribute values that are common to all metrics being reported
        /// as part of this MetricBatch.
        /// </summary>
        /// <param name="attributes">Collection of Key/Value pairs of attributes.  The keys should be unique.  
        /// In the event of duplicate keys, the last value will be accepted.</param>
        /// <returns></returns>
        public Metric WithAttributes(ICollection<KeyValuePair<string, object>> attributes)
        {
            if (attributes == null)
            {
                return this;
            }

            foreach (var attrib in attributes)
            {
                WithAttribute(attrib.Key, attrib.Value);
            }

            return this;
        }
    }

    public abstract class Metric<T> : Metric where T:struct
    {
        /// <summary>
        /// The value being reported for this metric.  The layout of this value will
        /// vary depending on the implementation: count, summary, or gauge
        /// </summary>
        [DataMember(Name = "value")]
        public readonly T MetricValue;

        protected Metric(string name, T value) : base(name)
        {
            this.MetricValue = value;
        }

        [IgnoreDataMember]
        public override object Value => MetricValue;
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
        public static CountMetric Create(string name, double value)
        {
            return new CountMetric(name, value);
        }

        private CountMetric(string name, double value) : base(name,value)
        {
        }

        public override string Type => "count";
    }

    /// <summary>
    /// Gauge Metrics represent a value that can increase or decrease with time.
    /// </summary>
    /// <example>The temperature, CPU usage, and memory.</example>
    public class GaugeMetric : Metric<double>
    {
        public static GaugeMetric Create(string name, double value)
        {
            return new GaugeMetric(name, value);
        }

        private GaugeMetric(string name, double value) : base(name, value)
        {
        }

        public override string Type => "gauge";
    }

    /// <summary>
    /// Summary Metrics represent pre-aggregated data, or information on aggregated 
    /// discrete events.
    /// When using a SummaryMetric, IntervalMs must be reported.
    /// </summary>
    public class SummaryMetric : Metric<MetricSummaryValue>
    {
        public static SummaryMetric Create(string name, double count, double sum, double min, double max)
        {
            return new SummaryMetric(name, count, sum, min, max);
        }

        private SummaryMetric(string name, double count, double sum, double min, double max)
            : this(name, new MetricSummaryValue(count, sum, min, max))
        {
        }

        public static SummaryMetric Create(string name, double count, double sum)
        {
            return new SummaryMetric(name, count, sum);
        }

        private SummaryMetric(string name, double count, double sum)
            : this(name, new MetricSummaryValue(count, sum))
        {
        }

        public static SummaryMetric Create(string name, MetricSummaryValue value)
        {
            return new SummaryMetric(name, value);
        }

        private SummaryMetric(string name, MetricSummaryValue value) : base(name, value)
        {
            Name = name;
        }

        public override string Type => "summary";
    }
}
