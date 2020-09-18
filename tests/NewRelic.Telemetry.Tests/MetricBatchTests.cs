using NUnit.Framework;
using System.Collections.Generic;
using NewRelic.Telemetry.Metrics;
using System;
using NewRelic.Telemetry.Extensions;
using System.Linq;

namespace NewRelic.Telemetry.Tests
{
    public class MetricBatchTests
    {
        [Test]
        public void MetricBatchWithCommonTimestampAndNoMetrics()
        {
            var timestamp = DateTime.UtcNow;

            var metricBatch = new NewRelicMetricBatch(
                commonProperties: new NewRelicMetricBatchCommonProperties(
                    timestamp: DateTimeExtensions.ToUnixTimeMilliseconds(timestamp),
                    intervalMs: null,
                    attributes: null),
                metrics: new NewRelicMetric[0]);

            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metricBatch.CommonProperties?.Timestamp, $"MetricBatch CommonProperties [Timestamp] - expected: {timestamp}, actual: {metricBatch.CommonProperties?.Timestamp}");
            Assert.IsEmpty(metricBatch.Metrics, $"MetricBatch Metrics - expected: empty, actual: not empty");
        }

        [Test]
        public void MetricBatchWithCommonPropertiesAndMetrics()
        {
            var timestamp = DateTime.UtcNow;
            var interval = 125L;
            var commonAttrs = new Dictionary<string, object>() { { "attr1Key", "attr1Value" } };
            var countValue = 88d;
            var gaugeValue = 213d;

            var metricBatch = new NewRelicMetricBatch(
                commonProperties: new NewRelicMetricBatchCommonProperties(
                    timestamp: timestamp.ToUnixTimeMilliseconds(),
                    intervalMs: interval,
                    attributes: new Dictionary<string, object>()
                    {
                        { "attr1Key", "attr1Value" }
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
                        value: gaugeValue)
                });

            var actualMetrics = metricBatch.Metrics.ToArray();

            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metricBatch.CommonProperties?.Timestamp, $"MetricBatch CommonProperties [Timestamp] - expected: {timestamp}, actual: {metricBatch.CommonProperties?.Timestamp}");
            Assert.AreEqual(interval, metricBatch.CommonProperties?.IntervalMs, $"MetricBatch CommonProperties [IntervalMs] - expected: {interval}, actual: {metricBatch.CommonProperties?.IntervalMs}");
            Assert.AreEqual(2, actualMetrics.Length, $"MetricBatch Metrics count - expected: 2, actual: {actualMetrics.Length}");
            Assert.AreEqual("count", actualMetrics[0].Type, $"MetricBatch Metrics[0].Type - expected: count, actual: {actualMetrics[0].Type}");
            Assert.AreEqual("gauge", actualMetrics[1].Type, $"MetricBatch Metrics[1].Type - expected: gauge, actual: {actualMetrics[1].Type}");
        }

        [Test]
        public void MetricBatchAllowsCommonAndSpecificSameNamedFields()
        {
            var commonTimestamp = DateTime.UtcNow;
            var commonInterval = 125L;

            var metricTimestamp = DateTime.UtcNow + TimeSpan.FromSeconds(60);
            var metricInterval = 312L;

            var countValue = 88d;

            var metricBatch = new NewRelicMetricBatch(
                commonProperties: new NewRelicMetricBatchCommonProperties(
                    timestamp: commonTimestamp.ToUnixTimeMilliseconds(),
                    intervalMs: commonInterval,
                    attributes: new Dictionary<string, object>
                    {
                        { "Attr1Key", "comAttr1Value" }
                    }),
                metrics: new[]
                {
                    NewRelicMetric.CreateCountMetric(
                        name: "CountMetric",
                        timestamp: metricTimestamp.ToUnixTimeMilliseconds(),
                        attributes: new Dictionary<string, object>()
                        {
                            { "Attr1Key", "metAttr1Value" }
                        },
                        value: countValue,
                        intervalMs: metricInterval)
                });

            var actualMetrics = metricBatch.Metrics.ToArray();

            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(commonTimestamp), metricBatch.CommonProperties?.Timestamp, $"MetricBatch CommonProperties [Timestamp] - expected: {commonTimestamp}, actual: {metricBatch.CommonProperties?.Timestamp}");
            Assert.AreEqual(commonInterval, metricBatch.CommonProperties?.IntervalMs, $"MetricBatch CommonProperties [IntervalMs] - expected: {commonInterval}, actual: {metricBatch.CommonProperties?.IntervalMs}");
            Assert.AreEqual("comAttr1Value", metricBatch.CommonProperties?.Attributes?["Attr1Key"], $"MetricBatch CommonProperties Attributes value - expected: comAttr1Value, actual: {metricBatch.CommonProperties?.Attributes?["Attr1Key"]}");

            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(metricTimestamp), actualMetrics[0].Timestamp, $"MetricBatch Metric [Timestamp] - expected: {DateTimeExtensions.ToUnixTimeMilliseconds(metricTimestamp)}, actual: {actualMetrics[0].Timestamp}");
            Assert.AreEqual(metricInterval, actualMetrics[0].IntervalMs, $"MetricBatch Metric [IntervalMs] - expected: {metricInterval}, actual: {actualMetrics[0].IntervalMs}");
            Assert.AreEqual("metAttr1Value", actualMetrics[0].Attributes?["Attr1Key"], $"MetricBatch Metric Attributes value - expected: metAttr1Value, actual: {actualMetrics[0].Attributes?["Attr1Key"]}");
        }

        [Test]
        public void MetricBatchWithNoCommonElement()
        {
            var timestamp = DateTime.UtcNow;
            var timestampL = DateTimeExtensions.ToUnixTimeMilliseconds(timestamp);
            var interval = 125L;
            var gaugeValue = 213d;

            var metricBatch = new NewRelicMetricBatch(
                commonProperties: null,
                metrics: new []
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
                        value: gaugeValue)
                });


            var actualMetrics = metricBatch.Metrics.ToArray();

            Assert.AreEqual(2, actualMetrics.Length, $"MetricBatch Metrics count - expected: 2, actual: {actualMetrics.Length}");
            Assert.IsNull(metricBatch.CommonProperties, $"MetricBatch CommonProperties - expected: null, actual: not null");
        }

    }
}
