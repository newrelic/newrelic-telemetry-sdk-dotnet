using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NewRelic.Telemetry.Spans
{
    public class SpanBatchCommonProperties
    {
        [DataMember(Name = "trace.id")]
        public string TraceId { get; internal set; }

        public Dictionary<string,object> Attributes { get; internal set; }

        internal SpanBatchCommonProperties()
        {
        }
    }


}
