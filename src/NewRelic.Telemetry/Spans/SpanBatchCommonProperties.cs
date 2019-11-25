using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NewRelic.Telemetry.Spans
{
    /// <summary>
    /// Properties that are common all of the Spans that are part of the SpanBatch.
    /// </summary>
    public class SpanBatchCommonProperties
    {
        /// <summary>
        /// The unique identifier that links all of the Spans that are part of this batch.  This field is 
        /// optional and should be used when all spans being reported in this Span Batch represent the same operation.
        /// </summary>
        [DataMember(Name = "trace.id")]
        public string TraceId { get; internal set; }

        /// <summary>
        /// Provides additional attribution/contextual that is common to all of the
        /// Spans being reported in the SpanBatch.
        /// </summary>
        public Dictionary<string,object> Attributes { get; internal set; }

        internal SpanBatchCommonProperties()
        {
        }
    }


}
