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

            var metricBuilder = MetricBuilder.Create("metricname", "count")
                .WithValue(value)
                .WithTimestamp(timestamp)
                .WithIntervalMs(interval)
                .WithAttribute("adsfasdf",12)
                .WithAttributes(attributes);

            var metric = metricBuilder.Build();

            Assert.AreEqual("metricname", metric.Name);
            Assert.AreEqual("count", metric.Type);
            Assert.AreEqual(value, ((CountMetric)metric).Value);
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
            Assert.AreEqual(interval, metric.IntervalMs);
            Assert.AreEqual("attrValue", metric.Attributes["attrKey"]);
        }

        //[Test]
        //public void BuildGaugeMetric()
        //{
        //    var timestamp = DateTime.UtcNow;
        //    var interval = 33L;
        //    var value = 87;

        //    var attributes = new Dictionary<string, object>
        //    { { "attrKey", "attrValue" } };

        //    var metricBuilder = MetricBuilder.Create("metricname", "gauge")
        //        .WithValue(value)
        //        .WithTimestamp(timestamp)
        //        .WithIntervalMs(interval)
        //        .WithAttribute("adsfasdf", 12)
        //        .WithAttributes(attributes);

        //    var metric = metricBuilder.Build();

        //    Assert.AreEqual("metricname", metric.Name);
        //    Assert.AreEqual("gauge", metric.MetricType);
        //    Assert.AreEqual(value, ((GaugeMetric)metric).Value);
        //    Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
        //    Assert.AreEqual(interval, metric.IntervalMs);
        //    Assert.AreEqual("attrValue", metric.Attributes["attrKey"]);
        //}

        //[Test]
        //public void BuildSummaryMetric()
        //{
        //    var timestamp = DateTime.UtcNow;
        //    var interval = 33L;
        //    var value = new MetricSummaryValue() { Count = 10d, Sum = 64, Min = 3, Max = 15 };

        //    var attributes = new Dictionary<string, object>
        //    { { "attrKey", "attrValue" } };

        //    var metricBuilder = MetricBuilder.Create("metricname", "summary")
        //        .WithValue(value)
        //        .WithTimestamp(timestamp)
        //        .WithIntervalMs(interval)
        //        .WithAttribute("adsfasdf", 12)
        //        .WithAttributes(attributes);

        //    var metric = metricBuilder.Build();

        //    Assert.AreEqual("metricname", metric.Name);
        //    Assert.AreEqual("summary", metric.MetricType);
        //    Assert.AreEqual(value, ((SummaryMetric)metric).Value);
        //    Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
        //    Assert.AreEqual(interval, metric.IntervalMs);
        //    Assert.AreEqual("attrValue", metric.Attributes["attrKey"]);
        //}

        [Test]
        public void ThrowExceptionIfNullName()
        {
            Assert.Throws<NullReferenceException>(new TestDelegate(() => MetricBuilder.Create(null, null)));
        }
    }
}