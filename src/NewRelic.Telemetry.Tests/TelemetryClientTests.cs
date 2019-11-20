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
            const int expectedCountSpans = 9;
            const int expectedCountCallsSendData = 7;
            const int expectedCountSuccessfulSpanBatches = 4;
            const int expectedCountDistinctTraceIds = 1;
            const int expectedCountSpanBatchAttribSets = 1;
            const string expectedTraceID = "TestTrace";


            var actualCountCallsSendData = 0;
            

            // Arrange
            var successfulSpanBatches = new List<SpanBatch>();

            var mockSpanBatchSender = new Mock<ISpanBatchSender>();
            mockSpanBatchSender.Setup(x => x.SendDataAsync(It.IsAny<SpanBatch>()))
                
                .Returns<SpanBatch>((sb) =>
                {
                    actualCountCallsSendData++;

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
            for(var i = 0; i < expectedCountSpans; i++)
            {
                var spanBuilder = new SpanBuilder(i.ToString());
                spans.Add(spanBuilder.Build());
            }

            var spanBatch = new SpanBatch(spans, attribs, expectedTraceID);

            // Act
            var client = new TelemetryClient(mockSpanBatchSender.Object);
            await client.SendBatchAsync(spanBatch);

            // Assert
            Assert.AreEqual(expectedCountCallsSendData, actualCountCallsSendData);

            //Test the Spans
            Assert.AreEqual(expectedCountSuccessfulSpanBatches, successfulSpanBatches.Count,"Unexpected number of calls");
            Assert.AreEqual(expectedCountSpans, successfulSpanBatches.SelectMany(x => x.Spans).Count(),"Unexpected number of successful Spans");
            Assert.AreEqual(expectedCountSpans, successfulSpanBatches.SelectMany(x => x.Spans).Select(x => x.Id).Distinct().Count(),"All Spans should be unique (spanId)");
            
            //Test the attributes on the spanbatch
            Assert.AreEqual(expectedCountDistinctTraceIds, successfulSpanBatches.Select(x => x.TraceId).Distinct().Count(),"The traceId on split batches are not the same");
            Assert.AreEqual(expectedTraceID, successfulSpanBatches.FirstOrDefault().TraceId,"The traceId on split batches does not match the original traceId");
            Assert.AreEqual(expectedCountSpanBatchAttribSets, successfulSpanBatches.Select(x => x.Attributes).Distinct().Count(), "The attributes on all span batches should be the same");
            Assert.AreEqual(attribs, successfulSpanBatches.Select(x => x.Attributes).FirstOrDefault(), "The Span Batch attribute values on split batches do not match the attributes of the original span batch.");
        }

        [Test]
        async public Task TestTelemetryClient_RequestTooLarge_SplitFail()
        {

            const int expectedCountCallsSendData = 7;
            const int expectedCountSuccessfulSpanBatches = 1;
            const string traceID_Success = "OK";
            const string traceID_SplitBatch_Prefix = "TooLarge";

            var actualCountCallsSendData = 0;
            var successfulSpans = new List<Span>();

            var mockSpanBatchSender = new Mock<ISpanBatchSender>();
            mockSpanBatchSender.Setup(x => x.SendDataAsync(It.IsAny<SpanBatch>()))
                .Returns<SpanBatch>((sb) =>
                {
                    actualCountCallsSendData++;
                    
                    if(sb.Spans.Any(x=>x.Id.StartsWith(traceID_SplitBatch_Prefix)))
                    {
                        return Task.FromResult(new Response(true, System.Net.HttpStatusCode.RequestEntityTooLarge));
                    }

                    successfulSpans.AddRange(sb.Spans);
                    return Task.FromResult(new Response(true, System.Net.HttpStatusCode.OK));
                });

            var spans = new List<Span>();
            spans.Add(new SpanBuilder($"{traceID_SplitBatch_Prefix}1").Build());
            spans.Add(new SpanBuilder($"{traceID_SplitBatch_Prefix}2").Build());
            spans.Add(new SpanBuilder($"{traceID_SplitBatch_Prefix}3").Build());
            spans.Add(new SpanBuilder(traceID_Success).Build());
            
            var spanBatch = new SpanBatch(spans, null,null);

            // Act
            var client = new TelemetryClient(mockSpanBatchSender.Object);
            await client.SendBatchAsync(spanBatch);

            // Assert
            Assert.AreEqual(expectedCountCallsSendData, actualCountCallsSendData, "Unexpected number of calls");
            Assert.AreEqual(expectedCountSuccessfulSpanBatches, successfulSpans.Count, $"Only {expectedCountSuccessfulSpanBatches} span should have successfully sent");
            Assert.AreEqual(traceID_Success, successfulSpans[0].Id, "Incorrect span was sent");
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
