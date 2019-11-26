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
        /// Creates a new span with a unique SpanId Identifier.
        /// </summary>
        /// <param name="spanId">Required,:  A unique identifier for the span being reported.  This identifier may be used to link child spans.</param>
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
        public Span Build()
        {
            return _span;
        }

        /// <summary>
        /// Identifies this span as a unit of work that is part of a specific trace/operation.
        /// </summary>
        /// <param name="traceId"></param>
        public SpanBuilder WithTraceId(string traceId)
        {
            _span.TraceId = traceId;
            return this;
        }

        /// <summary>
        /// Identifies the start time of the unit of work represented by this Span.
        /// </summary>
        /// <param name="timestamp">Unix timestamp value ms precision.  Should be reported in UTC</param>
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
        public SpanBuilder WithTimestamp(DateTimeOffset timestamp)
        {
            if (timestamp == null)
            {
                return this;
            }
            
            return WithTimestamp(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp));
        }
        
        /// <summary>
        /// Used to indicate that an error has occurred during the unit of work represented
        /// by this Span.
        /// </summary>
        /// <param name="hasError"></param>
        public SpanBuilder HasError(bool hasError)
        {
            if (hasError)
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
        /// Used to record the duration of the unit of work represented by this Span and downstream work
        /// that was has requested by this unit of work.
        /// </summary>
        /// <param name="durationMs">duration in milliseconds</param>
        public SpanBuilder WithDurationMs(double durationMs)
        {
            WithAttribute(attribName_DurationMs, durationMs);
            return this;
        }

        /// <summary>
        /// Used to record both the start and end time as well as the duration of the unit of work 
        /// represented by this Span.
        /// </summary>
        /// <param name="startTimestamp"></param>
        /// <param name="endTimestamp"></param>
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

        /// <summary>
        /// Sets the name of the span to something meaningful for later analysis.  This should not be a <see cref="Span.Id">unique 
        /// identifier for the span.</see>  It should describe the unit of work such that executions of
        /// similiar operations can be compared/analyzed.
        /// </summary>
        /// <example>This may be the name of a method being called</example>
        /// <example>In a web application, this may be the url to the controller action</example>
        /// <param name="name"></param>
        public SpanBuilder WithName(string name)
        {
            WithAttribute(attribName_Name, name);
            return this;
        }

        /// <summary>
        /// Identifies this Span as a sub-operation of another span.  Used to measure inner-work as part of 
        /// a larger operation.
        /// </summary>
        /// <param name="parentId">The Id of the Span to which this Span belongs.  <see cref="Span.Id>">See SpanId</see></param>
        public SpanBuilder WithParentId(string parentId)
        {
            WithAttribute(attribName_ParentID, parentId);
            return this;
        }


        /// <summary>
        /// Identifies the name of a service performing the units of work measured by these Spans.
        /// </summary>
        /// <param name="serviceName"></param>
        public SpanBuilder WithServiceName(string serviceName)
        {
            WithAttribute(attribName_ServiceName, serviceName);
            return this;
        }

        /// <summary>
        /// Allows custom attribution of the Span to provide additional contextual
        /// information for later analysis.  
        /// </summary>
        /// <param name="attributes">Key/Value pairs representing the custom attributes.  In the event of a duplicate key, the last value will be used.</param>
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
    
        /// <summary>
        /// Allows custom attribution of the Span
        /// </summary>
        /// <param name="attribName">the name of the attribute.  If an attribute with this name already exists, the previous value will be overwritten</param>
        /// <param name="attribVal">the value of the attribute</param>
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
