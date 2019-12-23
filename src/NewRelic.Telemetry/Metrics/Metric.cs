using System.Collections.Generic;
using System.Runtime.Serialization;
//using Newtonsoft.Json;

namespace NewRelic.Telemetry.Metrics
{
    [DataContract]
    public class MetricSummaryValue
    {
        [DataMember(Name = "count")]
        public double Count;
        [DataMember(Name = "sum")]
        public double Sum;
        [DataMember(Name = "min")]
        public double Min;
        [DataMember(Name = "max")]
        public double Max;
    }

    [DataContract]
    public abstract class Metric
    {
        public const string MetricTypeCount = "count";
        public const string MetricTypeGauge = "gauge";
        public const string MetricTypeSummary = "summary";

        /// <summary>
        /// TODO
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// TODO
        /// Type is not required. Defaults to 'gauge' in NR backend.
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }

        // Utf8Json does not serialize the derived Metric classes, 
        // so Value is required in the Metric base class as type object since it
        // has different type depending on Metric type, type checking done in 
        // MetricBuilder.WithValue()
        // Newtonsoft does serialize derived classes,
        // So will allow strong type for Value in derived classes,
        // but cannot be deserialized with System.Text.Json, used in TestHelper.
        [DataMember(Name = "value")]
        public abstract object Value { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        /// 
        // TODO: should this required field be nullable?
        [DataMember(Name = "timestamp")]
//        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? Timestamp { get; set; }

        /// <summary>
        /// Optional:  TODO
        /// </summary>
        /// <param name="intervalMs">TODO</param>
        /// <returns></returns>
        /// 
        // not needed for Gauge, can be Common field or in the Metric
        [DataMember(Name = "interval.ms")]
//        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? IntervalMs { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        [DataMember(Name = "attributes")]
//        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Attributes { get; set; }

        internal Metric()
        {
            // Type defaults to Gauge
        }
    }


    [DataContract]
    public class CountMetric : Metric
    {
        /// <summary>
        /// TODO
        /// </summary>
        public override object Value { get; set; }

        // for Newtonsoft
        //[DataMember]
        //public double Value { get; set; }

        internal CountMetric()
        {
        }
    }

    [DataContract]
    public class GaugeMetric : Metric
    {
        /// <summary>
        /// TODO
        /// </summary>
        public override object Value { get; set; }

        // for Newtonsoft
        //[DataMember]
        //public double Value { get; set; }

        internal GaugeMetric()
        {
        }

    }

    [DataContract]
    public class SummaryMetric : Metric
    {
        public override object Value { get; set; }

        // for Newtonsoft
        //[DataMember]
        //public MetricSummaryValue Value { get; set; }

        internal SummaryMetric()
        {
        }
    }

}
