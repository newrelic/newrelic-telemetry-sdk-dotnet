using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NewRelic.Telemetry.Metrics
{
    /// <summary>
    /// Properties that are common on all of the Metrics that are part of the MetricBatch.
    /// </summary>
    [DataContract]
    public class MetricBatchCommonProperties
    {
        /// <summary>
        /// The start time of the observation window for this metric batch.
        /// Often used with <see cref="IntervalMs"/> to describe the observation 
        /// time window for the values being reported.
        /// </summary>
        public long? Timestamp { get; internal set; }

        /// <summary>
        /// The duration of the observation window for this metric batch.
        /// Used in conjunction with <see cref="Timestamp"/> to describe the observation
        /// time window for the values being reported.
        /// </summary>
        [DataMember(Name = "interval.ms")]
        public long? IntervalMs { get; internal set; }

        /// <summary>
        /// Provides additional contextual information that is common to all of the
        /// Metrics being reported in this Batch.
        /// </summary>
        public Dictionary<string, object>? Attributes { get; private set; }

        internal Dictionary<string,object> EnsureAttributes()
        {
            return Attributes ?? (Attributes = new Dictionary<string, object>());
        }

        internal MetricBatchCommonProperties()
        {
        }
    }
}
