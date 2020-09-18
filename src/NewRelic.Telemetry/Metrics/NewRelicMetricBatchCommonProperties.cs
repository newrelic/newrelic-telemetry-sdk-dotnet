using System.Collections.Generic;
using System.Runtime.Serialization;
using NewRelic.Telemetry;

namespace NewRelic.Telemetry.Metrics
{
    public struct NewRelicMetricBatchCommonProperties
    {
        public long? Timestamp { get; }

        [DataMember(Name = "interval.ms")]
        public long? IntervalMs { get; }

        public Dictionary<string,object>? Attributes { get; private set; }

        public NewRelicMetricBatchCommonProperties(long? timestamp, long? intervalMs, Dictionary<string,object>? attributes)
        {
            this.Timestamp = timestamp;
            this.IntervalMs = intervalMs;
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
