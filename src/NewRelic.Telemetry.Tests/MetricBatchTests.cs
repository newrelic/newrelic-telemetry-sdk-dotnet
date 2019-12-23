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
        public void MetricBatchWithCommonTimestamp()
        {
            var timestamp = DateTime.UtcNow;
            var metricBatch = MetricBatchBuilder.Create()
                .WithTimestamp(timestamp)
                .Build();
            
            Assert.AreEqual(DateTimeExtensions.ToUnixTimeMilliseconds(timestamp), metricBatch.CommonProperties.Timestamp);
        }
    }
}
