using System;
using System.Collections.Generic;
using System.Linq;

namespace NewRelic.Telemetry.Spans
{
    /// <summary>
    /// Used to create batches/collections of spans to be sent to the New Relic Endpoint.
    /// </summary>
    public class SpanBatchBuilder
    {
        private readonly SpanBatch _spanBatch;

        /// <summary>
        /// Creates a new SpanBatch.
        /// </summary>
        /// <returns></returns>
        public static SpanBatchBuilder Create()
        {
            return new SpanBatchBuilder();
        }

        private SpanBatchBuilder()
        {
            _spanBatch = new SpanBatch();
        }

        /// <summary>
        /// Returns a completed SpanBatch to the caller.
        /// </summary>
        /// <returns></returns>
        public SpanBatch Build()
        {
            return _spanBatch;
        }

        private SpanBatchCommonProperties _commonProperties => _spanBatch.CommonProperties ?? (_spanBatch.CommonProperties = new SpanBatchCommonProperties());

        private Dictionary<string, object> _attributes => _commonProperties.Attributes ?? (_spanBatch.CommonProperties.Attributes = new Dictionary<string, object>());

        private List<Span> _spans => _spanBatch.Spans ?? (_spanBatch.Spans = new List<Span>());

        /// <summary>
        /// Setting the traceId for the SpanBatch indicates that all spans being reported are from 
        /// the same trace/operation.  New Relic will use this to group the spans together.
        /// The traceId should be an identifier that is unique across all operations being performed.
        /// Alternatively, the traceId can be specified on each span individually.
        /// </summary>
        /// <param name="traceId">the unique identifier for the group of spans.</param>
        /// <returns></returns>
        public SpanBatchBuilder WithTraceId(string traceId)
        {
            if (string.IsNullOrWhiteSpace(traceId))
            {
                return this;
            }

            _commonProperties.TraceId = traceId;

            return this;
        }

        /// <summary>
        /// Used to set the value of a custom attribute that is common to all spans being reported 
        /// as part of this SpanBatch.
        /// </summary>
        /// <param name="attribName">The name of the attribute.  If the name is already used, this operation will overwrite the existing value</param>
        /// <param name="attribValue">The value of the attribute.</param>
        /// <returns></returns>
        public SpanBatchBuilder WithAttribute(string attribName, object attribValue)
        {
            if (string.IsNullOrWhiteSpace(attribName))
            {
                throw new InvalidOperationException($"{nameof(attribName)} cannot be empty.");
            }

            _attributes[attribName] = attribValue;

            return this;
        }

        /// <summary>
        /// Used to apply a series of custom attributes values that are common to all spans being reported
        /// as part of this SpanBatch.
        /// </summary>
        /// <param name="attributes">Collection of Key/Value pairs of attributes.  The keys should be unique.</param>
        /// <returns></returns>
        public SpanBatchBuilder WithAttributes(ICollection<KeyValuePair<string, object>> attributes)
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

        /// <summary>
        /// Adds a single span to this batch.
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public SpanBatchBuilder WithSpan(Span span)
        {
            if (span == null)
            {
                return this;
            }

            _spans.Add(span);
            return this;
        }

        /// <summary>
        /// Adds one or many spans to this batch.
        /// </summary>
        /// <param name="spans"></param>
        /// <returns></returns>
        public SpanBatchBuilder WithSpans(params Span[] spans)
        {
            return WithSpans(spans as IEnumerable<Span>);
        }

        /// <summary>
        /// Adds a collection of spans to this batch.
        /// </summary>
        /// <param name="spans"></param>
        /// <returns></returns>
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
