// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.Serialization;
using Utf8Json;
using Utf8Json.Resolvers;

namespace NewRelic.Telemetry.Tracing
{
    public readonly struct NewRelicSpanBatch : ITelemetryDataType<NewRelicSpanBatch>
    {
        [DataMember(Name = "common")]
        public NewRelicSpanBatchCommonProperties CommonProperties { get; }

        public IEnumerable<NewRelicSpan> Spans { get; }

        public NewRelicSpanBatch(IEnumerable<NewRelicSpan> spans)
        {
            CommonProperties = new NewRelicSpanBatchCommonProperties(null);
            Spans = spans;
        }

        public NewRelicSpanBatch(IEnumerable<NewRelicSpan> spans, NewRelicSpanBatchCommonProperties commonProperties)
        {
            CommonProperties = commonProperties;
            Spans = spans;
        }

        public string ToJson()
        {
            return JsonSerializer.ToJsonString(new[] { this }, StandardResolver.ExcludeNullCamelCase);
        }

        public void SetInstrumentationProvider(string instrumentationProvider)
        {
            CommonProperties.SetInstrumentationProvider(instrumentationProvider);
        }
    }
}
