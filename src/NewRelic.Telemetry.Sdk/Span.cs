using System.Collections.Generic;

namespace NewRelic.Telemetry.Sdk
{
    public class Span
    {
        public string Id { get; }

        public string TraceId { get; }

        public long Timestamp { get; }

        public string ServiceName { get; }

        public double DurationMs { get; }

        public string Name { get; }

        public string ParentId { get; }

        public bool Error { get; }

        public IDictionary<string, object> Attributes { get; }

        internal Span(string id, string traceId, long timestamp, string serviceName, double durationMs, string name, string parentId, bool error, IDictionary<string, object> attributes)
        {
            Id = id;
            TraceId = traceId;
            Timestamp = timestamp;
            ServiceName = serviceName;
            DurationMs = durationMs;
            Name = name;
            ParentId = parentId;
            Error = error;
            Attributes = attributes;
        }
    }
}
