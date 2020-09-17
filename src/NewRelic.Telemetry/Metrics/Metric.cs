// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NewRelic.Telemetry.Metrics
{
    /// <summary>
    /// A metric represents the value of a measurement.
    /// </summary>
    public abstract class Metric
    {
        /// <summary>
        /// Gets the name of the metric.  Must be less that 255 characters.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the type of value represented by this metric.
        /// Valid values: count, gauge, or summary.
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// Gets the Metric's start time.  Metrics often represent values collected
        /// over a period of time.  This value should be reported as the start time
        /// of the measurement window.  For point in time observations, this value
        /// should be the time at which the measurement was taken.
        /// </summary>
        public long? Timestamp { get; internal set; }

        /// <summary>
        /// Gets the duration of the time window that the value represents.
        /// This is required for count and summary type metrics.
        /// </summary>
        [DataMember(Name = "interval.ms")]
        public long? IntervalMs { get; internal set; }

        /// <summary>
        /// Gets the value being reported for this metric.  The layout of this value will
        /// vary depending on the implementation: count, summary, or gauge.
        /// </summary>
        [DataMember(Name = "value")]
        public abstract object MetricValue { get; }

        /// <summary>
        /// Gets a map of Key Value pairs identifying the dimensions of this metric.
        /// Keys are case sensitive and must be less than 255 characters.  Values may be
        /// strings, numbers, or booleans.
        /// </summary>
        public Dictionary<string, object> Attributes { get; internal set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Improves readability and clutter.")]
    public abstract class Metric<T> : Metric
    {
        [IgnoreDataMember]
        public T Value { get; internal set; }

        public override object MetricValue => Value;
    }
}
