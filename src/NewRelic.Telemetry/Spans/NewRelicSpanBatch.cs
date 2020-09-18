﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using Utf8Json;
using Utf8Json.Resolvers;

namespace NewRelic.Telemetry.Tracing
{
    public struct NewRelicSpanBatch : ITelemetryDataType<NewRelicSpanBatch>
    {
        [DataMember(Name = "common")]
        public NewRelicSpanBatchCommonProperties? CommonProperties { get; private set; }

        public IEnumerable<NewRelicSpan> Spans { get; }

        public NewRelicSpanBatch(IEnumerable<NewRelicSpan> spans, NewRelicSpanBatchCommonProperties? commonProperties)
        {
            this.CommonProperties = commonProperties;
            this.Spans = spans;
        }

        public string ToJson()
        {
            return JsonSerializer.ToJsonString(new[] { this }, StandardResolver.ExcludeNullCamelCase);
        }

        public void SetInstrumentationProvider(string instrumentationProvider)
        {
            if(string.IsNullOrWhiteSpace(instrumentationProvider))
            {
                return;
            }

            if(CommonProperties == null)
            {
                CommonProperties = new NewRelicSpanBatchCommonProperties();
            }

            CommonProperties?.SetInstrumentationProvider(instrumentationProvider);
        }

    }
}
