// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

namespace NewRelic.Telemetry.Metrics
{
    /// <summary>
    /// Gauge Metrics represent a value that can increase or decrease with time.
    /// </summary>
    /// <example>The temperature, CPU usage, and memory.</example>
    public class GaugeMetric : Metric<double>
    {
        public override string Type => "gauge";
    }
}
