// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using NewRelic.Telemetry.Metrics;
using NUnit.Framework;

namespace NewRelic.Telemetry.Tests
{ 
    public class MetricBatchJsonTests
    {
        private readonly long _timestampL = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        private readonly long _interval = 250L;

        private readonly long _countValue = 67;

        private readonly NewRelicMetricSummaryValue _summaryValue = new NewRelicMetricSummaryValue(
            count: 10,
            sum: 64,
            min: 3,
            max: 15);

        private readonly Dictionary<string, object> _customAttributes = new Dictionary<string, object>() { { "attr1Key", "attr1Value" } };

        [Test]
        public void ToJson_EmptyMetricBatch()
        {
            // Arrange
            var metricBatch = new NewRelicMetricBatch(new List<NewRelicMetric>(), new NewRelicMetricBatchCommonProperties(_timestampL, null, null));

            // Act
            var jsonString = metricBatch.ToJson();

            // Assert
            var resultMetricBatch = TestHelpers.DeserializeArrayFirstOrDefault(jsonString);
            var resultCommonProps = TestHelpers.DeserializeObject(resultMetricBatch["common"]);

            TestHelpers.AssertForAttribValue(resultCommonProps, "timestamp", _timestampL);
       }

        [Test]
        public void ToJson_NonEmptyMetricBatch()
        {
            // Arrange
            var metricBatch = new NewRelicMetricBatch(
                commonProperties: new NewRelicMetricBatchCommonProperties(
                    timestamp: _timestampL,
                    intervalMs: _interval,
                    attributes: null),
                metrics: new []
                {
                    NewRelicMetric.CreateCountMetric(
                        name: "metric1",
                        timestamp: null,
                        attributes: _customAttributes,
                        value: _countValue,
                        intervalMs: _interval),
                    NewRelicMetric.CreateSummaryMetric(
                        name: "metric2",
                        timestamp: null,
                        attributes: null,
                        interval: _interval,
                        summaryValue: _summaryValue)
                });

            // Act
            var jsonString = metricBatch.ToJson();

            // Assert
            var resultMetricBatches = TestHelpers.DeserializeArray(jsonString);

            TestHelpers.AssertForCollectionLength(resultMetricBatches, 1);

            // CountMetric
            var resultMetricBatch = resultMetricBatches.First();
            var resultCommonProps = TestHelpers.DeserializeObject(resultMetricBatch["common"]);

            TestHelpers.AssertForAttribValue(resultCommonProps, "timestamp", _timestampL);
            TestHelpers.AssertForAttribValue(resultCommonProps, "interval.ms", _interval);

            var resultMetrics = TestHelpers.DeserializeArray(resultMetricBatch["metrics"]);

            TestHelpers.AssertForCollectionLength(resultMetrics, 2);

            var countMetric = resultMetrics.FirstOrDefault();

            TestHelpers.AssertForAttribCount(countMetric, 5);

            TestHelpers.AssertForAttribValue(countMetric, "name", "metric1");
            TestHelpers.AssertForAttribValue(countMetric, "type", "count");
            TestHelpers.AssertForAttribValue(countMetric, "value", _countValue);
            TestHelpers.AssertForAttribValue(countMetric, "interval.ms", _interval);


            var countMetricAttribs = TestHelpers.DeserializeObject(countMetric["attributes"]);
            TestHelpers.AssertForAttribCount(countMetricAttribs, 1);
            TestHelpers.AssertForAttribValue(countMetricAttribs, "attr1Key", "attr1Value");

            // SummaryMetric
            var summaryMetric = resultMetrics[1];

            TestHelpers.AssertForAttribCount(summaryMetric, 4);

            TestHelpers.AssertForAttribValue(summaryMetric, "name", "metric2");
            TestHelpers.AssertForAttribValue(summaryMetric, "type", "summary");
            TestHelpers.AssertForAttribValue(summaryMetric, "value", _summaryValue);
            TestHelpers.AssertForAttribValue(countMetric, "interval.ms", _interval);
        }
    }
}
