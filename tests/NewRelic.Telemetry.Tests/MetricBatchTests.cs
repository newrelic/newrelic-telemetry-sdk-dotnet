// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using NewRelic.Telemetry.Metrics;
using Xunit;

namespace NewRelic.Telemetry.Tests
{
    public class MetricBatchTests
    {
        [Fact]
        public void MetricBatchWithCommonTimestampAndNoMetrics()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var metricBatch = new NewRelicMetricBatch(
                commonProperties: new NewRelicMetricBatchCommonProperties(
                    timestamp: timestamp,
                    intervalMs: null,
                    attributes: null),
                metrics: new NewRelicMetric[0]);

            Assert.Equal(timestamp, metricBatch.CommonProperties.Timestamp);
            Assert.Empty(metricBatch.Metrics);
        }

        [Fact]
        public void MetricBatchWithCommonPropertiesAndMetrics()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var interval = 125L;
            var commonAttrs = new Dictionary<string, object>() { { "attr1Key", "attr1Value" } };
            var countValue = 88d;
            var gaugeValue = 213d;

            var metricBatch = new NewRelicMetricBatch(
                commonProperties: new NewRelicMetricBatchCommonProperties(
                    timestamp: timestamp,
                    intervalMs: interval,
                    attributes: new Dictionary<string, object>()
                    {
                        { "attr1Key", "attr1Value" },
                    }),
                metrics: new[]
                {
                    NewRelicMetric.CreateCountMetric(
                        name: "CountMetric",
                        timestamp: null,
                        attributes: null,
                        value: countValue,
                        intervalMs: interval),
                    NewRelicMetric.CreateGaugeMetric(
                        name: "GaugeMetric",
                        timestamp: null,
                        attributes: null,
                        value: gaugeValue),
                });

            var actualMetrics = metricBatch.Metrics.ToArray();

            Assert.Equal(timestamp, metricBatch.CommonProperties.Timestamp);
            Assert.Equal(interval, metricBatch.CommonProperties.IntervalMs);
            Assert.Equal(2, actualMetrics.Length);
            Assert.Equal("count", actualMetrics[0].Type);
            Assert.Equal("gauge", actualMetrics[1].Type);
        }

        [Fact]
        public void MetricBatchAllowsCommonAndSpecificSameNamedFields()
        {
            var currentUtcTime = DateTimeOffset.UtcNow;
            var commonTimestamp = currentUtcTime.ToUnixTimeMilliseconds();
            var commonInterval = 125L;

            var metricTimestamp = currentUtcTime.AddMinutes(1).ToUnixTimeMilliseconds();
            var metricInterval = 312L;

            var countValue = 88d;

            var metricBatch = new NewRelicMetricBatch(
                commonProperties: new NewRelicMetricBatchCommonProperties(
                    timestamp: commonTimestamp,
                    intervalMs: commonInterval,
                    attributes: new Dictionary<string, object>
                    {
                        { "Attr1Key", "comAttr1Value" },
                    }),
                metrics: new[]
                {
                    NewRelicMetric.CreateCountMetric(
                        name: "CountMetric",
                        timestamp: metricTimestamp,
                        attributes: new Dictionary<string, object>()
                        {
                            { "Attr1Key", "metAttr1Value" },
                        },
                        value: countValue,
                        intervalMs: metricInterval),
                });

            var actualMetrics = metricBatch.Metrics.ToArray();

            Assert.Equal(commonTimestamp, metricBatch.CommonProperties.Timestamp);
            Assert.Equal(commonInterval, metricBatch.CommonProperties.IntervalMs);
            Assert.Equal("comAttr1Value", metricBatch.CommonProperties.Attributes["Attr1Key"]);

            Assert.Equal(metricTimestamp, actualMetrics[0].Timestamp);
            Assert.Equal(metricInterval, actualMetrics[0].IntervalMs);
            Assert.Equal("metAttr1Value", actualMetrics[0].Attributes?["Attr1Key"]);
        }

        [Fact]
        public void MetricBatchWithNoCommonElement()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var timestampL = timestamp;
            var interval = 125L;
            var gaugeValue = 213d;

            var metricBatch = new NewRelicMetricBatch(
                metrics: new[]
                {
                    NewRelicMetric.CreateSummaryMetric(
                        name: "SummaryMetric",
                        timestamp: timestampL,
                        attributes: null,
                        interval: interval,
                        summaryValue: new NewRelicMetricSummaryValue(
                             count: 10d, 
                             sum: 64, 
                             min: 3,
                             max: 15)),
                    NewRelicMetric.CreateGaugeMetric(
                        name: "GaugeMetric",
                        timestamp: timestampL,
                        attributes: null,
                        value: gaugeValue),
                });

            var actualMetrics = metricBatch.Metrics.ToArray();

            Assert.Equal(2, actualMetrics.Length);
            Assert.Empty(metricBatch.CommonProperties.Attributes);
            Assert.Null(metricBatch.CommonProperties.IntervalMs);
            Assert.Null(metricBatch.CommonProperties.Timestamp);
        }
    }
}
