using System;
using System.Collections.Generic;

namespace NewRelic.Telemetry.Sdk
{
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
                throw new NullReferenceException("id is not set.");
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
