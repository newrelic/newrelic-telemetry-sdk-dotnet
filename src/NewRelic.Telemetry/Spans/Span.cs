using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NewRelic.Telemetry.Spans
{
    public class Span
    {
        public string Id { get; internal set; }

        [DataMember(Name = "trace.id")]
        public string TraceId { get; internal set; }

        public long? Timestamp { get; internal set; }

        public Dictionary<string, object> Attributes { get; internal set; }

        internal Span()
        {
        }
    }
}
