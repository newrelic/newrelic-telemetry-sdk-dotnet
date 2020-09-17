// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Utf8Json;
using Utf8Json.Resolvers;

namespace NewRelic.Telemetry.Spans
{
    public class SpanBatch : ITelemetryDataType
    {
        /// <summary>
        /// Properties that are common to all spans being submitted as part of this SpanBatch.
        /// </summary>
        [DataMember(Name = "common")]
        public SpanBatchCommonProperties CommonProperties { get; internal set; }

        /// <summary>
        /// The spans that are being reported as part of this batch.
        /// </summary>
        public List<Span> Spans { get; internal set; }

        internal SpanBatch()
        {
        }

        internal SpanBatch(SpanBatchCommonProperties commonProperties, IEnumerable<Span> spans)
        {
            CommonProperties = commonProperties;
            Spans = spans.ToList();
        }

        public string ToJson()
        {
            return JsonSerializer.ToJsonString(new[] { this }, StandardResolver.ExcludeNullCamelCase);
        }
    }
}
