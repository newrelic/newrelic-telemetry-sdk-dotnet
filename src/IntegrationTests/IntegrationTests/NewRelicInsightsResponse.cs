using Newtonsoft.Json;
using System.Collections.Generic;

namespace IntegrationTests
{
    [JsonObject]
    public class NewRelicInsightsResponse<T>
    {
        [JsonProperty("results")]
        public List<Result<T>> Results { get; set; }
    }

    [JsonObject]
    public class Result<T>
    {
        [JsonProperty("events")]
        public List<T> Events { get; set; }
    }

    [JsonObject]
    public class NewRelicSpanEvent
    {
        [JsonProperty("entityName")]
        public string EntityName { get; set; }

        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("service.name")]
        public string ServiceName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("trace.id")]
        public string TraceId { get; set; }
    }

    [JsonObject]
    public class NewRelicMetricEvent
    {
        [JsonProperty("metricName")]
        public string MetricName { get; set; }

        [JsonProperty("newrelic.source")]
        public string NewRelicSource { get; set; }

        [JsonProperty("timestamp")]
        public long TimeStamp { get; set; }
    }
}
