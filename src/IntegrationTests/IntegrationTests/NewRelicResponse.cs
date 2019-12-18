using Newtonsoft.Json;
using System.Collections.Generic;

namespace IntegrationTests
{
    [JsonObject]
    public class NewRelicResponse
    {
        [JsonProperty("results")]
        public List<Result> Results { get; set; }
    }

    [JsonObject]
    public class Result
    {
        [JsonProperty("events")]
        public List<NewRelicEvent> Events { get; set; }
    }

    [JsonObject]
    public class NewRelicEvent
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
}
