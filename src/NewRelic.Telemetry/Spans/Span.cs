using NewRelic.Telemetry.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NewRelic.Telemetry.Spans
{
    /// <summary>
    /// A Span measures a unit of work that is part of an operation/trace.
    /// </summary>
    public class Span
    {
        internal const string attribName_ServiceName = "service.name";
        internal const string attribName_DurationMs = "duration.ms";
        internal const string attribName_Name = "name";
        internal const string attribName_ParentID = "parent.id";
        internal const string attribName_Error = "error";
        internal const string attribName_ErrorMsg = "error.message";
        internal const string attribName_InstrumentationProvider = "instrumentation.provider";



        /// <summary>
        /// Uniquely identifies this Span.  This identifier may be used to link other spans
        /// to this one in a parent/child relationship.
        /// </summary>
        public string Id { get; internal set; }

        [IgnoreDataMember]
        public string? ParentId
        {
            get
            {
                if (Attributes == null)
                {
                    return null;
                }

                if (Attributes.TryGetValue(Span.attribName_ParentID, out var parentId))
                {
                    return parentId?.ToString();
                }

                return null;
            }
        }

        /// <summary>
        /// Identifies this span as a component of a trace/operation.
        /// </summary>
        [DataMember(Name = "trace.id")]
        public string? TraceId { get; internal set; }

        /// <summary>
        /// Records the start-time of the unit of work being measured by this Span.  
        /// It is represented as Unix timestamp with milliseconds.
        /// </summary>
        public long? Timestamp { get; internal set; }

        /// <summary>
        /// Custom attributes that provide additional context about the
        /// unit of work being represnted by this Span.
        /// </summary>
        public Dictionary<string, object>? Attributes { get; internal set; }

        private Dictionary<string,object> EnsureAttributes()
        {
            return Attributes ??= new Dictionary<string, object>();
        }

        public static Span Create(string id)
        {
            return new Span(id);
        }

        internal Span(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Identifies this span as a unit of work that is part of a specific trace/operation.
        /// </summary>
        /// <param name="traceId"></param>
        public Span WithTraceId(string traceId)
        {
            TraceId = traceId;
            return this;
        }

        /// <summary>
        /// Identifies the start time of the unit of work represented by this Span.
        /// </summary>
        /// <param name="timestamp">Unix timestamp value ms precision.  Should be reported in UTC.</param>
        public Span WithTimestamp(long timestamp)
        {
            if (timestamp == default)
            {
                return this;
            }

            Timestamp = timestamp;
            return this;
        }

        /// <summary>
        /// Identifies the start time of the operation represented by this Span.
        /// </summary>
        /// <param name="timestamp">UTC time.</param>
        public Span WithTimestamp(DateTimeOffset timestamp)
        {
            if (timestamp == null)
            {
                return this;
            }

            return WithTimestamp(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp));
        }

        /// <summary>
        /// Used to indicate that an error has occurred during the unit of work represented
        /// by this Span.  Setting hasError to false will also clear the "error.message" value.
        /// </summary>
        /// <param name="hasError"></param>
        public Span HasError(bool hasError)
        {
            if (hasError)
            {
                return WithAttribute(attribName_Error, true);
            }
            else if (Attributes != null)
            {
                if (Attributes.ContainsKey(attribName_Error) == true)
                {
                    Attributes.Remove(attribName_Error);
                }

                if (Attributes.ContainsKey(attribName_ErrorMsg) == true)
                {
                    Attributes.Remove(attribName_ErrorMsg);
                }
            }
            return this;
        }

        /// <summary>
        /// Used to indicate that an error has occurred during the unit of work represented
        /// by this Span.  Additionally records a message describing the error condition.
        /// </summary>
        /// <param name="hasError"></param>
        public Span HasError(string? errorMessage = null)
        {
            HasError(true);

            if (errorMessage == null || string.IsNullOrWhiteSpace(errorMessage))
            {
                return this;
            }

            return WithAttribute(attribName_ErrorMsg, errorMessage);
        }

        /// <summary>
        /// Used to record the duration of the unit of work represented by this Span and downstream work
        /// requested by this unit of work.
        /// </summary>
        /// <param name="durationMs">Duration in milliseconds.</param>
        public Span WithDurationMs(double durationMs)
        {
            WithAttribute(attribName_DurationMs, durationMs);
            return this;
        }

        /// <summary>
        /// Used to record both the start time and duration of the unit of work 
        /// represented by this Span.
        /// </summary>
        /// <param name="startTimestamp"></param>
        /// <param name="duration"></param>
        public Span WithExecutionTimeInfo(DateTimeOffset startTimestamp, TimeSpan duration)
        {
            if (startTimestamp == default)
            {
                return this;
            }

            var startTimestampUnix = DateTimeExtensions.ToUnixTimeMilliseconds(startTimestamp);
            WithTimestamp(startTimestampUnix);

            if (duration == default)
            {
                return this;
            }

            WithDurationMs(duration.TotalMilliseconds);

            return this;
        }

        /// <summary>
        /// Sets the name of the span to something meaningful for later analysis.  This should not be a <see cref="Span.Id">unique 
        /// identifier for the span.</see>  It should describe the unit of work such that executions of
        /// similiar operations can be compared/analyzed.
        /// </summary>
        /// <example>This may be the name of a method being called.</example>
        /// <example>In a web application, this may be the url to the controller action.</example>
        /// <param name="name"></param>
        public Span WithName(string name)
        {
            WithAttribute(attribName_Name, name);
            return this;
        }

        /// <summary>
        /// Identifies this Span as a sub-operation of another span.  Used to measure inner-work as part of 
        /// a larger operation.
        /// </summary>
        /// <param name="parentId">The Id of the Span which launched this Span.  <see cref="Span.Id>">See SpanId</see></param>
        public Span WithParentId(string parentId)
        {
            WithAttribute(attribName_ParentID, parentId);
            return this;
        }

        /// <summary>
        /// Identifies the name of a service performing the units of work measured by these Spans.
        /// </summary>
        /// <param name="serviceName"></param>
        public Span WithServiceName(string serviceName)
        {
            WithAttribute(attribName_ServiceName, serviceName);
            return this;
        }

        /// <summary>
        /// Allows custom attribution of the Span to provide additional contextual
        /// information for later analysis.  
        /// </summary>
        /// <param name="attributes">Key/Value pairs representing the custom attributes.  In the event of a duplicate key, the last value will be used.</param>
        public Span WithAttributes<T>(IEnumerable<KeyValuePair<string, T>> attributes)
        {
            if (attributes == null)
            {
                return this;
            }

            foreach (var attrib in attributes)
            {
                if(attrib.Value == null)
                {
                    continue;
                }

                WithAttribute(attrib.Key, attrib.Value);
            }

            return this;
        }

        /// <summary>
        /// Allows custom attribution of the Span.
        /// </summary>
        /// <param name="attribName">Name of the attribute.  If an attribute with this name already exists, the previous value will be overwritten.</param>
        /// <param name="attribVal">Value of the attribute.</param>
        public Span WithAttribute(string attribName, object attribVal)
        {
            if (string.IsNullOrWhiteSpace(attribName))
            {
                throw new InvalidOperationException($"{nameof(attribName)} cannot be empty.");
            }

            EnsureAttributes()[attribName] = attribVal;
            return this;
        }

    }
}
