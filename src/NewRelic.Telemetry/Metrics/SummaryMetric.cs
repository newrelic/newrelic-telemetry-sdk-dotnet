// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

namespace NewRelic.Telemetry.Metrics
{
    /// <summary>
    /// Summary Metrics represent pre-aggregated data, or information on aggregated 
    /// discrete events.
    /// When using a SummaryMetric, IntervalMs must be reported.
    /// </summary>
    public class SummaryMetric : Metric<MetricSummaryValue>
    {
        public override string Type => "summary";
    }
}
