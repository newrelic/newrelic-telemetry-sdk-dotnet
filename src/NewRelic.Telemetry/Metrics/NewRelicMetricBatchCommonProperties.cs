// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
#if NETFRAMEWORK
using Newtonsoft.Json;
#else
using System.Text.Json.Serialization;
#endif

namespace NewRelic.Telemetry.Metrics
{
#if INTERNALIZE_TELEMETRY_SDK
    internal
#else
    public
#endif
    readonly struct NewRelicMetricBatchCommonProperties
    {
        public long? Timestamp { get; }

#if NETFRAMEWORK
        [JsonProperty("interval.ms")]
#else
        [JsonPropertyName("interval.ms")]
#endif
        public long? IntervalMs { get; }

        private readonly Dictionary<string, object> _attributes;

        public IReadOnlyDictionary<string, object> Attributes => _attributes;

        public NewRelicMetricBatchCommonProperties(long? timestamp, long? intervalMs, Dictionary<string, object>? attributes)
        {
            Timestamp = timestamp;
            IntervalMs = intervalMs;
            _attributes = attributes ?? new Dictionary<string, object>();
        }
    }
}
