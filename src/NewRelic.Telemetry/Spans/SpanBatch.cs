using System.Collections.Generic;
using System.Runtime.Serialization;
using Utf8Json;
using Utf8Json.Resolvers;

namespace NewRelic.Telemetry.Spans
{
    public class SpanBatch
    {
        [DataMember(Name = "common")]
        public SpanBatchCommonProperties CommonProperties { get; internal set; }

        public List<Span> Spans { get; internal set; }

        internal SpanBatch()
        {
        }

        public string ToJson()
        {
            return JsonSerializer.ToJsonString(new[] { this }, StandardResolver.ExcludeNullCamelCase);
        }
    }


}
