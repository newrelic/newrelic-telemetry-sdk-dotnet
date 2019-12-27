using System.Collections.Generic;
using System.Runtime.Serialization;
using Utf8Json.Resolvers;
using System.Linq;


namespace NewRelic.Telemetry.Metrics
{
    [DataContract]
    public class MetricBatch : ITelemetryDataType
    {
        /// <summary>
        /// Properties that are common to all metrics being submitted as part of this MetricBatch.
        /// </summary>
        [DataMember(Name = "common")]
        public MetricBatchCommonProperties CommonProperties { get; internal set; }

        /// <summary>
        /// TODO
        /// </summary>
        public List<Metric> Metrics { get; internal set; }

        internal MetricBatch()
        {
        }

        internal MetricBatch(MetricBatchCommonProperties commonProperties, IEnumerable<Metric> metrics)
        {
            CommonProperties = commonProperties;
            Metrics = metrics.ToList();
        }

        public string ToJson()
        {
            return Utf8Json.JsonSerializer.ToJsonString(new[] { this }, 
                StandardResolver.ExcludeNullCamelCase);
        }
    }
}
