using System;
using System.Collections.Generic;

namespace NewRelic.Telemetry.Spans
{
    public class SpanBatchBuilder
    {
        private readonly SpanBatch _spanBatch;

        public static SpanBatchBuilder Create()
        {
            return new SpanBatchBuilder();
        }

        private SpanBatchBuilder()
        {
           _spanBatch = new SpanBatch();
        }

        public SpanBatch Build()
        {
            return _spanBatch;
        }

        private SpanBatchCommonProperties _commonProperties => _spanBatch.CommonProperties ?? (_spanBatch.CommonProperties = new SpanBatchCommonProperties());

        private Dictionary<string, object> _attributes => _commonProperties.Attributes ?? (_spanBatch.CommonProperties.Attributes = new Dictionary<string, object>());

        private List<Span> _spans => _spanBatch.Spans ?? (_spanBatch.Spans = new List<Span>());

        public SpanBatchBuilder WithTraceId(string traceId)
        {
            if(string.IsNullOrWhiteSpace(traceId))
            {
                return this;
            }

            _commonProperties.TraceId = traceId;

            return this;
        }

        public SpanBatchBuilder WithAttribute(string attribName, object attribValue)
        {
            if (string.IsNullOrWhiteSpace(attribName))
            {
                var ex = new InvalidOperationException($"{nameof(attribName)} cannot be empty.");
                Logging.LogException(ex);
                throw ex;
            }

            _attributes[attribName] = attribValue;

            return this;
        }

        public SpanBatchBuilder WithAttributes(ICollection<KeyValuePair<string,object>> attributes)
        {
            if (attributes == null)
            {
                return this;
            }

            foreach (var attrib in attributes)
            {
                WithAttribute(attrib.Key, attrib.Value);
            }

            return this;
        }

        public SpanBatchBuilder WithSpan(Span span)
        {
            if(span == null)
            {
                return this;
            }

            _spans.Add(span);
            return this;
        }

        public SpanBatchBuilder WithSpans(params Span[] spans)
        {
            return WithSpans(spans as IEnumerable<Span>);
        }

        public SpanBatchBuilder WithSpans(IEnumerable<Span> spans)
        {
            if (spans == null)
            {
                return this;
            }

            foreach (var span in spans)
            {
                WithSpan(span);
            }

            return this;
        }

    }
}
