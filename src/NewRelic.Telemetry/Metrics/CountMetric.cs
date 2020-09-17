// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

namespace NewRelic.Telemetry.Metrics
{
    /// <summary>
    /// Count Metrics measure the number of occurrences of an event within 
    /// a reporting window. The count should be reset to 0 for each window (i.e. every time the 
    /// metric is reported). 
    /// When using a CountMetric, IntervalMs must be reported.
    /// When using a CountMetric, the Value must be a positive value.
    /// </summary>
    /// <example>Cache hits per reporting interval.</example>
    /// <example>Number of threads created per reporting interval.</example>
    public class CountMetric : Metric<double>
    {
        public override string Type => "count";
    }
}
