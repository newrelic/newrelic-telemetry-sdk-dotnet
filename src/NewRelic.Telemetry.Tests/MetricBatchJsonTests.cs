using NUnit.Framework;
using NewRelic.Telemetry.Metrics;
using System.Linq;
using System;
using NewRelic.Telemetry.Extensions;
using System.Collections.Generic;

namespace NewRelic.Telemetry.Tests
{ 
    class MetricBatchJsonTests
    {
        private static DateTime timestamp = DateTime.UtcNow;
        private long timestampL = DateTimeExtensions.ToUnixTimeMilliseconds(timestamp);
        private long interval = 250L;
        private long countValue = 67;
        private MetricSummaryValue summaryValue = MetricSummaryValue.Create(10, 64, 3, 15);
        private Dictionary<string, object> CustomAttributes = new Dictionary<string, object>() { { "attr1Key", "attr1Value" } };

        [Test]
        public void ToJson_EmptyMetricBatch() 
        {
            // Arrange
            var metricBatch = MetricBatchBuilder.Create().WithTimestamp(timestamp)
                .Build();
            
            // Act
            var jsonString = metricBatch.ToJson();

            //Assert
            var resultMetricBatch = TestHelpers.DeserializeArrayFirstOrDefault(jsonString);
            var resultCommonProps = TestHelpers.DeserializeObject(resultMetricBatch["common"]);

            TestHelpers.AssertForAttribValue(resultCommonProps, "timestamp", timestampL);
       }

        [Test]
        public void ToJson_NonEmptyMetricBatch()
        {

            // Arrange
            var metricBatch = MetricBatchBuilder.Create()
                .WithIntervalMs(interval)
                .WithTimestamp(timestamp)
                .WithMetric(MetricBuilder.CreateCountMetric("metric1")
                    .WithValue(countValue)
                    .WithAttributes(CustomAttributes)
                    .Build())
                .WithMetric(MetricBuilder.CreateSummaryMetric("metric2")
                    .WithValue(summaryValue)
                    .Build())

                .Build();

            // Act
            var jsonString = metricBatch.ToJson();

            // Assert
            var resultMetricBatches = TestHelpers.DeserializeArray(jsonString);

            TestHelpers.AssertForCollectionLength(resultMetricBatches, 1);

            // CountMetric
            var resultMetricBatch = resultMetricBatches.First();
            var resultCommonProps = TestHelpers.DeserializeObject(resultMetricBatch["common"]);

            TestHelpers.AssertForAttribValue(resultCommonProps, "timestamp", timestampL);
            TestHelpers.AssertForAttribValue(resultCommonProps, "interval.ms", interval);

            var resultMetrics = TestHelpers.DeserializeArray(resultMetricBatch["metrics"]);

            TestHelpers.AssertForCollectionLength(resultMetrics, 2);

            var countMetric = resultMetrics.FirstOrDefault();

            TestHelpers.AssertForAttribCount(countMetric, 4);

            TestHelpers.AssertForAttribValue(countMetric, "name", "metric1");
            TestHelpers.AssertForAttribValue(countMetric, "type", "count");
            TestHelpers.AssertForAttribValue(countMetric, "value", countValue);

            var countMetricAttribs = TestHelpers.DeserializeObject(countMetric["attributes"]);
            TestHelpers.AssertForAttribCount(countMetricAttribs, 1);
            TestHelpers.AssertForAttribValue(countMetricAttribs, "attr1Key", "attr1Value");

            // SummaryMetric
            var summaryMetric = resultMetrics[1];

            TestHelpers.AssertForAttribCount(summaryMetric, 3);

            TestHelpers.AssertForAttribValue(summaryMetric, "name", "metric2");
            TestHelpers.AssertForAttribValue(summaryMetric, "type", "summary");
            TestHelpers.AssertForAttribValue(summaryMetric, "value", summaryValue);
        }
    }
}
