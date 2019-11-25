using System;
using System.Collections.Generic;
using NewRelic.Telemetry.Extensions;

namespace NewRelic.Telemetry.Spans
{
    /// <summary>
    /// Helper class that is used to create new spans.
    /// </summary>
    public class SpanBuilder
    {
        private const string attribName_ServiceName = "service.name";
        private const string attribName_DurationMs = "duration.ms";
        private const string attribName_Name = "name";
        private const string attribName_ParentID = "parent.id";
        private const string attribName_Error = "error";

        /// <summary>
        /// Creates a new span with a SpanId
        /// </summary>
        /// <param name="spanId">Required, unique identifier for the span being reported.  This identifier may be used to link child spans.</param>
        /// <returns></returns>
        public static SpanBuilder Create(string spanId)
        {
            return new SpanBuilder(spanId);
        }

        private readonly Span _span = new Span();

        private Dictionary<string, object> _attributes => _span.Attributes ?? (_span.Attributes = new Dictionary<string, object>());

        private SpanBuilder(string spanId)
        {
            if (string.IsNullOrEmpty(spanId))
            {
                throw new NullReferenceException("Span id is not set.");
            }

            _span.Id = spanId;
        }

        /// <summary>
        /// Returns the built span.
        /// </summary>
        /// <returns></returns>
        public Span Build()
        {
            return _span;
        }

        /// <summary>
        /// Identifies this span as part of a specific trace/operation.
        /// </summary>
        /// <param name="traceId"></param>
        /// <returns></returns>
        public SpanBuilder WithTraceId(string traceId)
        {
            _span.TraceId = traceId;
            return this;
        }

        /// <summary>
        /// Identifies the start time of the operation represented by this Span.
        /// </summary>
        /// <param name="timestamp">Unix timestamp value with ms.  Should be reported in UTC</param>
        /// <returns></returns>
        public SpanBuilder WithTimestamp(long timestamp)
        {
            if(timestamp == default)
            {
                return this;
            }

            _span.Timestamp = timestamp;
            return this;
        }

        /// <summary>
        /// Identifies the start time of the operation represented by this Span.
        /// </summary>
        /// <param name="timestamp">UTC time</param>
        /// <returns></returns>
        public SpanBuilder WithTimestamp(DateTimeOffset timestamp)
        {
            if (timestamp == null)
            {
                return this;
            }
            
            return WithTimestamp(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp));
        }
        
        /// <summary>
        /// Used to indicate that an error has occurred during the operation represented
        /// by this Span.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public SpanBuilder HasError(bool b)
        {
            if (b)
            {
                return WithAttribute(attribName_Error, true);
            }

            if (_span.Attributes?.ContainsKey(attribName_Error) == true)
            {
                _span.Attributes.Remove(attribName_Error);
            }

            return this;
        }

        /// <summary>
        /// Used to record the duration of the operation represented by this Span and downstream work
        /// 
        /// </summary>
        /// <param name="durationMs">duration in milliseconds</param>
        /// <returns></returns>
        public SpanBuilder WithDurationMs(double durationMs)
        {
            WithAttribute(attribName_DurationMs, durationMs);
            return this;
        }

        public SpanBuilder WithDurationMs(DateTimeOffset startTimestamp, DateTimeOffset endTimestamp)
        {
            if(startTimestamp == null || endTimestamp == null)
            {
                return this;
            }

            return WithDurationMs(DateTimeExtensions.ToUnixTimeMilliseconds(endTimestamp)
                - DateTimeExtensions.ToUnixTimeMilliseconds(startTimestamp));
        }

        public SpanBuilder WithExecutionTimeInfo(DateTimeOffset startTimestamp, DateTimeOffset endTimestamp)
        {
            if(startTimestamp == null)
            {
                return this;
            }

            var startTimestampUnix = DateTimeExtensions.ToUnixTimeMilliseconds(startTimestamp);
            WithTimestamp(startTimestampUnix);
            
            if(endTimestamp == null)
            {
                return this;
            }

            WithDurationMs(DateTimeExtensions.ToUnixTimeMilliseconds(endTimestamp) - startTimestampUnix);

            return this;
        }

        public SpanBuilder WithName(string name)
        {
            WithAttribute(attribName_Name, name);
            return this;
        }

        public SpanBuilder WithParentId(string parentId)
        {
            WithAttribute(attribName_ParentID, parentId);
            return this;
        }

        public SpanBuilder WithServiceName(string serviceName)
        {
            WithAttribute(attribName_ServiceName, serviceName);
            return this;
        }

        public SpanBuilder WithAttributes<T>(IEnumerable<KeyValuePair<string,T>> attributes)
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
    
        public SpanBuilder WithAttribute<T>(string attribName, T attribVal)
        {
            if (string.IsNullOrWhiteSpace(attribName))
            {
                throw new InvalidOperationException($"{nameof(attribName)} cannot be empty.");
            }
           
            _attributes[attribName] = attribVal;
            return this;
        }

    }
}
