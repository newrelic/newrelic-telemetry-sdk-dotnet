using Moq;
using NewRelic.Telemetry.Client;
using NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewRelic.Telemetry.Tests
{
    class TelemetryClientTests
    {
        [Test]
        async public Task TestTelemetryClient_RetryBackoffSequence()
        {
            var expectedBackoffSequenceFromTestRun = new List<int>() 
            {
                5000,
                10000,
                20000,
                40000,
                80000,
                80000,
                80000,
                80000
            };

            var actualBackoffSequenceFromTestRun = new List<int>();

            var mockDelayer = new Mock<IAsyncDelayer>();
            mockDelayer.Setup(x => x.Delay(It.IsAny<int>()))
                .Returns((int milliSecondsDelay) =>
                {
                    actualBackoffSequenceFromTestRun.Add(milliSecondsDelay);
                    return Task.FromResult(0);
                });

            var mockSpanBatchSender = new Mock<ISpanBatchSender>();
            mockSpanBatchSender.Setup(x => x.SendDataAsync(It.IsAny<SpanBatch>()))
                .Returns(() =>
                {
                    return Task.FromResult(new Response(true, System.Net.HttpStatusCode.RequestTimeout));
                });

            var client = new TelemetryClient(mockSpanBatchSender.Object, mockDelayer.Object);

            await client.SendBatchAsync(It.IsAny<SpanBatch>());

            mockSpanBatchSender.Verify(x => x.SendDataAsync(It.IsAny<SpanBatch>()), Times.Exactly(9));
            CollectionAssert.AreEqual(expectedBackoffSequenceFromTestRun, actualBackoffSequenceFromTestRun);
            return;
        }
    }
}
