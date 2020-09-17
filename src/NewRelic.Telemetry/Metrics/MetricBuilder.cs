// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using NewRelic.Telemetry.Extensions;

namespace NewRelic.Telemetry.Metrics
{
    /// <summary>
    /// Helper class that is used to create new metrics.
    /// </summary>
    public class MetricBuilder
    {
        public static MetricBuilder<CountMetric, double> CreateCountMetric(string name)
        {
            return new MetricBuilder<CountMetric, double>(name);
        }

        public static MetricBuilder<GaugeMetric, double> CreateGaugeMetric(string name)
        {
            return new MetricBuilder<GaugeMetric, double>(name);
        }

        public static MetricBuilder<SummaryMetric, MetricSummaryValue> CreateSummaryMetric(string name)
        {
            return new MetricBuilder<SummaryMetric, MetricSummaryValue>(name);
        }

        protected MetricBuilder()
        {
        }
    }

    /// <summary>
    /// Helper class that is used to create new type-specific metrics.
    /// </summary>
    /// <typeparam name="TMetric">The type of metric being built (Count, Gauge, Summary).</typeparam>
    /// <typeparam name="TValue">The value of the type of metric being reported.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Having this class in teh same file improves readability.")]
    public class MetricBuilder<TMetric, TValue> : MetricBuilder
        where TMetric : Metric<TValue>, new()
    {
        private readonly TMetric _metric;

        private Dictionary<string, object> _attributes => _metric.Attributes ?? (_metric.Attributes = new Dictionary<string, object>());

        internal MetricBuilder(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            _metric = new TMetric
            {
                Name = name,
            };
        }

        /// <summary>
        /// Identifies the start time for all of the observations that are being
        /// aggregated as part of this metric.  For gauge metrics, this value
        /// represents the time at which the observation was made.
        /// </summary>
        /// <param name="timestamp">Should be reported in UTC.</param>
        public MetricBuilder<TMetric, TValue> WithTimestamp(DateTime timestamp)
        {
            _metric.Timestamp = DateTimeExtensions.ToUnixTimeMilliseconds(timestamp);
            return this;
        }

        /// <summary>
        /// Sets the value of the metric.
        /// </summary>
        public MetricBuilder<TMetric, TValue> WithValue(TValue value)
        {
            _metric.Value = value;
            return this;
        }

        /// <summary>
        /// For Count and Summary Metrics, identifies the duration of the observation window.  Used in conjunction with
        /// <see cref="WithTimestamp(DateTime)"/> to describe an observation window
        /// for the values being reported.
        /// 
        /// </summary>
        /// <param name="intervalMs">The number of milliseconds.</param>
        /// <returns></returns>
        public MetricBuilder<TMetric, TValue> WithIntervalMs(long intervalMs)
        {
            _metric.IntervalMs = intervalMs;
            return this;
        }

        /// <summary>
        /// Allows custom attribution of the Metric to provide additional contextual
        /// information for later analysis.  
        /// </summary>
        /// <param name="attributes">Key/Value pairs representing the custom attributes.  In the event of a duplicate key, the last value will be used.</param>
        public MetricBuilder<TMetric, TValue> WithAttributes<T>(IEnumerable<KeyValuePair<string, T>> attributes)
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
        /// Allows custom attribution of the Metric.
        /// </summary>
        /// <param name="attribName">Name of the attribute.  If an attribute with this name already exists, the previous value will be overwritten.</param>
        /// <param name="attribVal">Value of the attribute.</param>
        public MetricBuilder<TMetric, TValue> WithAttribute<T>(string attribName, T attribVal)
        {
            if (string.IsNullOrWhiteSpace(attribName))
            {
                throw new InvalidOperationException($"{nameof(attribName)} cannot be empty.");
            }

            _attributes[attribName] = attribVal;
            return this;
        }

        /// <summary>
        /// Returns the built Metric.
        /// </summary>
        public TMetric Build()
        {
            return _metric;
        }
    }
}
