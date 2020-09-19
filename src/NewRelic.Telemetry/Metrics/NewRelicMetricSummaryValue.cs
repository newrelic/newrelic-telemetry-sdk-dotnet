// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

namespace NewRelic.Telemetry.Metrics
{
#if INTERNALIZE_TELEMETRY_SDK
    internal
#else
    public
#endif
    readonly struct NewRelicMetricSummaryValue
    {
        public double Count { get; }

        public double Sum { get; }

        public double? Min { get; }

        public double? Max { get; }

        public NewRelicMetricSummaryValue(double count, double sum, double? min, double? max)
        {
            Count = count;
            Sum = sum;
            Min = min;
            Max = max;
        }
    }
}
