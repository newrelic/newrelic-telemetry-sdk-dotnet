using System.Collections.Generic;
using System.Runtime.Serialization;
using Utf8Json.Resolvers;
using System.Linq;
using System;
using NewRelic.Telemetry.Extensions;

namespace NewRelic.Telemetry.Metrics
{
    public class MetricBatch : ITelemetryDataType<MetricBatch>
    {
        public static MetricBatch Create()
        {
            return new MetricBatch();
        }

        /// <summary>
        /// Properties that are common to all metrics being submitted as part of this MetricBatch.
        /// </summary>
        [DataMember(Name = "common")]
        public MetricBatchCommonProperties? CommonProperties { get; private set; }

        /// <summary>
        /// The collection of metrics being reported in this batch.
        /// </summary>
        public List<Metric>? Metrics { get; private set; }

        public string ToJson()
        {
            return Utf8Json.JsonSerializer.ToJsonString(new[] { this },
                StandardResolver.ExcludeNullCamelCase);
        }

        private MetricBatchCommonProperties EnsureCommonProperties()
        {
            return CommonProperties ?? (CommonProperties = new MetricBatchCommonProperties());
        }

        private Dictionary<string, object> EnsureBatchAttributes()
        {
            return EnsureCommonProperties().EnsureAttributes();
        }

        private List<Metric> EnsureMetrics()
        {
            return Metrics ??= new List<Metric>();
        }

        /// <summary>
        /// Identifies the metrics start time for all of the metrics that are part 
        /// of this batch.
        /// </summary>
        /// <param name="timestamp">Should be reported in UTC.</param>
        public MetricBatch WithTimestamp(DateTime timestamp)
        {
            if (timestamp == default)
            {
                return this;
            }

            EnsureCommonProperties().Timestamp = DateTimeExtensions.ToUnixTimeMilliseconds(timestamp);

            return this;
        }

        /// <summary>
        /// Used in conjunction with <see cref="WithTimestamp(DateTime)"/>, identifies the duration of 
        /// the observation window for this metric batch.
        /// </summary>
        /// <param name="intervalMs">The number of milliseconds</param>
        public MetricBatch WithIntervalMs(long intervalMs)
        {
            if (intervalMs == default)
            {
                return this;
            }

            EnsureCommonProperties().IntervalMs = intervalMs;

            return this;
        }

        /// <summary>
        /// Used to set the value of a custom attribute that is common to all metrics being reported 
        /// as part of this MetricBatch.
        /// </summary>
        /// <param name="attribName">Required: The name of the attribute.  If the name is already used, this operation will overwrite any existing value.</param>
        /// <param name="attribValue">The value of the attribute.  A NULL value will NOT be reported to the New Relic endpoint.</param>
        public MetricBatch WithAttribute(string attribName, object attribValue)
        {
            if (string.IsNullOrWhiteSpace(attribName))
            {
                throw new InvalidOperationException($"{nameof(attribName)} cannot be empty.");
            }

            EnsureBatchAttributes()[attribName] = attribValue;

            return this;
        }

        /// <summary>
        /// Used to apply a set of custom attribute values that are common to all metrics being reported
        /// as part of this MetricBatch.
        /// </summary>
        /// <param name="attributes">Collection of Key/Value pairs of attributes.  The keys should be unique.  
        /// In the event of duplicate keys, the last value will be accepted.</param>
        /// <returns></returns>
        public MetricBatch WithAttributes(ICollection<KeyValuePair<string, object>> attributes)
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

        /// <summary>
        /// Adds a single Metric to this MetricBatch.
        /// </summary>
        /// <param name="metric"></param>
        public MetricBatch WithMetric(Metric metric)
        {
            if (metric == null)
            {
                return this;
            }

            EnsureMetrics().Add(metric);

            return this;
        }

        /// <summary>
        /// Adds one or many metrics to this batch.
        /// </summary>
        /// <param name="metrics"></param>
        public MetricBatch WithMetrics(params Metric[] metrics)
        {
            return WithMetrics(metrics as IEnumerable<Metric>);
        }

        /// <summary>
        /// Adds a collection of spans to this batch.
        /// </summary>
        /// <param name="spans"></param>
        public MetricBatch WithMetrics(IEnumerable<Metric> metrics)
        {
            if (metrics == null)
            {
                return this;
            }

            foreach (var metric in metrics)
            {
                WithMetric(metric);
            }

            return this;
        }

        private MetricBatch WithCommonProperties(MetricBatchCommonProperties commonProperties)
        {
            CommonProperties = commonProperties;
            return this;
        }

        private static readonly List<MetricBatch> _emptyMetricBatchArray = new List<MetricBatch>();

        public List<MetricBatch> Split()
        {
            var countMetrics = Metrics?.Count;
            if (countMetrics == null || countMetrics <= 1)
            {
                return _emptyMetricBatchArray;
            }

            var targetMetricCount = countMetrics.Value / 2;
            var batch0Metrics = Metrics.Take(targetMetricCount).ToList();
            var batch1Metrics = Metrics.Skip(targetMetricCount).ToList();

            var result = new List<MetricBatch>();

            if (batch0Metrics.Count > 0)
            {
                result.Add(new MetricBatch().WithMetrics(batch0Metrics));
            }

            if (batch1Metrics.Count > 0)
            {
                result.Add(new MetricBatch().WithMetrics(batch1Metrics));
            }

            if (CommonProperties != null)
            {
                foreach (var MetricBatch in result)
                {
                    MetricBatch.WithCommonProperties(CommonProperties);
                }
            }

            return result;
        }
    }
}
