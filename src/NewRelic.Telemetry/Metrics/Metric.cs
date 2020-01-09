using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NewRelic.Telemetry.Metrics
{
    [DataContract]
    public class MetricSummaryValue
    {
        public double Count { get; private set; }
        public double Sum { get; private set; }
        public double? Min { get; private set; }
        public double? Max { get; private set; }

        private MetricSummaryValue()
        {
        }

        public static MetricSummaryValue Create(double count, double sum, double min, double max)
        {
            var result = Create(count, sum);

            result.Min = min;
            result.Max = max;

            return result;
        }

        public static MetricSummaryValue Create(double count, double sum)
        {
            return new MetricSummaryValue()
            {
                Count = count,
                Sum = sum,
            };
        }
    }

    public abstract class Metric
    {
        /// <summary>
        /// TODO
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// TODO
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// TODO
        /// </summary>
        /// 
        public long? Timestamp { get; internal set; }

        /// <summary>
        /// Optional:  TODO
        /// </summary>
        /// <param name="intervalMs">TODO</param>
        /// <returns></returns>
        public long? IntervalMs { get; internal set; }

        protected string JasonNM { get; set; }

        protected string testPropertyID => IntervalMs.ToString();


        [DataMember(Name = "value")]
        public abstract object MetricValue { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public Dictionary<string, object> Attributes { get; internal set; }
    }

    public abstract class Metric<T> : Metric
    {
        [IgnoreDataMember]
        public T Value { get; internal set; }

        public override object MetricValue => Value;
    }

    public class CountMetric : Metric<double>
    {
        public override string Type => "count";
    }

    public class GaugeMetric : Metric<double>
    {
        public override string Type => "gauge";
    }

    public class SummaryMetric : Metric<MetricSummaryValue>
    {
        public override string Type => "summary";
    }
}
