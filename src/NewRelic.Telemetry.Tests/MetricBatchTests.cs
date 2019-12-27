using NUnit.Framework;
using System.Collections.Generic;
using NewRelic.Telemetry.Metrics;
using System;
using NewRelic.Telemetry.Extensions;

namespace NewRelic.Telemetry.Tests
{
    public class MetricBatchTests
    {
        [Test]
        public void MetricBatchWithCommonTimestampAndNoMetrics()
        {
            var timestamp = DateTime.UtcNow;
            var metricBatch = MetricBatchBuilder.Create()
                .WithTimestamp(timestamp)
                .Build();
            
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metricBatch.CommonProperties.Timestamp, $"MetricBatch CommonProperties [Timestamp] - expected: {timestamp}, actual: {metricBatch.CommonProperties.Timestamp}");
            Assert.IsNull(metricBatch.Metrics, $"MetricBatch Metrics - expected: null, actual: not null");
        }

        [Test]
        public void MetricBatchWithCommonPropertiesAndMetrics()
        {
            var timestamp = DateTime.UtcNow;
            var interval = 125L;
            var commonAttrs = new Dictionary<string, object>() { { "attr1Key", "attr1Value" } };
            var countValue = 88d;
            var gaugeValue = 213d;

            var metricBatch = MetricBatchBuilder.Create()
                .WithTimestamp(timestamp)
                .WithIntervalMs(interval)
                .WithAttributes(commonAttrs)
                .WithMetric(MetricBuilder.CreateCountMetric("CountMetric")
                    .WithValue(countValue)
                    .Build())
                .WithMetric(MetricBuilder.CreateGaugeMetric("GaugeMetric")
                    .WithValue(gaugeValue)
                    .Build())
                .Build();

            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metricBatch.CommonProperties.Timestamp, $"MetricBatch CommonProperties [Timestamp] - expected: {timestamp}, actual: {metricBatch.CommonProperties.Timestamp}");
            Assert.AreEqual(interval, metricBatch.CommonProperties.IntervalMs, $"MetricBatch CommonProperties [IntervalMs] - expected: {interval}, actual: {metricBatch.CommonProperties.IntervalMs}");

            Assert.AreEqual(2, metricBatch.Metrics.Count, $"MetricBatch Metrics count - expected: 2, actual: {metricBatch.Metrics.Count}");
            Assert.AreEqual("count", metricBatch.Metrics[0].Type, $"MetricBatch Metrics[0].Type - expected: count, actual: {metricBatch.Metrics[0].Type}");
            Assert.AreEqual("gauge", metricBatch.Metrics[1].Type, $"MetricBatch Metrics[1].Type - expected: gauge, actual: {metricBatch.Metrics[1].Type}");
        }

        [Test]
        public void MetricBatchAllowsCommonAndSpecificSameNamedFields()
        {
            var commonTimestamp = DateTime.UtcNow;
            var commonInterval = 125L;

            var metricTimestamp = DateTime.UtcNow + TimeSpan.FromSeconds(60);
            var metricInterval = 312L;

            var countValue = 88d;

            var metricBatch = MetricBatchBuilder.Create()
                .WithTimestamp(commonTimestamp)
                .WithIntervalMs(commonInterval)
                .WithAttribute("Attr1Key", "comAttr1Value")
                .WithMetric(MetricBuilder.CreateCountMetric("CountMetric")
                    .WithTimestamp(metricTimestamp)
                    .WithIntervalMs(metricInterval)
                    .WithAttribute("Attr1Key", "metAttr1Value")
                    .WithValue(countValue)
                    .Build())
                .Build();

            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(commonTimestamp), metricBatch.CommonProperties.Timestamp, $"MetricBatch CommonProperties [Timestamp] - expected: {commonTimestamp}, actual: {metricBatch.CommonProperties.Timestamp}");
            Assert.AreEqual(commonInterval, metricBatch.CommonProperties.IntervalMs, $"MetricBatch CommonProperties [IntervalMs] - expected: {commonInterval}, actual: {metricBatch.CommonProperties.IntervalMs}");
            Assert.AreEqual("comAttr1Value", metricBatch.CommonProperties.Attributes["Attr1Key"], $"MetricBatch CommonProperties Attributes value - expected: comAttr1Value, actual: {metricBatch.CommonProperties.Attributes["Attr1Key"]}");

            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(metricTimestamp), metricBatch.Metrics[0].Timestamp, $"MetricBatch Metric [Timestamp] - expected: {DateTimeExtensions.ToUnixTimeMilliseconds(metricTimestamp)}, actual: {metricBatch.Metrics[0].Timestamp}");
            Assert.AreEqual(metricInterval, metricBatch.Metrics[0].IntervalMs, $"MetricBatch Metric [IntervalMs] - expected: {metricInterval}, actual: {metricBatch.Metrics[0].IntervalMs}");
            Assert.AreEqual("metAttr1Value", metricBatch.Metrics[0].Attributes["Attr1Key"], $"MetricBatch Metric Attributes value - expected: metAttr1Value, actual: {metricBatch.Metrics[0].Attributes["Attr1Key"]}");
        }

        [Test]
        public void MetricBatchWithNoCommonElement()
        {
            var timestamp = DateTime.UtcNow;
            var interval = 125L;
            var gaugeValue = 213d;
            var summaryValue = new MetricSummaryValue() { Count = 10d, Sum = 64, Min = 3, Max = 15 };

            var metricBatch = MetricBatchBuilder.Create()
                .WithMetric(MetricBuilder.CreateSummaryMetric("SummaryMetric")
                    .WithTimestamp(timestamp)
                    .WithIntervalMs(interval)
                    .WithValue(summaryValue)
                    .Build())
                .WithMetric(MetricBuilder.CreateGaugeMetric("GaugeMetric")
                    .WithTimestamp(timestamp)
                    .WithIntervalMs(interval)
                    .WithValue(gaugeValue)
                    .Build())
                .Build();

            Assert.AreEqual(2, metricBatch.Metrics.Count, $"MetricBatch Metrics count - expected: 2, actual: {metricBatch.Metrics.Count}");
            Assert.IsNull(metricBatch.CommonProperties, $"MetricBatch CommonProperties - expected: null, actual: not null");
        }

    }
}
