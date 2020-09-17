// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http;
using System.Threading.Tasks;
using NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;
using NUnit.Framework;

namespace NewRelic.Telemetry.Tests
{
    public class SpanDataSenderTests
    {
        [Test]
        public void SendAnEmptySpanBatch()
        {
            var traceId = "123";
            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId(traceId)
                .Build();

            var dataSender = new SpanDataSender(new TelemetryConfiguration().WithApiKey("123456"));

            dataSender.WithHttpHandlerImpl((serializedJson) =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                return Task.FromResult(response);
            });

            var response = dataSender.SendDataAsync(spanBatch).Result;

            Assert.AreEqual(NewRelicResponseStatus.DidNotSend_NoData, response.ResponseStatus);
        }

        [Test]
        public void SendANonEmptySpanBatch()
        {
            var traceId = "123";

            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId(traceId)
                .WithSpan(SpanBuilder.Create("TestSpan").Build())
                .Build();

            var dataSender = new SpanDataSender(new TelemetryConfiguration().WithApiKey("123456"));

            dataSender.WithHttpHandlerImpl((serializedJson) =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                return Task.FromResult(response);
            });

            var response = dataSender.SendDataAsync(spanBatch).Result;

            Assert.AreEqual(NewRelicResponseStatus.Success, response.ResponseStatus);
        }

        [Test]
        public void InstrumentationProviderSuppliedWhenConfigured()
        {
            var traceId = "123";
            var instrumentationProvider = "TestInstrumentationProvider";

            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId(traceId)
                .WithSpan(SpanBuilder.Create("TestSpan").Build())
                .Build();

            var dataSender = new SpanDataSender(new TelemetryConfiguration()
                .WithApiKey("123456")
                .WithInstrumentationProviderName(instrumentationProvider));

            SpanBatch capturedSpanbatch = null;
            dataSender.WithCaptureSendDataAsyncDelegate((spanBatch, attempt) =>
            {
                capturedSpanbatch = spanBatch;
            });

            var response = dataSender.SendDataAsync(spanBatch).Result;

            Assert.IsNotNull(spanBatch);
            Assert.AreEqual(1, spanBatch.Spans.Count);
            Assert.IsNotNull(spanBatch.Spans[0].Attributes);
            Assert.IsTrue(spanBatch.Spans[0].Attributes.ContainsKey("instrumentation.provider"));
            Assert.AreEqual(instrumentationProvider, spanBatch.Spans[0].Attributes["instrumentation.provider"]);
        }
    }
}
