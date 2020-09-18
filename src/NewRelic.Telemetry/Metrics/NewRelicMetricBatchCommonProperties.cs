// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NewRelic.Telemetry.Metrics
{
    public readonly struct NewRelicMetricBatchCommonProperties
    {
        public long? Timestamp { get; }

        [DataMember(Name = "interval.ms")]
        public long? IntervalMs { get; }

        private readonly Dictionary<string, object> _attributes;

        public IReadOnlyDictionary<string, object> Attributes => _attributes;

        public NewRelicMetricBatchCommonProperties(long? timestamp, long? intervalMs, Dictionary<string, object>? attributes)
        {
            Timestamp = timestamp;
            IntervalMs = intervalMs;
            _attributes = attributes ?? new Dictionary<string, object>();
        }

        public void SetInstrumentationProvider(string instrumentationProvider)
        {
            _attributes[NewRelicConsts.AttribNameInstrumentationProvider] = instrumentationProvider;
        }
    }
}
