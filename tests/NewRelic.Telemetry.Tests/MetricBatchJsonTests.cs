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
        private NewRelicMetricSummaryValue summaryValue = new NewRelicMetricSummaryValue(10, 64, 3, 15);
        private Dictionary<string, object> CustomAttributes = new Dictionary<string, object>() { { "attr1Key", "attr1Value" } };

        [Test]
        public void ToJson_EmptyMetricBatch() 
        {
            // Arrange
            var metricBatch = new NewRelicMetricBatch(new List<NewRelicMetric>(), new NewRelicMetricBatchCommonProperties(timestampL, null, null));

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
            var metricBatch = new NewRelicMetricBatch(
                commonProperties: new NewRelicMetricBatchCommonProperties(
                    timestamp: timestampL,
                    intervalMs: interval,
                    attributes: null),
                metrics: new []
                {
                    NewRelicMetric.CreateCountMetric(
                        name: "metric1",
                        timestamp: null,
                        attributes: CustomAttributes,
                        value: countValue,
                        intervalMs: interval),
                    NewRelicMetric.CreateSummaryMetric(
                        name: "metric2",
                        timestamp: null,
                        attributes: null,
                        interval: interval,
                        summaryValue: summaryValue)
                });

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

            TestHelpers.AssertForAttribCount(countMetric, 5);

            TestHelpers.AssertForAttribValue(countMetric, "name", "metric1");
            TestHelpers.AssertForAttribValue(countMetric, "type", "count");
            TestHelpers.AssertForAttribValue(countMetric, "value", countValue);
            TestHelpers.AssertForAttribValue(countMetric, "interval.ms", interval);


            var countMetricAttribs = TestHelpers.DeserializeObject(countMetric["attributes"]);
            TestHelpers.AssertForAttribCount(countMetricAttribs, 1);
            TestHelpers.AssertForAttribValue(countMetricAttribs, "attr1Key", "attr1Value");

            // SummaryMetric
            var summaryMetric = resultMetrics[1];

            TestHelpers.AssertForAttribCount(summaryMetric, 4);

            TestHelpers.AssertForAttribValue(summaryMetric, "name", "metric2");
            TestHelpers.AssertForAttribValue(summaryMetric, "type", "summary");
            TestHelpers.AssertForAttribValue(summaryMetric, "value", summaryValue);
            TestHelpers.AssertForAttribValue(countMetric, "interval.ms", interval);
        }
    }
}
