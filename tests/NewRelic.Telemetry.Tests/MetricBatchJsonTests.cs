// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using NewRelic.Telemetry.Extensions;
using NewRelic.Telemetry.Metrics;
using NUnit.Framework;

namespace NewRelic.Telemetry.Tests
{ 
    internal class MetricBatchJsonTests
    {
        private static DateTime _timestamp = DateTime.UtcNow;
        private long _timestampL = DateTimeExtensions.ToUnixTimeMilliseconds(_timestamp);
        private long _interval = 250L;
        private long _countValue = 67;
        private MetricSummaryValue _summaryValue = MetricSummaryValue.Create(10, 64, 3, 15);
        private Dictionary<string, object> _customAttributes = new Dictionary<string, object>() { { "attr1Key", "attr1Value" } };

        [Test]
        public void ToJson_EmptyMetricBatch() 
        {
            // Arrange
            var metricBatch = MetricBatchBuilder.Create().WithTimestamp(_timestamp)
                .Build();
            
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
            var metricBatch = MetricBatchBuilder.Create()
                .WithIntervalMs(_interval)
                .WithTimestamp(_timestamp)
                .WithMetric(MetricBuilder.CreateCountMetric("metric1")
                    .WithIntervalMs(_interval)
                    .WithValue(_countValue)
                    .WithAttributes(_customAttributes)
                    .Build())
                .WithMetric(MetricBuilder.CreateSummaryMetric("metric2")
                    .WithIntervalMs(_interval)
                    .WithValue(_summaryValue)
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
