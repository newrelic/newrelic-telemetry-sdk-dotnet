// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
#if NETFRAMEWORK
using Newtonsoft.Json;
#else
using System.Text.Json.Serialization;
#endif

namespace NewRelic.Telemetry.Tracing
{
#if INTERNALIZE_TELEMETRY_SDK
    internal
#else
    public
#endif
    readonly struct NewRelicSpanBatchCommonProperties
    {
#if NETFRAMEWORK
        [JsonProperty("trace.id")]
#else
        [JsonPropertyName("trace.id")]
#endif
        public string? TraceId { get; }

        private readonly Dictionary<string, object> _attributes;

        public IReadOnlyDictionary<string, object> Attributes => _attributes;

        public NewRelicSpanBatchCommonProperties(string? traceId)
        {
            TraceId = traceId;
            _attributes = new Dictionary<string, object>();
        }

        public NewRelicSpanBatchCommonProperties(string? traceId, Dictionary<string, object> attributes)
        {
            TraceId = traceId;
            _attributes = attributes;
        }

        public void SetInstrumentationProvider(string instrumentationProvider)
        {
            _attributes[NewRelicConsts.AttribNameInstrumentationProvider] = instrumentationProvider;
        }
    }
}
