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
            var interval = 33L;
            var value = 22;

            var attributes = new Dictionary<string, object> 
            { { "attrKey", "attrValue" } };

            var metric = CountMetric.Create("metricname",value)
                .WithTimestamp(timestamp)
                .WithIntervalMs(interval)
                .WithAttribute("adsfasdf",12)
                .WithAttributes(attributes);

            Assert.AreEqual("metricname", metric.Name);
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
            var interval = 33L;
            var value = 87;

            var attributes = new Dictionary<string, object>
            { { "attrKey", "attrValue" } };

            var metric = GaugeMetric.Create("metricname",value)
                .WithTimestamp(timestamp)
                .WithIntervalMs(interval)
                .WithAttribute("adsfasdf", 12)
                .WithAttributes(attributes);

            Assert.AreEqual("metricname", metric.Name);
            Assert.AreEqual("gauge", metric.Type);
            Assert.AreEqual(value, ((GaugeMetric)metric).Value);
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
            Assert.AreEqual(interval, metric.IntervalMs);
            Assert.AreEqual(12, metric.Attributes?["adsfasdf"]);
            Assert.AreEqual("attrValue", metric.Attributes["attrKey"]);
        }

        [Test]
        public void BuildSummaryMetric()
        {
            var timestamp = DateTime.UtcNow;
            var interval = 33L;
            var value = new MetricSummaryValue(10d, 64, 3, 15);

            var attributes = new Dictionary<string, object>
            { { "attrKey", "attrValue" } };

            var metric = SummaryMetric.Create("metricname", value)
                .WithTimestamp(timestamp)
                .WithIntervalMs(interval)
                .WithAttribute("adsfasdf", 12)
                .WithAttributes(attributes);

            Assert.AreEqual("metricname", metric.Name);
            Assert.AreEqual("summary", metric.Type);
            Assert.AreEqual(value, metric.Value);
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
            Assert.AreEqual(interval, metric.IntervalMs);
            Assert.AreEqual(12, metric.Attributes?["adsfasdf"]);
            Assert.AreEqual("attrValue", metric.Attributes?["attrKey"]);
        }

        [Test]
        public void BuildSummaryMetricWithNullMinMax()
        {
            var timestamp = DateTime.UtcNow;
            var interval = 33L;
            var value = new MetricSummaryValue(10d, 64);

            var attributes = new Dictionary<string, object>
            { { "attrKey", "attrValue" } };

            var metric = SummaryMetric.Create("metricname", value)
                .WithTimestamp(timestamp)
                .WithIntervalMs(interval)
                .WithAttribute("adsfasdf", 12)
                .WithAttributes(attributes);

            Assert.AreEqual("metricname", metric.Name);
            Assert.AreEqual("summary", metric.Type);
            Assert.AreEqual(value, metric.Value);
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
            Assert.AreEqual(interval, metric.IntervalMs);
            Assert.AreEqual(12, metric.Attributes?["adsfasdf"]);
            Assert.AreEqual("attrValue", metric.Attributes?["attrKey"]);
        }

        //[Test]
        //public void ThrowExceptionIfNullName()
        //{
        //    Assert.Throws<ArgumentNullException>(new TestDelegate(() => CountMetric.Create(null)));
        //}
    }
}