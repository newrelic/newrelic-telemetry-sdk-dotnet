using Moq;
using NewRelic.Telemetry.Client;
using NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewRelic.Telemetry.Tests
{
    class TelemetryClientTests
    {
        [Test]
        async public Task TestTelemetryClient_RequestTooLarge_SplitSuccess()
        {
            const int countSpans = 9;
            var countCalls = 0;

            // Arrange
            var successfulSpanBatches = new List<SpanBatch>();

            var mockSpanBatchSender = new Mock<ISpanBatchSender>();
            mockSpanBatchSender.Setup(x => x.SendDataAsync(It.IsAny<SpanBatch>()))
                
                .Returns<SpanBatch>((sb) =>
                {
                    countCalls++;

                    if (sb.Spans.Count >= 4)
                    {
                        return Task.FromResult(new Response(true, System.Net.HttpStatusCode.RequestEntityTooLarge));
                    }

                    successfulSpanBatches.Add(sb);
                    return Task.FromResult(new Response(true, System.Net.HttpStatusCode.OK));
                });

            var attribs = new Dictionary<string, object>()
            {
                {"testAttrib1", "testAttribValue1" }
            };

            var spans = new List<Span>();
            for(var i = 0; i < countSpans; i++)
            {
                var spanBuilder = new SpanBuilder(i.ToString());
                spans.Add(spanBuilder.Build());
            }

            var spanBatch = new SpanBatch(spans, attribs, "TestTrace");

            // Act
            var client = new TelemetryClient(mockSpanBatchSender.Object);
            await client.SendBatchAsync(spanBatch);

            // Assert
            Assert.AreEqual(7, countCalls);

            //Test the Spans
            Assert.AreEqual(4, successfulSpanBatches.Count,"Unexpected number of calls");
            Assert.AreEqual(countSpans, successfulSpanBatches.SelectMany(x => x.Spans).Count(),"Unexpected number of successful Spans");
            Assert.AreEqual(countSpans, successfulSpanBatches.SelectMany(x => x.Spans).Select(x => x.Id).Distinct().Count(),"All Spans should be unique (spanId)");
            
            //Test the attributes on the spanbatch
            Assert.AreEqual(1, successfulSpanBatches.Select(x => x.TraceId).Distinct().Count(),"The traceId on split batches are not the same");
            Assert.AreEqual("TestTrace", successfulSpanBatches.FirstOrDefault().TraceId,"The traceId on split batches does not match the original traceId");
            Assert.AreEqual(1, successfulSpanBatches.Select(x => x.Attributes).Distinct().Count(), "The attributes on all span batches should be the same");
            Assert.AreEqual(attribs, successfulSpanBatches.Select(x => x.Attributes).FirstOrDefault(), "The attributes on all span batches should be the same");
        }

        [Test]
        async public Task TestTelemetryClient_RequestTooLarge_SplitFail()
        {
            var countCalls = 0;
            var successfulSpans = new List<Span>();

            var mockSpanBatchSender = new Mock<ISpanBatchSender>();
            mockSpanBatchSender.Setup(x => x.SendDataAsync(It.IsAny<SpanBatch>()))
                .Returns<SpanBatch>((sb) =>
                {
                    countCalls++;
                    
                    if(sb.Spans.Any(x=>x.Id.StartsWith("TooLarge")))
                    {
                        return Task.FromResult(new Response(true, System.Net.HttpStatusCode.RequestEntityTooLarge));
                    }

                    successfulSpans.AddRange(sb.Spans);
                    return Task.FromResult(new Response(true, System.Net.HttpStatusCode.OK));
                });

            var spans = new List<Span>();
            spans.Add(new SpanBuilder("TooLarge1").Build());
            spans.Add(new SpanBuilder("TooLarge2").Build());
            spans.Add(new SpanBuilder("TooLarge3").Build());
            spans.Add(new SpanBuilder("OK").Build());
            
            var spanBatch = new SpanBatch(spans, null,null);

            // Act
            var client = new TelemetryClient(mockSpanBatchSender.Object);
            await client.SendBatchAsync(spanBatch);

            // Assert
            Assert.AreEqual(7, countCalls, "Unexpected number of calls");
            Assert.AreEqual(1, successfulSpans.Count, "Only 1 span should have successfully sent");
            Assert.AreEqual("OK", successfulSpans[0].Id, "Incorrect span was sent");
        }

        [Test]
        async public Task TestTelemetryClient_RetryBackoffSequence_RetriesExceeded()
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
                return ;
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
