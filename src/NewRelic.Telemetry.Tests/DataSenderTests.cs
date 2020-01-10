using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NewRelic.Telemetry.Tests
{
    public class DataSenderTests
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
        async public Task RequestTooLarge_SplitSuccess()
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

            var dataSender = new SpanDataSender(new TelemetryConfiguration().WithApiKey("123456"));

            var okJsons = new List<string>();

            // Mock the behavior to return EntityTooLarge for any span batch with 4 or more spans.
            // Anything with less than 4 will return success.
            dataSender.WithCaptureSendDataAsyncDelegate((spanBatch, retryNum) =>
            {
                actualCountCallsSendData++;

                if (spanBatch.Spans.Count < 4)
                {
                    okJsons.Add(spanBatch.ToJson());
                    successfulSpanBatches.Add(spanBatch);
                }
            });

            dataSender.WithHttpHandlerImpl((json) =>
            {
                var response = okJsons.Contains(json)
                    ? new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    : new HttpResponseMessage(System.Net.HttpStatusCode.RequestEntityTooLarge);

                return Task.FromResult(response);
            });

            var attribs = new Dictionary<string, object>()
            {
                {"testAttrib1", "testAttribValue1" }
            };

            var spanBatchBuilder = SpanBatchBuilder.Create()
                .WithTraceId(expectedTraceID)
                .WithAttributes(attribs);

            for (var i = 0; i < expectedCountSpans; i++)
            {
                spanBatchBuilder.WithSpan(SpanBuilder.Create(i.ToString()).Build());
            }

            var spanBatch = spanBatchBuilder.Build();

            // Act
            await dataSender.SendDataAsync(spanBatch);

            // Assert
            Assert.AreEqual(expectedCountCallsSendData, actualCountCallsSendData);

            //Test the Spans
            Assert.AreEqual(expectedCountSuccessfulSpanBatches, successfulSpanBatches.Count, "Unexpected number of calls");
            Assert.AreEqual(expectedCountSpans, successfulSpanBatches.SelectMany(x => x.Spans).Count(), "Unexpected number of successful Spans");
            Assert.AreEqual(expectedCountSpans, successfulSpanBatches.SelectMany(x => x.Spans).Select(x => x.Id).Distinct().Count(), "All Spans should be unique (spanId)");

            //Test the attributes on the spanbatch
            Assert.AreEqual(expectedCountDistinctTraceIds, successfulSpanBatches.Select(x => x.CommonProperties.TraceId).Distinct().Count(), "The traceId on split batches are not the same");
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
        async public Task RequestTooLarge_SplitFail()
        {
            const int expectedCountCallsSendData = 7;
            const int expectedCountSuccessfulSpanBatches = 1;
            const string traceID_Success = "OK";
            const string traceID_SplitBatch_Prefix = "TooLarge";

            var actualCountCallsSendData = 0;
            var successfulSpans = new List<Span>();

            var dataSender = new SpanDataSender(new TelemetryConfiguration().WithApiKey("123456"));

            var shouldSplitJsons = new List<string>();

            // Mock the behavior to return EntityTooLarge for any span batch that has a span with an 
            // id that starts with TooLarge.
            dataSender.WithCaptureSendDataAsyncDelegate((spanBatch, retryNum) =>
            {
                actualCountCallsSendData++;

                if (spanBatch.Spans.Any(x => x.Id.StartsWith(traceID_SplitBatch_Prefix)))
                {
                    shouldSplitJsons.Add(spanBatch.ToJson());
                }
                else
                {
                    successfulSpans.AddRange(spanBatch.Spans);
                }
            });

            dataSender.WithHttpHandlerImpl((json) =>
            {
                var response = shouldSplitJsons.Contains(json)
                    ? new HttpResponseMessage(System.Net.HttpStatusCode.RequestEntityTooLarge)
                    : new HttpResponseMessage(System.Net.HttpStatusCode.OK);

                return Task.FromResult(response);
            });

            var spanBatchBuilder = SpanBatchBuilder.Create();

            spanBatchBuilder.WithSpan(SpanBuilder.Create($"{traceID_SplitBatch_Prefix}1").Build());
            spanBatchBuilder.WithSpan(SpanBuilder.Create($"{traceID_SplitBatch_Prefix}2").Build());
            spanBatchBuilder.WithSpan(SpanBuilder.Create($"{traceID_SplitBatch_Prefix}3").Build());
            spanBatchBuilder.WithSpan(SpanBuilder.Create(traceID_Success).Build());

            var spanBatch = spanBatchBuilder.Build();

            // Act
            var result = await dataSender.SendDataAsync(spanBatch);

            // Assert
            Assert.AreEqual(NewRelicResponseStatus.Failure, result.ResponseStatus);
            Assert.AreEqual(expectedCountCallsSendData, actualCountCallsSendData, "Unexpected number of calls");
            Assert.AreEqual(expectedCountSuccessfulSpanBatches, successfulSpans.Count, $"Only {expectedCountSuccessfulSpanBatches} span should have successfully sent");
            Assert.AreEqual(traceID_Success, successfulSpans[0].Id, "Incorrect span was sent");
        }

        [Test]
        async public Task RetryBackoffSequence_RetriesExceeded()
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
            var actualBackoffSequenceFromTestRun = new List<uint>();
            var actualCountCallsSendData = 0;

            var dataSender = new SpanDataSender(new TelemetryConfiguration().WithApiKey("123456"));

            dataSender.WithDelayFunction(async (uint milliSecondsDelay) =>
            {
                actualBackoffSequenceFromTestRun.Add(milliSecondsDelay);
                await Task.Delay(0);
                return;
            });

            dataSender.WithCaptureSendDataAsyncDelegate((spanBatch, retryNum) =>
            {
                actualCountCallsSendData++;
            });

            dataSender.WithHttpHandlerImpl((json) =>
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.RequestTimeout));
            });

            var spanBatch = SpanBatchBuilder.Create()
                .WithSpan(SpanBuilder.Create("Test Span").Build())
                .Build();

            var result = await dataSender.SendDataAsync(spanBatch);

            Assert.AreEqual(NewRelicResponseStatus.Failure, result.ResponseStatus);
            Assert.AreEqual(HttpStatusCode.RequestTimeout, result.HttpStatusCode);
            Assert.AreEqual(expectedNumSendBatchAsyncCall, actualCountCallsSendData, "Unexpected Number of SendDataAsync calls");
            CollectionAssert.AreEqual(expectedBackoffSequenceFromTestRun, actualBackoffSequenceFromTestRun);
        }

        [Test]
        async public Task RetryBackoffSequence_IntermittentTimeoutEventuallySucceeds()
        {
            var expectedNumSendBatchAsyncCall = 4; // 1 first call + 3 calls from retries
            var expectedBackoffSequenceFromTestRun = new List<int>()
            {
                5000,
                10000,
                20000,
            };

            var actualBackoffSequenceFromTestRun = new List<uint>();

            var dataSender = new SpanDataSender(new TelemetryConfiguration().WithApiKey("123456"));
            dataSender.WithDelayFunction(async (uint milliSecondsDelay) =>
            {
                actualBackoffSequenceFromTestRun.Add(milliSecondsDelay);
                await Task.Delay(0);
                return;
            });

            var actualCountCallsSendData = 0;
            dataSender.WithCaptureSendDataAsyncDelegate((spanBatch, retryNum) =>
            {
                actualCountCallsSendData++;
            });

            var callCount = 0;
            dataSender.WithHttpHandlerImpl((json) =>
            {
                callCount++;
                if (callCount < 4)
                {
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.RequestTimeout));
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            });

            var spanBatch = SpanBatchBuilder.Create()
               .WithSpan(SpanBuilder.Create("Test Span").Build())
               .Build();

            var result = await dataSender.SendDataAsync(spanBatch);

            Assert.AreEqual(NewRelicResponseStatus.Success, result.ResponseStatus);
            Assert.AreEqual(expectedNumSendBatchAsyncCall, actualCountCallsSendData, "Unexpected Number of SendDataAsync calls");
            CollectionAssert.AreEqual(expectedBackoffSequenceFromTestRun, actualBackoffSequenceFromTestRun);
        }

        [Test]
        async public Task RetryOn429_RetriesExceeded()
        {
            const int delayMS = 10000;
            const int expectedNumSendBatchAsyncCall = 9; // 1 first call + 3 calls from retries
            var expectedBackoffSequenceFromTestRun = new List<int>()
            {
                delayMS,
                delayMS,
                delayMS,
                delayMS,
                delayMS,
                delayMS,
                delayMS,
                delayMS
            };

            var actualBackoffSequenceFromTestRun = new List<uint>();

            var dataSender = new SpanDataSender(new TelemetryConfiguration().WithApiKey("123456"));
            dataSender.WithDelayFunction(async (uint milliSecondsDelay) =>
            {
                actualBackoffSequenceFromTestRun.Add(milliSecondsDelay);
                await Task.Delay(0);
                return;
            });

            var actualCountCallsSendData = 0;
            dataSender.WithCaptureSendDataAsyncDelegate((spanBatch, retryNum) =>
            {
                actualCountCallsSendData++;
            });

            dataSender.WithHttpHandlerImpl((json) =>
            {
                var httpResponse = new HttpResponseMessage((HttpStatusCode)429);
                httpResponse.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromMilliseconds(delayMS));

                return Task.FromResult(httpResponse);
            });

            var spanBatch = SpanBatchBuilder.Create()
               .WithSpan(SpanBuilder.Create("Test Span").Build())
               .Build();

            var result = await dataSender.SendDataAsync(spanBatch);

            Assert.AreEqual(NewRelicResponseStatus.Failure, result.ResponseStatus);
            Assert.AreEqual(expectedNumSendBatchAsyncCall, actualCountCallsSendData, "Unexpected Number of SendDataAsync calls");
            CollectionAssert.AreEqual(expectedBackoffSequenceFromTestRun, actualBackoffSequenceFromTestRun);
        }

        [Test]
        async public Task RetryOn429WithDuration_429HappensOnce()
        {
            const int delayMS = 10000;
            const int expectedNumSendBatchAsyncCall = 2;
            var expectedBackoffSequenceFromTestRun = new List<int>()
            {
                delayMS
            };

            var actualBackoffSequenceFromTestRun = new List<uint>();

            var dataSender = new SpanDataSender(new TelemetryConfiguration().WithApiKey("123456"));

            dataSender.WithDelayFunction(async (uint milliSecondsDelay) =>
            {
                actualBackoffSequenceFromTestRun.Add(milliSecondsDelay);
                await Task.Delay(0);
                return;
            });

            var actualCountCallsSendData = 0;
            dataSender.WithCaptureSendDataAsyncDelegate((spanBatch, retryNum) =>
            {
                actualCountCallsSendData++;
            });

            dataSender.WithHttpHandlerImpl((json) =>
            {
                if (actualCountCallsSendData < 2)
                {
                    var httpResponse = new HttpResponseMessage((HttpStatusCode)429);
                    httpResponse.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromMilliseconds(delayMS));

                    return Task.FromResult(httpResponse);
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            });

            var spanBatch = SpanBatchBuilder.Create()
               .WithSpan(SpanBuilder.Create("Test Span").Build())
               .Build();

            var result = await dataSender.SendDataAsync(spanBatch);

            Assert.AreEqual(NewRelicResponseStatus.Success, result.ResponseStatus);
            Assert.AreEqual(expectedNumSendBatchAsyncCall, actualCountCallsSendData, "Unexpected Number of SendDataAsync calls");
            CollectionAssert.AreEqual(expectedBackoffSequenceFromTestRun, actualBackoffSequenceFromTestRun);
        }

        [Test]
        async public Task RetryOn429WithSpecificDate_429HappensOnce()
        {
            const int delayMs = 10000;
            // The actual retry delay will be slightly less than delayMs since UtcNow is recalculated in RetryWithServerDelay()
            var errorMargin = TimeSpan.FromMilliseconds(50).TotalMilliseconds;
            var actualResponseFromTestRun = new List<Response>();

            uint actualDelayFromTestRun = 0;

            var dataSender = new SpanDataSender(new TelemetryConfiguration().WithApiKey("123456").WithMaxRetryAttempts(1));

            dataSender.WithDelayFunction(async (uint milliSecondsDelay) =>
            {
                actualDelayFromTestRun = milliSecondsDelay;
                await Task.Delay(0);
                return;
            });

            dataSender.WithHttpHandlerImpl((json) =>
            {
                var httpResponse = new HttpResponseMessage((HttpStatusCode)429);
                var retryOnSpecificTime = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(delayMs);
                httpResponse.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(retryOnSpecificTime);

                return Task.FromResult(httpResponse);
            });

            var spanBatch = SpanBatchBuilder.Create()
               .WithSpan(SpanBuilder.Create("Test Span").Build())
               .Build();

            var response = await dataSender.SendDataAsync(spanBatch);

            Assert.IsTrue(actualDelayFromTestRun >= delayMs - errorMargin && actualDelayFromTestRun <= delayMs + errorMargin,$"Expected delay: {delayMs}, margin: +/-{errorMargin}, actual delay: {actualDelayFromTestRun}");
        }

        [Test]
        async public Task SendDataAsyncThrowsHttpException()
        {
            const int expectedNumSendBatchAsyncCall = 1;

            var dataSender = new SpanDataSender(new TelemetryConfiguration().WithApiKey("123456"));

            dataSender.WithDelayFunction(async (uint milliSecondsDelay) =>
            {
                await Task.Delay(0);
                return;
            });

            var actualCountCallsSendData = 0;
            dataSender.WithCaptureSendDataAsyncDelegate((spanBatch, retryNum) =>
            {
                actualCountCallsSendData++;
            });

            dataSender.WithHttpHandlerImpl((json) =>
            {
                throw new Exception("Server Error", new Exception("Inner exception message"));
            });

            var spanBatch = SpanBatchBuilder.Create()
               .WithSpan(SpanBuilder.Create("Test Span").Build())
               .Build();

            var result = await dataSender.SendDataAsync(spanBatch);

            Assert.AreEqual(NewRelicResponseStatus.Failure, result.ResponseStatus);
            Assert.AreEqual("Inner exception message", result.Message);
            Assert.IsNull(result.HttpStatusCode);
            Assert.AreEqual(expectedNumSendBatchAsyncCall, actualCountCallsSendData, "Unexpected Number of SendDataAsync calls");
        }


        [Test]
        async public Task SendDataAsyncThrowsNonHttpException()
        {
            const int expectedNumSendBatchAsyncCall = 1;
            const int expectedNumHttpHandlerCall = 0;

            var dataSender = new SpanDataSender(new TelemetryConfiguration().WithApiKey("123456"));

            dataSender.WithDelayFunction(async (uint milliSecondsDelay) =>
            {
                await Task.Delay(0);
                return;
            });

            var actualCountCallsSendData = 0;
            dataSender.WithCaptureSendDataAsyncDelegate((sb, retry) =>
            {
                actualCountCallsSendData++;
                throw new Exception("Test Exception");
            });

            var actualCallsHttpHandler = 0;
            dataSender.WithHttpHandlerImpl((json) =>
            {
                actualCallsHttpHandler++;
                return Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });
            });


            var spanBatch = SpanBatchBuilder.Create()
               .WithSpan(SpanBuilder.Create("Test Span").Build())
               .Build();

            var result = await dataSender.SendDataAsync(spanBatch);

            Assert.AreEqual(NewRelicResponseStatus.Failure, result.ResponseStatus);
            Assert.AreEqual("Test Exception", result.Message);
            Assert.IsNull(result.HttpStatusCode);

            Assert.AreEqual(expectedNumSendBatchAsyncCall, actualCountCallsSendData, "Unexpected Number of SendDataAsync calls");
            Assert.AreEqual(expectedNumHttpHandlerCall, actualCallsHttpHandler, "Unexpected Number of Http Handler calls");
        }

        [TestCase(null, "1.0.0")]
        [TestCase("productName", null)]
        [TestCase("", "")]
        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase(null, "")]
        [TestCase("productName", "1.0.0")]
        public void AddVersionInfo(string productName, string productVersion)
        {
            var dataSender = new SpanDataSender(new TelemetryConfiguration().WithApiKey("123456"));

            var expectedUserAgentValue = dataSender.UserAgent;

            if (!string.IsNullOrEmpty(productName) && !string.IsNullOrEmpty(productVersion)) 
            {
                expectedUserAgentValue += " " + $@"{productName}/{productVersion}";
            }

            dataSender.AddVersionInfo(productName, productVersion);

            var userAgentValueAfter = dataSender.UserAgent;

            Assert.AreEqual(expectedUserAgentValue, userAgentValueAfter);
        }
    }
}
