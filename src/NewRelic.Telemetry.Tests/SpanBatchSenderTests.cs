using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;
using System;
using System.Collections.Generic;

namespace NewRelic.Telemetry.Tests
{
    public class SpanBatchSenderTests
    {
        [Test]
        public void SendAnEmptySpanBatch()
        {
            var traceId =   "123";
            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId(traceId)
                .Build();

            var dataSender = new SpanDataSender(new TelemetryConfiguration());

            dataSender.WithHttpHandlerImpl((serializedJson) =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                return Task.FromResult(response);
            });

            var response = dataSender.SendDataAsync(spanBatch).Result;

            Assert.AreEqual(NewRelicResponseStatus.DidNotSend, response.ResponseStatus);
        }

        [Test]
        public void SendANonEmptySpanBatch()
        {
            var traceId = "123";

            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId(traceId)
                .WithSpan(SpanBuilder.Create("TestSpan").Build())
                .Build();

            var dataSender = new SpanDataSender(new TelemetryConfiguration());

            dataSender.WithHttpHandlerImpl((serializedJson) =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                return Task.FromResult(response);
            });

            var response = dataSender.SendDataAsync(spanBatch).Result;

            Assert.AreEqual(NewRelicResponseStatus.SendSuccess, response.ResponseStatus);
        }


        [Test]
        public void SendANonEmptySpanBatch_ResponseHasCorrectRetryAfterValue_RetryAfterWithDateSpecific()
        {
            var traceId = "123";
            var retryDuration = TimeSpan.FromSeconds(10);
            var errorMargin = TimeSpan.FromMilliseconds(50);

            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId(traceId)
                .WithSpan(new Mock<Span>().Object)
                .Build();

            var config = new TelemetryConfiguration().WithAPIKey("12345");

            var dataSender = new SpanDataSender(config);

            dataSender.WithHttpHandlerImpl((serializedJson) =>
            {
                var response = new HttpResponseMessage((System.Net.HttpStatusCode)429);
                var retryOnSpecificTime = DateTimeOffset.Now + retryDuration;
                response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(retryOnSpecificTime);
                return Task.FromResult(response);

            });

            var capturedDelays = new List<int>();
            dataSender.WithDelayFunction((delay) =>
            {
                capturedDelays.Add(delay);
                return Task.Delay(0);
            });

            var response = dataSender.SendDataAsync(spanBatch).Result;

            Assert.AreEqual(NewRelicResponseStatus.SendFailure, response.ResponseStatus);
            Assert.AreEqual(config.MaxRetryAttempts, capturedDelays.Count);

        }

        //[Test]
        //public void SendANonEmptySpanBatch_ResponseHasCorrectRetryAfterValue_RetryAfterWithDuration()
        //{
        //    var traceId = "123";
        //    var retryDuration = TimeSpan.FromSeconds(10);

        //    var spanBatch = SpanBatchBuilder.Create()
        //        .WithTraceId(traceId)
        //        .WithSpan(new Mock<Span>().Object)
        //        .Build();

        //    var mockBatchDataSender = new Mock<IBatchDataSender>();
        //    mockBatchDataSender.Setup(x => x.SendBatchAsync(It.IsAny<string>())).Returns(() =>
        //    {
        //        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        //        response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(retryDuration);
        //        return Task.FromResult(response);
        //    });

        //    var spanBatchSender = new SpanBatchSender(mockBatchDataSender.Object);

        //    var response = spanBatchSender.SendDataAsync(spanBatch).Result;

        //    Assert.AreEqual(true, response.DidSend);
        //    Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode, string.Empty);
        //    Assert.AreEqual(retryDuration, response.RetryAfter, string.Empty);

        //}
    }
}
