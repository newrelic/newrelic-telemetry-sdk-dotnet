using Moq;
using NewRelic.Telemetry.Client;
using NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NewRelic.Telemetry.Tests
{
    class TelemetryClientTests
    {
        [Test]
        async public Task TestTelemetryClient_Retry()
        {
            var calls = 0;
            var mockSpanBatchSender = new Mock<ISpanBatchSender>();
            mockSpanBatchSender.Setup(x => x.SendDataAsync(It.IsAny<SpanBatch>()))
            .Callback(() =>
            {
                calls++;
            }).Returns(() => {
                if (calls == 3) 
                {
                    return Task.FromResult(new Response(true, System.Net.HttpStatusCode.OK));
                }
                return Task.FromResult(new Response(true, System.Net.HttpStatusCode.RequestTimeout));
            });

            var client = new TelemetryClient(mockSpanBatchSender.Object);
            await client.SendBatchAsync(It.IsAny<SpanBatch>());

            mockSpanBatchSender.Verify(x => x.SendDataAsync(It.IsAny<SpanBatch>()), Times.Exactly(3));

            return;
        }
    }
}
