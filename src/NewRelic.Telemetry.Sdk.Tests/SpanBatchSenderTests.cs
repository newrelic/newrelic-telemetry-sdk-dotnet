using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;
using Telerik.JustMock;

namespace NewRelic.Telemetry.Sdk.Tests
{
    public class SpanBatchSenderTests
    {
        [Test]
        public void SendAnEmptySpanBatch()
        {
            var traceId = "123";
            var spanBatch = new SpanBatch(new List<Span>(), new Dictionary<string, object>(), traceId);
            var spanBatchMarshaller = Mock.Create<Sdk.SpanBatchMarshaller>();
            var batchDataSender = Mock.Create<BatchDataSender>();
            Mock.Arrange(() => batchDataSender.SendBatch(Arg.AnyString)).Returns(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            var spanBatchSender = new SpanBatchSender(batchDataSender, spanBatchMarshaller);
            var response = spanBatchSender.SendData(spanBatch);
            Assert.AreEqual(false, response.DidSend);
            Assert.IsNull(response.Message);
        }

        [Test]
        public void SendANonEmptySpanBatch()
        {
            var traceId = "123";
            var spanBatch = new SpanBatch(new List<Span>() { Mock.Create<Span>() }, new Dictionary<string, object>(), traceId);
            var spanBatchMarshaller = Mock.Create<Sdk.SpanBatchMarshaller>();
            var batchDataSender = Mock.Create<BatchDataSender>();
            Mock.Arrange(() => batchDataSender.SendBatch(Arg.AnyString)).Returns(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            var spanBatchSender = new SpanBatchSender(batchDataSender, spanBatchMarshaller);
            var response = spanBatchSender.SendData(spanBatch);
            Assert.AreEqual(true, response.DidSend);
            Assert.NotNull(response.Message);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.Message.StatusCode);
        }
    }
}
