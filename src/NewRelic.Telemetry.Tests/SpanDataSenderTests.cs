using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;

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

            var dataSender = new SpanDataSender(new TelemetryConfiguration().WithAPIKey("123456"));

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

            var dataSender = new SpanDataSender(new TelemetryConfiguration().WithAPIKey("123456"));

            dataSender.WithHttpHandlerImpl((serializedJson) =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                return Task.FromResult(response);
            });

            var response = dataSender.SendDataAsync(spanBatch).Result;

            Assert.AreEqual(NewRelicResponseStatus.Success, response.ResponseStatus);
        }
    }
}
