﻿// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using NewRelic.Telemetry.Metrics;
using NewRelic.Telemetry.Transport;

namespace NewRelic.Telemetry.Tests
{
    public class MetricDataSenderTests
    {
        [Test]
        public void SendAnEmptyMetricBatch()
        {
            var spanBatch = MetricBatchBuilder.Create()
                .Build();

            var dataSender = new MetricDataSender(new TelemetryConfiguration().WithApiKey("123456"));

            dataSender.WithHttpHandlerImpl((serializedJson) =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                return Task.FromResult(response);
            });

            var response = dataSender.SendDataAsync(spanBatch).Result;

            Assert.AreEqual(NewRelicResponseStatus.DidNotSend_NoData, response.ResponseStatus);
        }

        [Test]
        public void SendANonEmptyMetricBatch()
        {

            var metricBatch = MetricBatchBuilder.Create()
                .WithMetric(MetricBuilder.CreateGaugeMetric("TestMetric").Build())
                .Build();

            var dataSender = new MetricDataSender(new TelemetryConfiguration().WithApiKey("123456"));

            dataSender.WithHttpHandlerImpl((serializedJson) =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                return Task.FromResult(response);
            });

            var response = dataSender.SendDataAsync(metricBatch).Result;

            Assert.AreEqual(NewRelicResponseStatus.Success, response.ResponseStatus);
        }
    }
}
