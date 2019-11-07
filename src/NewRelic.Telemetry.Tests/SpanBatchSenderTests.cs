using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Telerik.JustMock;

namespace NewRelic.Telemetry.Tests
{
    public class SpanBatchSenderTests
    {
        [Test]
        public void SendAnEmptySpanBatch()
        {
            var traceId = "123";
            var spanBatch = new SpanBatch(new List<Span>(), new Dictionary<string, object>(), traceId);
            var spanBatchMarshaller = Mock.Create<SpanBatchMarshaller>();
            var batchDataSender = Mock.Create<BatchDataSender>();
            Mock.Arrange(() => batchDataSender.SendBatchAsync(Arg.AnyString)).Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
            var spanBatchSender = new SpanBatchSender(batchDataSender, spanBatchMarshaller);

            var response = spanBatchSender.SendDataAsync(spanBatch).Result;

            Assert.AreEqual(false, response.DidSend);
        }

        [Test]
        public void SendANonEmptySpanBatch()
        {
            var traceId = "123";
            var spanBatch = new SpanBatch(new List<Span>() { Mock.Create<Span>() }, new Dictionary<string, object>(), traceId);
            var spanBatchMarshaller = Mock.Create<SpanBatchMarshaller>();
            var batchDataSender = Mock.Create<BatchDataSender>();
            Mock.Arrange(() => batchDataSender.SendBatchAsync(Arg.AnyString)).Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
            var spanBatchSender = new SpanBatchSender(batchDataSender, spanBatchMarshaller);

            var response = spanBatchSender.SendDataAsync(spanBatch).Result;

            Assert.AreEqual(true, response.DidSend);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }
}
