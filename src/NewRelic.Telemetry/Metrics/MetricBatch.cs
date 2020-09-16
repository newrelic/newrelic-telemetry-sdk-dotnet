// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Utf8Json.Resolvers;

namespace NewRelic.Telemetry.Metrics
{
    public class MetricBatch : ITelemetryDataType
    {
        /// <summary>
        /// Properties that are common to all metrics being submitted as part of this MetricBatch.
        /// </summary>
        [DataMember(Name = "common")]
        public MetricBatchCommonProperties CommonProperties { get; internal set; }

        /// <summary>
        /// The collection of metrics being reported in this batch.
        /// </summary>
        public List<Metric> Metrics { get; internal set; }

        internal MetricBatch()
        {
        }

        internal MetricBatch(MetricBatchCommonProperties commonProperties, IEnumerable<Metric> metrics)
        {
            CommonProperties = commonProperties;
            Metrics = metrics.ToList();
        }

        public string ToJson()
        {
            return Utf8Json.JsonSerializer.ToJsonString(new[] { this },
                StandardResolver.ExcludeNullCamelCase);
        }
    }
}
