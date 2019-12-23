using System.Collections.Generic;
using System.Runtime.Serialization;
using Utf8Json;
using Utf8Json.Resolvers;
using System.Linq;
//using Newtonsoft.Json;

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
            // Utf8Json does not serialize the derived Metric classes, 
            // so Value is required in the Metric base class as type object since it
            // has different type depending on Metric type, type checking done in 
            // MetricBuilder.WithValue()
            // Newtonsoft does serialize derived classes,
            // So will allow strong type for Value in derived classes,
            // but cannot be deserialized with System.Text.Json, used in TestHelper.

            //return Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Utf8Json.JsonSerializer.ToJsonString(new[] { this }, 
                StandardResolver.ExcludeNullCamelCase);
        }
    }
}
