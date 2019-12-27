using System;
using System.Collections.Generic;
using NewRelic.Telemetry.Extensions;

namespace NewRelic.Telemetry.Metrics
{
    /// <summary>
    /// Used to create batches/collections of metrics to be sent to the New Relic Endpoint.
    /// </summary>
    public class MetricBatchBuilder
    {
        private readonly MetricBatch _metricBatch;

        /// <summary>
        /// Creates a new MetricBatch Builder.
        /// </summary>
        /// <returns></returns>
        public static MetricBatchBuilder Create()
        {
            return new MetricBatchBuilder();
        }

        private MetricBatchBuilder()
        {
            _metricBatch = new MetricBatch();
        }

        /// <summary>
        /// Returns the built MetricBatch to the caller.
        /// </summary>
        /// <returns></returns>
        public MetricBatch Build()
        {
            return _metricBatch;
        }

        private MetricBatchCommonProperties _commonProperties => _metricBatch.CommonProperties ?? (_metricBatch.CommonProperties = new MetricBatchCommonProperties());

        private Dictionary<string, object> _attributes => _commonProperties.Attributes ?? (_metricBatch.CommonProperties.Attributes = new Dictionary<string, object>());

        private List<Metric> _metrics => _metricBatch.Metrics ?? (_metricBatch.Metrics = new List<Metric>());

        /// <summary>
        /// Optional:  TODO
        /// </summary>
        /// <param name="timestamp">Unix timestamp value ms precision.  Should be reported in UTC.</param>
        /// <returns></returns>
        public MetricBatchBuilder WithTimestamp(DateTime timestamp)
        {
            if (timestamp == default)
            {
                return this;
            }

            _commonProperties.Timestamp = DateTimeExtensions.ToUnixTimeMilliseconds(timestamp);

            return this;
        }

        /// <summary>
        /// Optional:  TODO
        /// </summary>
        /// <param name="intervalMs">TODO</param>
        /// <returns></returns>
        public MetricBatchBuilder WithIntervalMs(long intervalMs)
        {
            if (intervalMs == default)
            {
                return this;
            }

            _commonProperties.IntervalMs = intervalMs;

            return this;
        }

        /// <summary>
        /// Used to set the value of a custom attribute that is common to all metrics being reported 
        /// as part of this MetricBatch.
        /// </summary>
        /// <param name="attribName">Required: The name of the attribute.  If the name is already used, this operation will overwrite any existing value.</param>
        /// <param name="attribValue">The value of the attribute.  A NULL value will NOT be reported to the New Relic endpoint.</param>
        /// <returns></returns>
        public MetricBatchBuilder WithAttribute(string attribName, object attribValue)
        {
            if (string.IsNullOrWhiteSpace(attribName))
            {
                throw new InvalidOperationException($"{nameof(attribName)} cannot be empty.");
            }

            _attributes[attribName] = attribValue;

            return this;
        }

        /// <summary>
        /// Used to apply a set of custom attribute values that are common to all metrics being reported
        /// as part of this MetricBatch.
        /// </summary>
        /// <param name="attributes">Collection of Key/Value pairs of attributes.  The keys should be unique.  
        /// In the event of duplicate keys, the last value will be accepted.</param>
        /// <returns></returns>
        public MetricBatchBuilder WithAttributes(ICollection<KeyValuePair<string, object>> attributes)
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
        /// <returns></returns>
        public MetricBatchBuilder WithMetric(Metric metric)
        {
            if (metric == null)
            {
                return this;
            }

            _metrics.Add(metric);
            return this;
        }

        /// <summary>
        /// Adds one or many metrics to this batch.
        /// </summary>
        /// <param name="metrics"></param>
        /// <returns></returns>
        public MetricBatchBuilder WithMetrics(params Metric[] metrics)
        {
            return WithMetrics(metrics as IEnumerable<Metric>);
        }

        /// <summary>
        /// Adds a collection of spans to this batch.
        /// </summary>
        /// <param name="spans"></param>
        /// <returns></returns>
        public MetricBatchBuilder WithMetrics(IEnumerable<Metric> metrics)
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
    }
}
