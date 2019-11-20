using System.Collections.Generic;
using System.Runtime.Serialization;
using Utf8Json;
using Utf8Json.Resolvers;
using System.Linq;

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

        internal SpanBatch(SpanBatchCommonProperties commonProperties, IEnumerable<Span> spans)
        {
            CommonProperties = commonProperties;
            Spans = spans.ToList();
        }

        public string ToJson()
        {
            return JsonSerializer.ToJsonString(new[] { this }, StandardResolver.ExcludeNullCamelCase);
        }
    }


}
