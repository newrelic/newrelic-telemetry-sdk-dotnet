// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
#if NETFRAMEWORK
using Newtonsoft.Json;
#else
using System.Text.Json.Serialization;
#endif

namespace NewRelic.Telemetry.Metrics
{
#if INTERNALIZE_TELEMETRY_SDK
    internal
#else
    public
#endif
    readonly struct NewRelicMetric
    {
        public string Type { get; }

        public Dictionary<string, object>? Attributes { get; }

        public long? Timestamp { get; }

        public string Name { get; }

#if NETFRAMEWORK
        [JsonProperty("interval.ms")]
#else
        [JsonPropertyName("interval.ms")]
#endif
        public long? IntervalMs { get; }

#if NETFRAMEWORK
        [JsonProperty("value")]
#else
        [JsonPropertyName("value")]
#endif
        public object? ValueForSerialization => SummaryValue ?? Value as object;

        [JsonIgnore]
        public double? Value { get; }

        [JsonIgnore]
        public NewRelicMetricSummaryValue? SummaryValue { get; }

        private NewRelicMetric(string type, long? timestamp, string name, Dictionary<string, object>? attributes, long? intervalMs, double? value, NewRelicMetricSummaryValue? summaryValue)
        {
            Type = type;
            Timestamp = timestamp;
            Name = name;
            Attributes = attributes;
            IntervalMs = intervalMs;
            Value = value;
            SummaryValue = summaryValue;
        }
        
        public static NewRelicMetric CreateCountMetric(string name, long? timestamp, Dictionary<string, object>? attributes, double value, long intervalMs)
        {
            return new NewRelicMetric("count", timestamp, name, attributes, intervalMs, value, null);
        }

        public static NewRelicMetric CreateGaugeMetric(string name, long? timestamp, Dictionary<string, object>? attributes, double value)
        {
            return new NewRelicMetric("gauge", timestamp, name, attributes, null, value, null);
        }

        public static NewRelicMetric CreateSummaryMetric(string name, long? timestamp, Dictionary<string, object>? attributes, long interval, double count, double sum, double? min, double? max)
        {
            var summary = new NewRelicMetricSummaryValue(count, sum, min, max);
            
            return CreateSummaryMetric(name, timestamp, attributes, interval, summary);
        }

        public static NewRelicMetric CreateSummaryMetric(string name, long? timestamp, Dictionary<string, object>? attributes, long interval, NewRelicMetricSummaryValue summaryValue)
        {
            return new NewRelicMetric("summary", timestamp, name, attributes, interval, null, summaryValue);
        }
    }
}
