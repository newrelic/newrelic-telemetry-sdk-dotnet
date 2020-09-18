// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NewRelic.Telemetry.Metrics
{
    public struct NewRelicMetricBatchCommonProperties
    {
        public long? Timestamp { get; }

        [DataMember(Name = "interval.ms")]
        public long? IntervalMs { get; }

        public Dictionary<string, object>? Attributes { get; private set; }

        public NewRelicMetricBatchCommonProperties(long? timestamp, long? intervalMs, Dictionary<string, object>? attributes)
        {
            Timestamp = timestamp;
            IntervalMs = intervalMs;
            Attributes = attributes;
        }

        public void SetInstrumentationProvider(string instrumentationProvider)
        {
            if (string.IsNullOrWhiteSpace(instrumentationProvider))
            {
                return;
            }

            if (Attributes == null)
            {
                Attributes = new Dictionary<string, object>();
            }

            Attributes[NewRelicConsts.AttribNameInstrumentationProvider] = instrumentationProvider;
        }
    }
}
