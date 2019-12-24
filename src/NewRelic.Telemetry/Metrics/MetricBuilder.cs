using System;
using System.Collections.Generic;
using NewRelic.Telemetry.Extensions;

namespace NewRelic.Telemetry.Metrics
{
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
    }


    /// <summary>
    /// Helper class that is used to create new metrics.
    /// </summary>
    /// 
    public class MetricBuilder<TMetric, TValue>  : MetricBuilder
        where TMetric:Metric<TValue>, new()
    {
        private readonly TMetric _metric;

        private Dictionary<string, object> _attributes => _metric.Attributes ?? (_metric.Attributes = new Dictionary<string, object>());

        internal MetricBuilder(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            _metric = new TMetric
            {
                Name = name
            };
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="timestamp"></param>
        public MetricBuilder<TMetric, TValue> WithTimestamp(DateTime timestamp)
        {
            _metric.Timestamp = DateTimeExtensions.ToUnixTimeMilliseconds(timestamp);
            return this;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public MetricBuilder<TMetric, TValue> WithValue(TValue value)
        {

            _metric.Value = value;

            return this;
        }

        /// <summary>
        /// Optional:  TODO
        /// </summary>
        /// <param name="intervalMs">TODO</param>
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
        public MetricBuilder<TMetric, TValue> WithAttributes<T>(IEnumerable<KeyValuePair<string,T>> attributes)
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
