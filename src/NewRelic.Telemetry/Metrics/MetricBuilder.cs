using System;
using System.Collections.Generic;
using NewRelic.Telemetry.Extensions;

namespace NewRelic.Telemetry.Metrics
{
    /// <summary>
    /// Helper class that is used to create new metrics.
    /// </summary>
    /// 
    public class MetricBuilder
    {
        // TODO: intrinsic attrs for Metric
        //internal const string attribName_ServiceName = "service.name";
        //internal const string attribName_DurationMs = "duration.ms";
        //internal const string attribName_Name = "name";
        //internal const string attribName_ParentID = "parent.id";
        //internal const string attribName_Error = "error";

        /// <summary>
        /// TODO
        /// </summary>
        public static MetricBuilder Create(string name, string type)
        {
            return new MetricBuilder(name, type);
        }

        private readonly Metric _Metric;

        private Dictionary<string, object> _attributes => _Metric.Attributes ?? (_Metric.Attributes = new Dictionary<string, object>());

        private MetricBuilder(string name, string type)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new NullReferenceException("Metric name is not set.");
            }

            switch (type)
            {
                case "count":
                    _Metric = new CountMetric();
                    _Metric.Type = type;
                    break;

                case "summary":
                    _Metric = new SummaryMetric();
                    _Metric.Type = type;
                    break;

                default:
                    _Metric = new GaugeMetric();
                    _Metric.Type = "gauge";
                    break;
            }

            _Metric.Name = name;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="timestamp"></param>
        public MetricBuilder WithTimestamp(DateTime timestamp)
        {
            _Metric.Timestamp = DateTimeExtensions.ToUnixTimeMilliseconds(timestamp);
            return this;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public MetricBuilder WithValue(double value)
        {
            //if (_Metric is CountMetric || _Metric is GaugeMetric)
            //{
            //    _Metric.Value = value;
            //}
            if (_Metric is CountMetric)
            {
                ((CountMetric)_Metric).Value = value;
            }
            if (_Metric is GaugeMetric)
            {
                ((GaugeMetric)_Metric).Value = value;
            }
            else
            {
                // TODO: log an error
            }

            return this;
        }

        public MetricBuilder WithValue(MetricSummaryValue value)
        {
            if (_Metric is SummaryMetric)
            {
                ((SummaryMetric)_Metric).Value = value;
            }
            else
            {
                // TODO: log an error
            }

            return this;
        }

        /// <summary>
        /// Optional:  TODO
        /// </summary>
        /// <param name="intervalMs">TODO</param>
        /// <returns></returns>
        public MetricBuilder WithIntervalMs(long intervalMs)
        {
            _Metric.IntervalMs = intervalMs;
            return this;
        }

        ///// <summary>
        ///// TODO: not sure if this is needed;
        ///// </summary>
        ///// <param name="serviceName"></param>
        //public MetricBuilder WithServiceName(string serviceName)
        //{
        //    WithAttribute(attribName_ServiceName, serviceName);
        //    return this;
        //}

        /// <summary>
        /// Allows custom attribution of the Metric to provide additional contextual
        /// information for later analysis.  
        /// </summary>
        /// <param name="attributes">Key/Value pairs representing the custom attributes.  In the event of a duplicate key, the last value will be used.</param>
        public MetricBuilder WithAttributes<T>(IEnumerable<KeyValuePair<string,T>> attributes)
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
        public MetricBuilder WithAttribute<T>(string attribName, T attribVal)
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
        public Metric Build()
        {
            return _Metric;
        }
    }
}
