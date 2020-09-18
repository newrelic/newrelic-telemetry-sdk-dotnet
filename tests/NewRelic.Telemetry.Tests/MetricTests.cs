using NUnit.Framework;
using System;
using System.Collections.Generic;
using NewRelic.Telemetry.Metrics;
using NewRelic.Telemetry.Extensions;

namespace NewRelic.Telemetry.Tests
{
    public class MetricBuilderTests
    {
        [Test]
        public void BuildCountMetric()
        {
            var timestamp = DateTime.UtcNow;
            var timestampL = DateTimeExtensions.ToUnixTimeMilliseconds(timestamp);
            var interval = 33L;
            var value = 22;

            var metric = NewRelicMetric.CreateCountMetric(
                name: "metricName",
                timestamp: timestampL,
                attributes: new Dictionary<string, object>
                    {
                        { "attrKey", "attrValue" },
                        { "adsfasdf", 12 }
                    },
                value: value,
                intervalMs: interval);

            Assert.AreEqual("metricName", metric.Name);
            Assert.AreEqual("count", metric.Type);
            Assert.AreEqual(value, metric.Value);
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
            Assert.AreEqual(interval, metric.IntervalMs);
            Assert.AreEqual("attrValue", metric.Attributes?["attrKey"]);
        }

        [Test]
        public void BuildGaugeMetric()
        {
            var timestamp = DateTime.UtcNow;
            var timestampL = DateTimeExtensions.ToUnixTimeMilliseconds(timestamp);
            var value = 87;

            var metric = NewRelicMetric.CreateGaugeMetric(
                name: "metricName",
                timestamp: timestampL,
                attributes: new Dictionary<string, object>
                    {
                        { "attrKey", "attrValue" },
                        { "adsfasdf", 12 }
                    },
                value: value);

            Assert.AreEqual("metricName", metric.Name);
            Assert.AreEqual("gauge", metric.Type);
            Assert.AreEqual(value, metric.Value);
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
            Assert.AreEqual(12, metric.Attributes?["adsfasdf"]);
            Assert.AreEqual("attrValue", metric.Attributes?["attrKey"]);
        }

        [Test]
        public void BuildSummaryMetricWithClass()
        {
            var timestamp = DateTime.UtcNow;
            var timestampL = DateTimeExtensions.ToUnixTimeMilliseconds(timestamp);
            var interval = 33L;
            var value = new NewRelicMetricSummaryValue(
                    count: 10d,
                    sum: 64,
                    min: 3,
                    max: 15);

            var metric = NewRelicMetric.CreateSummaryMetric(
                name: "metricName",
                timestamp: timestampL,
                attributes: new Dictionary<string, object>
                    {
                        { "attrKey", "attrValue" },
                        { "adsfasdf", 12 }
                    },
                interval: interval,
                summaryValue: value
               );

            Assert.AreEqual("metricName", metric.Name);
            Assert.AreEqual("summary", metric.Type);
            Assert.AreEqual(value, metric.SummaryValue);
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
            Assert.AreEqual(interval, metric.IntervalMs);
            Assert.AreEqual(12, metric.Attributes?["adsfasdf"]);
            Assert.AreEqual("attrValue", metric.Attributes?["attrKey"]);
        }

        [Test]
        public void BuildSummaryMetricWithValues()
        {
            var timestamp = DateTime.UtcNow;
            var timestampL = DateTimeExtensions.ToUnixTimeMilliseconds(timestamp);
            var interval = 33L;
            var value = new NewRelicMetricSummaryValue(10d, 64, 3, 15);


            var metric = NewRelicMetric.CreateSummaryMetric(
                name: "metricName",
                timestamp: timestampL,
                attributes: new Dictionary<string, object>
                    {
                        { "attrKey", "attrValue" },
                        { "adsfasdf", 12 }
                    },
                interval: interval,
                count: value.Count,
                min: value.Min,
                max: value.Max,
                sum: value.Sum
               );

            Assert.AreEqual("metricName", metric.Name);
            Assert.AreEqual("summary", metric.Type);
            Assert.AreEqual(value, metric.SummaryValue);
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
            Assert.AreEqual(interval, metric.IntervalMs);
            Assert.AreEqual(12, metric.Attributes?["adsfasdf"]);
            Assert.AreEqual("attrValue", metric.Attributes?["attrKey"]);
        }

        [Test]
        public void BuildSummaryMetricWithNullMinMax()
        {
            var timestamp = DateTime.UtcNow;
            var timestampL = DateTimeExtensions.ToUnixTimeMilliseconds(timestamp);
            var interval = 33L;
            var value = new NewRelicMetricSummaryValue(
                    count: 10d,
                    sum: 64,
                    min: null,
                    max: null);

            var metric = NewRelicMetric.CreateSummaryMetric(
                name: "metricName",
                timestamp: timestampL,
                attributes: new Dictionary<string, object>
                    {
                        { "attrKey", "attrValue" },
                        { "adsfasdf", 12 }
                    },
                interval: interval,
                summaryValue: value
               );

            Assert.AreEqual("metricName", metric.Name);
            Assert.AreEqual("summary", metric.Type);
            Assert.AreEqual(value, metric.SummaryValue);
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
            Assert.AreEqual(interval, metric.IntervalMs);
            Assert.AreEqual(12, metric.Attributes?["adsfasdf"]);
            Assert.AreEqual("attrValue", metric.Attributes?["attrKey"]);
        }
    }
}