using System.Collections.Generic;
using System.Runtime.Serialization;
using NewRelic.Telemetry;

namespace NewRelic.Telemetry.Tracing
{
    public readonly struct NewRelicSpan
    {
        public string Id { get; }

        [DataMember(Name = "trace.id")]
        public string? TraceId { get; }
        
        public long Timestamp { get; }

        public Dictionary<string, object>? Attributes { get; }

        public NewRelicSpan(string? traceId, string spanId, long timestamp, string? parentSpanId, Dictionary<string,object>? attributes)
        {
            this.Id = spanId;
            this.TraceId = traceId;
            this.Timestamp = timestamp;

            if (parentSpanId != null && !string.IsNullOrWhiteSpace(parentSpanId))
            {
                (attributes ??= new Dictionary<string, object>())[NewRelicConsts.Tracing.AttribName_ParentId] = parentSpanId;
            }

            this.Attributes = attributes;
        }
    }
}
