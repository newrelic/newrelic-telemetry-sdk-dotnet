﻿// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.Serialization;
using Utf8Json;
using Utf8Json.Resolvers;

namespace NewRelic.Telemetry.Metrics
{
    public struct NewRelicMetricBatch : ITelemetryDataType<NewRelicMetricBatch>
    {
        [DataMember(Name = "common")]
        public NewRelicMetricBatchCommonProperties? CommonProperties { get; private set; }

        public IEnumerable<NewRelicMetric> Metrics { get; }

        public NewRelicMetricBatch(IEnumerable<NewRelicMetric> metrics, NewRelicMetricBatchCommonProperties? commonProperties)
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
            if (string.IsNullOrWhiteSpace(instrumentationProvider))
            {
                return;
            }

            if (CommonProperties == null)
            {
                CommonProperties = new NewRelicMetricBatchCommonProperties(null, null, null);
            }

            CommonProperties?.SetInstrumentationProvider(instrumentationProvider);
        }
    }
}
