using Moq;
using NewRelic.Telemetry.Client;
using NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewRelic.Telemetry.Tests
{
    class TelemetryClientTests
    {
        [Test]
        async public Task TestTelemetryClient_RetryBackoffSequence_RemoteServiceTimeoutForGood()
        {
            var expectedNumSendBatchAsyncCall = 9; //1 first call + 8 calls from retries
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

            var customDelayer = new Func<int, Task>(async (int milliSecondsDelay) => 
            {
                actualBackoffSequenceFromTestRun.Add(milliSecondsDelay);
                await Task.Delay(0);
            });

            var mockSpanBatchSender = new Mock<ISpanBatchSender>();
            mockSpanBatchSender.Setup(x => x.SendDataAsync(It.IsAny<SpanBatch>()))
                .Returns(() =>
                {
                    return Task.FromResult(new Response(true, System.Net.HttpStatusCode.RequestTimeout));
                });

            var client = new TelemetryClient(mockSpanBatchSender.Object, customDelayer);

            await client.SendBatchAsync(It.IsAny<SpanBatch>());

            mockSpanBatchSender.Verify(x => x.SendDataAsync(It.IsAny<SpanBatch>()), Times.Exactly(expectedNumSendBatchAsyncCall));
            CollectionAssert.AreEqual(expectedBackoffSequenceFromTestRun, actualBackoffSequenceFromTestRun);
            return;
        }


        [Test]
        async public Task TestTelemetryClient_RetryBackoffSequence_RemoteSeriviceTimeOutNotForLong()
        {
            var expectedNumSendBatchAsyncCall = 4; // 1 first call + 3 calls from retries
            var expectedBackoffSequenceFromTestRun = new List<int>()
            {
                5000,
                10000,
                20000,
            };
            var actualBackoffSequenceFromTestRun = new List<int>();

            var customDelayer = new Func<int, Task>(async (int milliSecondsDelay) =>
            {
                actualBackoffSequenceFromTestRun.Add(milliSecondsDelay);
                await Task.Delay(0);
            });

            var callCount = 0;
            var mockSpanBatchSender = new Mock<ISpanBatchSender>();
            mockSpanBatchSender.Setup(x => x.SendDataAsync(It.IsAny<SpanBatch>()))
                .Returns(() =>
                {
                    callCount++;
                    if (callCount < 4)
                    {
                        return Task.FromResult(new Response(true, System.Net.HttpStatusCode.RequestTimeout));
                    }

                    return Task.FromResult(new Response(true, System.Net.HttpStatusCode.Accepted));

                });

            var client = new TelemetryClient(mockSpanBatchSender.Object, customDelayer);

            await client.SendBatchAsync(It.IsAny<SpanBatch>());

            mockSpanBatchSender.Verify(x => x.SendDataAsync(It.IsAny<SpanBatch>()), Times.Exactly(expectedNumSendBatchAsyncCall));
            CollectionAssert.AreEqual(expectedBackoffSequenceFromTestRun, actualBackoffSequenceFromTestRun);
            return;
        }
    }
}
