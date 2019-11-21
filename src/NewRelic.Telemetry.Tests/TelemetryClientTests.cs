using Moq;
using NewRelic.Telemetry.Client;
using NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NewRelic.Telemetry.Tests
{
    class TelemetryClientTests
    {


        /// <summary>
        /// Test will manipulate the New Relic End point response such that any request with 4 or more spans will result in a 
        /// RequestTooLarge response. This will test:
        /// 
        ///     * The splitting logic is working properly
        ///     * All of the spans eventually are sent
        ///     * Spans are only sent once
        ///     * The Attributes on the span batch are copied on the split batches
        /// 
        /// Test Case:  9 Spans
        ///             A.  Initial Request                     9 spans         --> Too Large (results in B and C)
        ///             B.  1/2 of Request A                    5 spans         --> Too Large (results in D and E)
        ///             C.  1/2 of Request A                    4 spans         --> Too Large (results in F and G)
        ///             D.  1/2 of Request B                    3 spans         --> OK
        ///             E.  1/2 of Request B                    2 spans         --> OK
        ///             F.  1/2 of Request C                    2 spans         --> OK
        ///             G.  1/2 of Request C                    2 spans         --> OK
        ///         --------------------------------------------------------------------------
        ///             Total = 7 Requests/Batches              9 spans
        /// </summary>
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
                        return Task.FromResult( new Response(true, System.Net.HttpStatusCode.RequestEntityTooLarge, null));
                    }

                    successfulSpanBatches.Add(sb);
                    return Task.FromResult(new Response(true, System.Net.HttpStatusCode.OK, null));
                });

            var attribs = new Dictionary<string, object>()
            {
                {"testAttrib1", "testAttribValue1" }
            };

            var spanBatchBuilder = SpanBatchBuilder.Create()
                .WithTraceId(expectedTraceID)
                .WithAttributes(attribs);

            for(var i = 0; i < expectedCountSpans; i++)
            {
                spanBatchBuilder.WithSpan(SpanBuilder.Create(i.ToString()).Build());
            }

            var spanBatch = spanBatchBuilder.Build();

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
            Assert.AreEqual(expectedCountDistinctTraceIds, successfulSpanBatches.Select(x => x.CommonProperties.TraceId).Distinct().Count(),"The traceId on split batches are not the same");
            Assert.AreEqual(expectedTraceID, successfulSpanBatches.FirstOrDefault().CommonProperties.TraceId, "The traceId on split batches does not match the original traceId");
            Assert.AreEqual(expectedCountSpanBatchAttribSets, successfulSpanBatches.Select(x => x.CommonProperties.Attributes).Distinct().Count(), "The attributes on all span batches should be the same");
            Assert.AreEqual(attribs, successfulSpanBatches.Select(x => x.CommonProperties.Attributes).FirstOrDefault(), "The Span Batch attribute values on split batches do not match the attributes of the original span batch.");
        }

        /// <summary>
        /// Test will create a batch of 4-spans (3 named as 'TooLarge' and 1 maked as 'OK').  It will manipulate the New Relic endpoint response such that
        /// if any of the spans in the batch is named as 'TooLarge' the endpoint will report RequestTooLarge and invoke the splitting logic.
        /// Basd on the splitting logic, it is expected that there will eventually be a split resulting in a single span of 1 item that is 'TooLarge'.
        /// 
        ///     * The splitting logic is working properly
        ///     * The splitting logic stops if there is only a single span that is Too Large.
        ///     * The spans that are not too large are eventually sent.
        /// 
        /// Test Case:  4 Spans
        ///             A.  Initial Request             TooLarge1, TooLarge2, TooLarge3, OK         -->  Too Large (results in B and C)
        ///             B.  1/2 of Request A            TooLarge1, TooLarge2                        -->  Too Large (results in D and E)
        ///             C.  1/2 of Request A            TooLarge3, OK                               -->  Too Large (results in E and F)
        ///             D.  1/2 of Request B            TooLarge1                                   -->  Too Large (Can't Split)
        ///             E.  1/2 of Request B            TooLarge2                                   -->  Too Large (Can't Split)
        ///             F.  1/2 of Request C            TooLarge3                                   -->  Too Large (Can't Split)
        ///             G.  1/2 of Request C            OK                                          -->  Success
        ///         ----------------------------------------------------------------------------------------------------------------------
        ///             Total = 7 Requests/Batches      4 spans requested, 1 span successful
        /// </summary>

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
                        return Task.FromResult(new Response(true, System.Net.HttpStatusCode.RequestEntityTooLarge, null));
                    }

                    successfulSpans.AddRange(sb.Spans);
                    return Task.FromResult(new Response(true, System.Net.HttpStatusCode.OK, null));
                });

            var spanBatchBuilder = SpanBatchBuilder.Create();

            spanBatchBuilder.WithSpan(SpanBuilder.Create($"{traceID_SplitBatch_Prefix}1").Build());
            spanBatchBuilder.WithSpan(SpanBuilder.Create($"{traceID_SplitBatch_Prefix}2").Build());
            spanBatchBuilder.WithSpan(SpanBuilder.Create($"{traceID_SplitBatch_Prefix}3").Build());
            spanBatchBuilder.WithSpan(SpanBuilder.Create(traceID_Success).Build());

            var spanBatch = spanBatchBuilder.Build();

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
                    return Task.FromResult(new Response(true, System.Net.HttpStatusCode.RequestTimeout, null));
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
                        return Task.FromResult(new Response(true, System.Net.HttpStatusCode.RequestTimeout, null));
                    }

                    return Task.FromResult(new Response(true, System.Net.HttpStatusCode.Accepted, null));

                });

            var client = new TelemetryClient(mockSpanBatchSender.Object, customDelayer);

            await client.SendBatchAsync(It.IsAny<SpanBatch>());

            mockSpanBatchSender.Verify(x => x.SendDataAsync(It.IsAny<SpanBatch>()), Times.Exactly(expectedNumSendBatchAsyncCall));
            CollectionAssert.AreEqual(expectedBackoffSequenceFromTestRun, actualBackoffSequenceFromTestRun);
            return;
        }

        [Test]
        async public Task TestTelemetryClient_RetryOn429_RetriesExceeded()
        {
            var expectedNumSendBatchAsyncCall = 9; // 1 first call + 3 calls from retries
            var expectedBackoffSequenceFromTestRun = new List<int>()
            {
                10000,
                10000,
                10000,
                10000,
                10000,
                10000,
                10000,
                10000
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

                    var response = new Response(true, (System.Net.HttpStatusCode)429, TimeSpan.FromSeconds(10));
                    return Task.FromResult(response);
                });

            var client = new TelemetryClient(mockSpanBatchSender.Object, customDelayer);

            await client.SendBatchAsync(It.IsAny<SpanBatch>());

            mockSpanBatchSender.Verify(x => x.SendDataAsync(It.IsAny<SpanBatch>()), Times.Exactly(expectedNumSendBatchAsyncCall));
            CollectionAssert.AreEqual(expectedBackoffSequenceFromTestRun, actualBackoffSequenceFromTestRun);
            return;
        }


        [Test]
        async public Task TestTelemetryClient_RetryOn429_429HappensOnce()
        {
            var expectedNumSendBatchAsyncCall = 2; // 1 first call + 3 calls from retries
            var expectedBackoffSequenceFromTestRun = new List<int>()
            {
                10000,
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
                    if (callCount < 2)
                    {
                        var response = new Response(true, (System.Net.HttpStatusCode)429, TimeSpan.FromSeconds(10));
                        return Task.FromResult(response);
                    }
                    return Task.FromResult(new Response(true, System.Net.HttpStatusCode.Accepted, null));
                });

            var client = new TelemetryClient(mockSpanBatchSender.Object, customDelayer);

            await client.SendBatchAsync(It.IsAny<SpanBatch>());

            mockSpanBatchSender.Verify(x => x.SendDataAsync(It.IsAny<SpanBatch>()), Times.Exactly(expectedNumSendBatchAsyncCall));
            CollectionAssert.AreEqual(expectedBackoffSequenceFromTestRun, actualBackoffSequenceFromTestRun);
            return;
        }

    }
}
