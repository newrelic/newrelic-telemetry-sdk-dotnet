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

        private Span(string id, string traceId, long timestamp, string serviceName, double durationMs, string name, string parentId, bool error, IDictionary<string, object> attributes)
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

        public static SpanBuilder GetSpanBuilder(string id) 
        {
            return new SpanBuilder(id);
        }

        public class SpanBuilder
        {
            private string _id;
            private IDictionary<string, object> _attributes;

            private string _traceId;
            private long _timestamp;

            private string _serviceName;
            private double _durationMs;
            private string _name;
            private string _parentId;
            private bool _error;

            public SpanBuilder(string spanId)
            {
                _id = spanId;
            }

            public Span Build()
            {
                if (string.IsNullOrEmpty(_id))
                {
                    //TODO: Log out "can not create span with a null id"  message 
                    return null;
                }

                return new Span(_id, _traceId, _timestamp, _serviceName, _durationMs, _name, _parentId, _error, _attributes);
            }

            public SpanBuilder TraceId(string traceId)
            {
                _traceId = traceId;
                return this;
            }

            public SpanBuilder TimeStamp(long timestamp)
            {
                _timestamp = timestamp;
                return this;
            }

            public SpanBuilder ServiceName(string serviceName)
            {
                _serviceName = serviceName;
                return this;
            }

            public SpanBuilder DurationMs(double durationMs)
            {
                _durationMs = durationMs;
                return this;
            }

            public SpanBuilder Name(string name)
            {
                _name = name;
                return this;
            }

            public SpanBuilder ParentId(string parentId)
            {
                _parentId = parentId;
                return this;
            }

            public SpanBuilder Error(bool b)
            {
                _error = b;
                return this;
            }

            public SpanBuilder Attributes(IDictionary<string, object> attributes)
            {
                _attributes = attributes;
                return this;
            }
        }
    }
}
