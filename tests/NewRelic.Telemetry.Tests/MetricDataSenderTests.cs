// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http;
using System.Threading.Tasks;
using NewRelic.Telemetry.Metrics;
using NewRelic.Telemetry.Transport;
using Xunit;

namespace NewRelic.Telemetry.Tests
{
    public class MetricDataSenderTests
    {
        [Fact]
        public void SendAnEmptyMetricBatch()
        {
            var spanBatch = new NewRelicMetricBatch(
                metrics: new NewRelicMetric[0]);

            var dataSender = new MetricDataSender(new TelemetryConfiguration().WithApiKey("123456"));

            dataSender.WithHttpHandlerImpl((serializedJson) =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                return Task.FromResult(response);
            });

            var response = dataSender.SendDataAsync(spanBatch).Result;

            Assert.Equal(NewRelicResponseStatus.DidNotSend_NoData, response.ResponseStatus);
        }

        [Fact]
        public void SendANonEmptyMetricBatch()
        {
            var metricBatch = new NewRelicMetricBatch(
                metrics: new[]
                    {
                        NewRelicMetric.CreateGaugeMetric(
                            name: "TestMetric",
                            timestamp: null,
                            attributes: null,
                            value: 0),
                    });

            var dataSender = new MetricDataSender(new TelemetryConfiguration().WithApiKey("123456"));

            dataSender.WithHttpHandlerImpl((serializedJson) =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                return Task.FromResult(response);
            });

            var response = dataSender.SendDataAsync(metricBatch).Result;

            Assert.Equal(NewRelicResponseStatus.Success, response.ResponseStatus);
        }
    }
}
