using System.Collections.Generic;
using System.Runtime.Serialization;
//using Newtonsoft.Json;

namespace NewRelic.Telemetry.Metrics
{
    [DataContract]
    public class MetricSummaryValue
    {
        //TODO:  Figure out how a caller instantiates one of these

        public double Count { get; set; }
        public double Sum { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
    }

    public abstract class Metric
    {
        /// <summary>
        /// TODO
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// TODO
        /// Type is not required. Defaults to 'gauge' in NR backend.
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// TODO
        /// </summary>
        /// 
        // TODO: should this required field be nullable?
        public long? Timestamp { get; set; }

        /// <summary>
        /// Optional:  TODO
        /// </summary>
        /// <param name="intervalMs">TODO</param>
        /// <returns></returns>
        /// 
        // not needed for Gauge, can be Common field or in the Metric
        public long? IntervalMs { get; set; }

        [DataMember(Name = "value")]
        public abstract object MetricValue { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public Dictionary<string, object> Attributes { get; set; }

        protected Metric()
        {
        }
    }

    public abstract class Metric<T> : Metric
    {
        // Utf8Json does not serialize the derived Metric classes, 
        // so Value is required in the Metric base class as type object since it
        // has different type depending on Metric type, type checking done in 
        // MetricBuilder.WithValue()
        // Newtonsoft does serialize derived classes,
        // So will allow strong type for Value in derived classes,
        // but cannot be deserialized with System.Text.Json, used in TestHelper.
        internal T Value { get; set; }

        public override object MetricValue => Value;
    }

    public class CountMetric : Metric<double>
    {
        public override string Type => "count";

        public CountMetric()
        {
        }
    }

    public class GaugeMetric : Metric<double>
    {
        public override string Type => "gauge";

        public GaugeMetric()
        {
        }
    }

    public class SummaryMetric : Metric<MetricSummaryValue>
    {
        public override string Type => "summary";

        public SummaryMetric()
        {
        }
    }
}
