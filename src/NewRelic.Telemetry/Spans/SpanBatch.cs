using System.Collections.Generic;
using System.Runtime.Serialization;
using Utf8Json;
using Utf8Json.Resolvers;
using System.Linq;
using NewRelic.Telemetry.Transport;
using System;

namespace NewRelic.Telemetry.Spans
{
    public class SpanBatch : ITelemetryDataType<SpanBatch>
    {
        public static SpanBatch Create()
        {
            return new SpanBatch();
        }


        /// <summary>
        /// Properties that are common to all spans being submitted as part of this SpanBatch.
        /// </summary>
        [DataMember(Name = "common")]
        public SpanBatchCommonProperties? CommonProperties { get; private set; }

        private SpanBatchCommonProperties EnsureCommonProperties()
        {
            return CommonProperties ??= new SpanBatchCommonProperties();
        }

        /// <summary>
        /// The spans that are being reported as part of this batch.
        /// </summary>
        public List<Span>? Spans { get; internal set; }

        private List<Span> EnsureSpans()
        {
            return Spans ??= new List<Span>();
        }

 
        public string ToJson()
        {
            return JsonSerializer.ToJsonString(new[] { this }, StandardResolver.ExcludeNullCamelCase);
        }


        /// <summary>
        /// Optional:  Setting the traceId for the SpanBatch indicates that all spans being reported are from 
        /// the same trace/operation.  New Relic will use this to group the spans together.
        /// The traceId should be an identifier that is unique across all operations being performed.
        /// Alternatively, the traceId can be specified on each span individually.
        /// </summary>
        /// <param name="traceId">the unique identifier for the group of spans.</param>
        /// <returns></returns>
        public SpanBatch WithTraceId(string traceId)
        {
            if (string.IsNullOrWhiteSpace(traceId))
            {
                return this;
            }

            EnsureCommonProperties().TraceId = traceId;

            return this;
        }

        /// <summary>
        /// Used to set the value of a custom attribute that is common to all spans being reported 
        /// as part of this SpanBatch.
        /// </summary>
        /// <param name="attribName">Required: The name of the attribute.  If the name is already used, this operation will overwrite any existing value.</param>
        /// <param name="attribValue">The value of the attribute.  A NULL value will NOT be reported to the New Relic endpoint.</param>
        /// <returns></returns>
        public SpanBatch WithAttribute(string attribName, object attribValue)
        {
            if (string.IsNullOrWhiteSpace(attribName))
            {
                throw new InvalidOperationException($"{nameof(attribName)} cannot be empty.");
            }

            EnsureCommonProperties().EnsureAttributes()[attribName] = attribValue;

            return this;
        }

        /// <summary>
        /// Used to apply a set of custom attribute values that are common to all spans being reported
        /// as part of this SpanBatch.
        /// </summary>
        /// <param name="attributes">Collection of Key/Value pairs of attributes.  The keys should be unique.  
        /// In the event of duplicate keys, the last value will be accepted.</param>
        /// <returns></returns>
        public SpanBatch WithAttributes(ICollection<KeyValuePair<string, object>> attributes)
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
        /// Adds a single span to this Span Batch.
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public SpanBatch WithSpan(Span span)
        {
            if (span == null)
            {
                return this;
            }

            EnsureSpans().Add(span);

            return this;
        }

        /// <summary>
        /// Adds one or many spans to this batch.
        /// </summary>
        /// <param name="spans"></param>
        /// <returns></returns>
        public SpanBatch WithSpans(params Span[] spans)
        {
            return WithSpans(spans as IEnumerable<Span>);
        }

        /// <summary>
        /// Adds a collection of spans to this batch.
        /// </summary>
        /// <param name="spans"></param>
        /// <returns></returns>
        public SpanBatch WithSpans(IEnumerable<Span> spans)
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

        private static readonly List<SpanBatch> _emptySpanBatchArray = new List<SpanBatch>();

        private SpanBatch WithCommonProperties(SpanBatchCommonProperties commonProperties)
        {
            CommonProperties = commonProperties;
            return this;
        }

        public List<SpanBatch> Split()
        {
            var countSpans = Spans?.Count;
            if (countSpans == null || countSpans <= 1)
            {
                return _emptySpanBatchArray;
            }

            var targetSpanCount = countSpans.Value / 2;
            var batch0Spans = Spans.Take(targetSpanCount).ToList();
            var batch1Spans = Spans.Skip(targetSpanCount).ToList();

            var result = new List<SpanBatch>();

            if (batch0Spans.Count > 0)
            {
                result.Add(new SpanBatch().WithSpans(batch0Spans));
            }

            if (batch1Spans.Count > 0)
            {
                result.Add(new SpanBatch().WithSpans(batch1Spans));
            }

            if (CommonProperties != null)
            {
                foreach(var spanBatch in result)
                {
                    spanBatch.WithCommonProperties(CommonProperties);
                }
            }

            return result;
        }
    }


}
