// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
#if NETFRAMEWORK
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#else
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace NewRelic.Telemetry.Tracing
{
#if INTERNALIZE_TELEMETRY_SDK
    internal
#else
    public
#endif
    readonly struct NewRelicSpanBatch : ITelemetryDataType<NewRelicSpanBatch>
    {
#if NETFRAMEWORK
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };
#else
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
        {
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
#endif

#if NETFRAMEWORK
        [JsonProperty("common")]
#else
        [JsonPropertyName("common")]
#endif
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
#if NETFRAMEWORK
            return JsonConvert.SerializeObject(new[] { this }, JsonSerializerSettings);
#else
            return JsonSerializer.Serialize(new[] { this }, JsonSerializerOptions);
#endif
        }

        public void SetInstrumentationProvider(string instrumentationProvider)
        {
            CommonProperties.SetInstrumentationProvider(instrumentationProvider);
        }
    }
}
