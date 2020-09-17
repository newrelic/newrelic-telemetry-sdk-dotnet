// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using NewRelic.Telemetry.Extensions;
using NewRelic.Telemetry.Metrics;
using NUnit.Framework;

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

            var metricBuilder = MetricBuilder.CreateCountMetric("metricname")
                .WithValue(value)
                .WithTimestamp(timestamp)
                .WithIntervalMs(interval)
                .WithAttribute("adsfasdf", 12)
                .WithAttributes(attributes);

            var metric = metricBuilder.Build();

            Assert.AreEqual("metricname", metric.Name);
            Assert.AreEqual("count", metric.Type);
            Assert.AreEqual(value, metric.MetricValue);
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
            Assert.AreEqual(interval, metric.IntervalMs);
            Assert.AreEqual("attrValue", metric.Attributes["attrKey"]);
        }

        [Test]
        public void BuildGaugeMetric()
        {
            var timestamp = DateTime.UtcNow;
            var interval = 33L;
            var value = 87;

            var attributes = new Dictionary<string, object>
            { { "attrKey", "attrValue" } };

            var metricBuilder = MetricBuilder.CreateGaugeMetric("metricname")
                .WithValue(value)
                .WithTimestamp(timestamp)
                .WithIntervalMs(interval)
                .WithAttribute("adsfasdf", 12)
                .WithAttributes(attributes);

            var metric = metricBuilder.Build();

            Assert.AreEqual("metricname", metric.Name);
            Assert.AreEqual("gauge", metric.Type);
            Assert.AreEqual(value, ((GaugeMetric)metric).Value);
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
            Assert.AreEqual(interval, metric.IntervalMs);
            Assert.AreEqual(12, metric.Attributes["adsfasdf"]);
            Assert.AreEqual("attrValue", metric.Attributes["attrKey"]);
        }

        [Test]
        public void BuildSummaryMetric()
        {
            var timestamp = DateTime.UtcNow;
            var interval = 33L;
            var value = MetricSummaryValue.Create(10d, 64, 3, 15);

            var attributes = new Dictionary<string, object>
            { { "attrKey", "attrValue" } };

            var metricBuilder = MetricBuilder.CreateSummaryMetric("metricname")
                .WithValue(value)
                .WithTimestamp(timestamp)
                .WithIntervalMs(interval)
                .WithAttribute("adsfasdf", 12)
                .WithAttributes(attributes);

            var metric = metricBuilder.Build();

            Assert.AreEqual("metricname", metric.Name);
            Assert.AreEqual("summary", metric.Type);
            Assert.AreEqual(value, metric.Value);
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
            Assert.AreEqual(interval, metric.IntervalMs);
            Assert.AreEqual(12, metric.Attributes["adsfasdf"]);
            Assert.AreEqual("attrValue", metric.Attributes["attrKey"]);
        }

        [Test]
        public void BuildSummaryMetricWithNullMinMax()
        {
            var timestamp = DateTime.UtcNow;
            var interval = 33L;
            var value = MetricSummaryValue.Create(10d, 64);

            var attributes = new Dictionary<string, object>
            { { "attrKey", "attrValue" } };

            var metricBuilder = MetricBuilder.CreateSummaryMetric("metricname")
                .WithValue(value)
                .WithTimestamp(timestamp)
                .WithIntervalMs(interval)
                .WithAttribute("adsfasdf", 12)
                .WithAttributes(attributes);

            var metric = metricBuilder.Build();

            Assert.AreEqual("metricname", metric.Name);
            Assert.AreEqual("summary", metric.Type);
            Assert.AreEqual(value, metric.Value);
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metric.Timestamp);
            Assert.AreEqual(interval, metric.IntervalMs);
            Assert.AreEqual(12, metric.Attributes["adsfasdf"]);
            Assert.AreEqual("attrValue", metric.Attributes["attrKey"]);
        }

        [Test]
        public void ThrowExceptionIfNullName()
        {
            Assert.Throws<ArgumentNullException>(new TestDelegate(() => MetricBuilder.CreateCountMetric(null)));
        }
    }
}