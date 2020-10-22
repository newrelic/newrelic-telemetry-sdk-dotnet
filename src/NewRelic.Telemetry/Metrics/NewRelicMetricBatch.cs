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

namespace NewRelic.Telemetry.Metrics
{
#if INTERNALIZE_TELEMETRY_SDK
    internal
#else
    public
#endif
    readonly struct NewRelicMetricBatch : ITelemetryDataType<NewRelicMetricBatch>
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
        public NewRelicMetricBatchCommonProperties CommonProperties { get; }

        public IEnumerable<NewRelicMetric> Metrics { get; }

        public NewRelicMetricBatch(IEnumerable<NewRelicMetric> metrics)
        {
            Metrics = metrics;
            CommonProperties = new NewRelicMetricBatchCommonProperties(null, null, null);
        }

        public NewRelicMetricBatch(IEnumerable<NewRelicMetric> metrics, NewRelicMetricBatchCommonProperties commonProperties)
        {
            Metrics = metrics;
            CommonProperties = commonProperties;
        }

        public string ToJson()
        {
#if NETFRAMEWORK
            return JsonConvert.SerializeObject(new[] { this }, JsonSerializerSettings);
#else
            return JsonSerializer.Serialize(new[] { this }, JsonSerializerOptions);
#endif
        }
    }
}
