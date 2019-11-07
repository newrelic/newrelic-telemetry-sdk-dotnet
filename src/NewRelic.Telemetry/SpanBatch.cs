using System.Collections.Generic;

namespace NewRelic.Telemetry
{
    public class SpanBatch
    {
        public IDictionary<string, object> Attributes { get; }

        public string TraceId { get; }

        public IList<Span> Spans { get; }

        public SpanBatch(IList<Span> spans, IDictionary<string, object> attributes)
        {
            Spans = spans;
            Attributes = attributes;
        }

        public SpanBatch(IList<Span> spans, IDictionary<string, object> attributes, string traceId) : this(spans, attributes)
        {
            TraceId = traceId;
        }
    }
}
