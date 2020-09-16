// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NewRelic.Telemetry.Spans
{
    /// <summary>
    /// A Span measures a unit of work that is part of an operation/trace.
    /// </summary>
    public class Span
    {
        /// <summary>
        /// Uniquely identifies this Span.  This identifier may be used to link other spans
        /// to this one in a parent/child relationship.
        /// </summary>
        public string Id { get; internal set; }

        [IgnoreDataMember]
        public string ParentId
        {
            get
            {
                if (Attributes == null)
                {
                    return null;
                }

                if (Attributes.TryGetValue(SpanBuilder.attribName_ParentID, out var parentId))
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
        public string TraceId { get; internal set; }

        /// <summary>
        /// Records the start-time of the unit of work being measured by this Span.  
        /// It is represented as Unix timestamp with milliseconds.
        /// </summary>
        public long? Timestamp { get; internal set; }

        /// <summary>
        /// Custom attributes that provide additional context about the
        /// unit of work being represnted by this Span.
        /// </summary>
        public Dictionary<string, object> Attributes { get; internal set; }

        internal Span()
        {
        }
    }
}
