using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;

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
    }
}
