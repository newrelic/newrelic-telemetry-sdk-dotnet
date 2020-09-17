// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

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
        /// Optional:  A unique identifier that links all of the Spans that are part of this batch.  This field 
        /// should be used when all spans being reported are part of the same operation.  Alternatively,
        /// you can set the <see cref="Span.TraceId">TraceId</see> on each span individually./>.
        /// </summary>
        [DataMember(Name = "trace.id")]
        public string TraceId { get; internal set; }

        /// <summary>
        /// Provides additional contextual information that is common to all of the
        /// Spans being reported in this SpanBatch.
        /// </summary>
        public Dictionary<string, object> Attributes { get; internal set; }

        internal SpanBatchCommonProperties()
        {
        }
    }
}
