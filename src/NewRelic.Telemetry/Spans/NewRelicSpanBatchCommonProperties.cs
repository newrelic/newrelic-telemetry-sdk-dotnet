using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NewRelic.Telemetry.Tracing
{
    public struct NewRelicSpanBatchCommonProperties
    {
        [DataMember(Name = "trace.id")]
        public string? TraceId { get; }

        public Dictionary<string, object>? Attributes { get; private set; }

        public NewRelicSpanBatchCommonProperties(string? traceId, Dictionary<string, object>? attributes)
        {
            this.TraceId = traceId;
            this.Attributes = attributes;
        }

        public void SetInstrumentationProvider(string instrumentationProvider)
        {
            if (string.IsNullOrWhiteSpace(instrumentationProvider))
            {
                return;
            }

            if (Attributes == null)
            {
                Attributes = new Dictionary<string, object>();
            }

            Attributes[NewRelicConsts.AttribName_InstrumentationProvider] = instrumentationProvider;
        }
    }
}
