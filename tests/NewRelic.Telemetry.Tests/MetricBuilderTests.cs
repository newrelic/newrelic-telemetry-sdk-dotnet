// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using NewRelic.Telemetry.Metrics;
using Xunit;

namespace NewRelic.Telemetry.Tests
{
    public class MetricBuilderTests
    {
        [Fact]
        public void BuildCountMetric()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var interval = 33L;
            var value = 22;

            var metric = NewRelicMetric.CreateCountMetric(
                name: "metricName",
                timestamp: timestamp,
                attributes: new Dictionary<string, object>
                    {
                        { "attrKey", "attrValue" },
                        { "adsfasdf", 12 },
                    },
                value: value,
                intervalMs: interval);

            Assert.Equal("metricName", metric.Name);
            Assert.Equal("count", metric.Type);
            Assert.Equal(value, metric.Value);
            Assert.Equal(timestamp, metric.Timestamp);
            Assert.Equal(interval, metric.IntervalMs);
            Assert.Equal("attrValue", metric.Attributes?["attrKey"]);
        }

        [Fact]
        public void BuildGaugeMetric()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var value = 87;

            var metric = NewRelicMetric.CreateGaugeMetric(
                name: "metricName",
                timestamp: timestamp,
                attributes: new Dictionary<string, object>
                    {
                        { "attrKey", "attrValue" },
                        { "adsfasdf", 12 },
                    },
                value: value);

            Assert.Equal("metricName", metric.Name);
            Assert.Equal("gauge", metric.Type);
            Assert.Equal(value, metric.Value);
            Assert.Equal(timestamp, metric.Timestamp);
            Assert.Equal(12, metric.Attributes?["adsfasdf"]);
            Assert.Equal("attrValue", metric.Attributes?["attrKey"]);
        }

        [Fact]
        public void BuildSummaryMetricWithClass()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var interval = 33L;
            var value = new NewRelicMetricSummaryValue(
                    count: 10d,
                    sum: 64,
                    min: 3,
                    max: 15);

            var metric = NewRelicMetric.CreateSummaryMetric(
                name: "metricName",
                timestamp: timestamp,
                attributes: new Dictionary<string, object>
                    {
                        { "attrKey", "attrValue" },
                        { "adsfasdf", 12 },
                    },
                interval: interval,
                summaryValue: value);

            Assert.Equal("metricName", metric.Name);
            Assert.Equal("summary", metric.Type);
            Assert.Equal(value, metric.SummaryValue);
            Assert.Equal(timestamp, metric.Timestamp);
            Assert.Equal(interval, metric.IntervalMs);
            Assert.Equal(12, metric.Attributes?["adsfasdf"]);
            Assert.Equal("attrValue", metric.Attributes?["attrKey"]);
        }

        [Fact]
        public void BuildSummaryMetricWithValues()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var interval = 33L;
            var value = new NewRelicMetricSummaryValue(10d, 64, 3, 15);

            var metric = NewRelicMetric.CreateSummaryMetric(
                name: "metricName",
                timestamp: timestamp,
                attributes: new Dictionary<string, object>
                    {
                        { "attrKey", "attrValue" },
                        { "adsfasdf", 12 },
                    },
                interval: interval,
                count: value.Count,
                min: value.Min,
                max: value.Max,
                sum: value.Sum);

            Assert.Equal("metricName", metric.Name);
            Assert.Equal("summary", metric.Type);
            Assert.Equal(value, metric.SummaryValue);
            Assert.Equal(timestamp, metric.Timestamp);
            Assert.Equal(interval, metric.IntervalMs);
            Assert.Equal(12, metric.Attributes?["adsfasdf"]);
            Assert.Equal("attrValue", metric.Attributes?["attrKey"]);
        }

        [Fact]
        public void BuildSummaryMetricWithNullMinMax()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var interval = 33L;
            var value = new NewRelicMetricSummaryValue(
                    count: 10d,
                    sum: 64,
                    min: null,
                    max: null);

            var metric = NewRelicMetric.CreateSummaryMetric(
                name: "metricName",
                timestamp: timestamp,
                attributes: new Dictionary<string, object>
                    {
                        { "attrKey", "attrValue" },
                        { "adsfasdf", 12 },
                    },
                interval: interval,
                summaryValue: value);

            Assert.Equal("metricName", metric.Name);
            Assert.Equal("summary", metric.Type);
            Assert.Equal(value, metric.SummaryValue);
            Assert.Equal(timestamp, metric.Timestamp);
            Assert.Equal(interval, metric.IntervalMs);
            Assert.Equal(12, metric.Attributes?["adsfasdf"]);
            Assert.Equal("attrValue", metric.Attributes?["attrKey"]);
        }
    }
}
