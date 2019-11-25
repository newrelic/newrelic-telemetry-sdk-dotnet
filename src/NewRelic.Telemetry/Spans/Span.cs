using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NewRelic.Telemetry.Spans
{
    public class Span
    {
        /// <summary>
        /// Uniquely identifies this Span.  This identifier can be used to link other spans
        /// to this one in a parent/child relationship.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Identifies the trace/operation to which the span belongs.
        /// </summary>
        [DataMember(Name = "trace.id")]
        public string TraceId { get; internal set; }

        /// <summary>
        /// Records the start-time of the operation.  Represented as Unix timestamp with
        /// milliseconds.
        /// </summary>
        public long? Timestamp { get; internal set; }

        /// <summary>
        /// Custom attributes that provide additional context/additional information about the
        /// unit of work being represnted by this Span.
        /// </summary>
        public Dictionary<string, object> Attributes { get; internal set; }

        internal Span()
        {
        }
    }
}
