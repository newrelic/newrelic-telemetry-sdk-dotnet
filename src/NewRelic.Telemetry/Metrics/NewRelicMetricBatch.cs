// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.Serialization;
using Utf8Json;
using Utf8Json.Resolvers;

namespace NewRelic.Telemetry.Metrics
{
    public readonly struct NewRelicMetricBatch : ITelemetryDataType<NewRelicMetricBatch>
    {
        [DataMember(Name = "common")]
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
            return JsonSerializer.ToJsonString(new[] { this }, StandardResolver.ExcludeNullCamelCase);
        }

        public void SetInstrumentationProvider(string instrumentationProvider)
        {
            CommonProperties.SetInstrumentationProvider(instrumentationProvider);
        }
    }
}
