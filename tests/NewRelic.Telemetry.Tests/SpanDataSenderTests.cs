// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NewRelic.Telemetry.Tracing;
using NewRelic.Telemetry.Transport;
using Xunit;

namespace NewRelic.Telemetry.Tests
{
    public class SpanDataSenderTests
    {
        [Fact]
        public void SendAnEmptySpanBatch()
        {
            var traceId = "123";
            var spanBatch = new NewRelicSpanBatch(
                spans: new NewRelicSpan[0],
                commonProperties: new NewRelicSpanBatchCommonProperties(traceId));

            var dataSender = new TraceDataSender(new TelemetryConfiguration() { ApiKey = "123456" }, null);

            dataSender.WithHttpHandlerImpl((serializedJson) =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                return Task.FromResult(response);
            });

            var response = dataSender.SendDataAsync(spanBatch).Result;

            Assert.Equal(NewRelicResponseStatus.DidNotSend_NoData, response.ResponseStatus);
        }

        [Fact]
        public void SendANonEmptySpanBatch()
        {
            var traceId = "123";

            var span = new NewRelicSpan(
                traceId: null,
                spanId: "Span1",
                timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                parentSpanId: null,
                attributes: new Dictionary<string, object>()
                {
                    { NewRelicConsts.Tracing.AttribNameName, "TestSpan" },
                });

            var spanBatch = new NewRelicSpanBatch(
                spans: new[] { span }, 
                commonProperties: new NewRelicSpanBatchCommonProperties(traceId));

            var dataSender = new TraceDataSender(new TelemetryConfiguration() { ApiKey = "123456" }, null);

            dataSender.WithHttpHandlerImpl((serializedJson) =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                return Task.FromResult(response);
            });

            var response = dataSender.SendDataAsync(spanBatch).Result;

            Assert.Equal(NewRelicResponseStatus.Success, response.ResponseStatus);
        }
    }
}
