using System.Collections.Generic;
using System.Runtime.Serialization;

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
