using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;
using System;

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

            var mockBatchDataSender = new Mock<IBatchDataSender>();
            mockBatchDataSender.Setup(x => x.SendBatchAsync(It.IsAny<string>())).Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));

            var spanBatchSender = new SpanBatchSender(mockBatchDataSender.Object);

            var response = spanBatchSender.SendDataAsync(spanBatch).Result;

            Assert.AreEqual(false, response.DidSend);
        }

        [Test]
        public void SendANonEmptySpanBatch()
        {
            var traceId = "123";

            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId(traceId)
                .WithSpan(new Mock<Span>().Object)
                .Build();

            var mockBatchDataSender = new Mock<IBatchDataSender>();
            mockBatchDataSender.Setup(x => x.SendBatchAsync(It.IsAny<string>())).Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));

            var spanBatchSender = new SpanBatchSender(mockBatchDataSender.Object);

            var response = spanBatchSender.SendDataAsync(spanBatch).Result;

            Assert.AreEqual(true, response.DidSend);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode, string.Empty);
        }

        [Test]
        public void SendANonEmptySpanBatch_ResponseHasCorrectRetryAfterValue_RetryAfterWithDateSpecific()
        {
            var traceId = "123";

            var now = DateTimeOffset.Now;
            var retryDuration = TimeSpan.FromSeconds(10);
            var retryOnSpecificTime = now + retryDuration;
            var errorMargin = TimeSpan.FromMilliseconds(500);

            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId(traceId)
                .WithSpan(new Mock<Span>().Object)
                .Build();

            var mockBatchDataSender = new Mock<IBatchDataSender>();
            mockBatchDataSender.Setup(x => x.SendBatchAsync(It.IsAny<string>())).Returns(() =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(retryOnSpecificTime);
                return Task.FromResult(response);
            });

            var spanBatchSender = new SpanBatchSender(mockBatchDataSender.Object);

            var response = spanBatchSender.SendDataAsync(spanBatch).Result;

            Assert.AreEqual(true, response.DidSend);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode, string.Empty);

            Assert.LessOrEqual(response.RetryAfter, retryDuration, string.Empty);
            Assert.GreaterOrEqual(response.RetryAfter, retryDuration - errorMargin, string.Empty);
        }

        [Test]
        public void SendANonEmptySpanBatch_ResponseHasCorrectRetryAfterValue_RetryAfterWithDuration()
        {
            var traceId = "123";
            var retryDuration = TimeSpan.FromSeconds(10);

            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId(traceId)
                .WithSpan(new Mock<Span>().Object)
                .Build();

            var mockBatchDataSender = new Mock<IBatchDataSender>();
            mockBatchDataSender.Setup(x => x.SendBatchAsync(It.IsAny<string>())).Returns(() =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(retryDuration);
                return Task.FromResult(response);
            });

            var spanBatchSender = new SpanBatchSender(mockBatchDataSender.Object);

            var response = spanBatchSender.SendDataAsync(spanBatch).Result;

            Assert.AreEqual(true, response.DidSend);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode, string.Empty);
            Assert.AreEqual(retryDuration, response.RetryAfter, string.Empty);

        }
    }
}
