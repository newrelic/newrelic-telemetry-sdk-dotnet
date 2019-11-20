using System.Collections.Generic;
using System.Linq;

namespace NewRelic.Telemetry.Spans
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

        public static SpanBatch[] Split(SpanBatch batch)
        {
            var countSpans = batch.Spans.Count;
            if(countSpans <= 1)
            {
                return null;
            }

            var targetSpanCount = countSpans / 2;
            var batch0Spans = batch.Spans.Take(targetSpanCount).ToList();
            var batch1Spans = batch.Spans.Skip(targetSpanCount).ToList();

            var batch0 = new SpanBatch(batch0Spans, batch.Attributes, batch.TraceId);
            var batch1 = new SpanBatch(batch1Spans, batch.Attributes, batch.TraceId);

            return new[]{ batch0, batch1 };
        }

    }
}
