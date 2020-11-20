// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NewRelic.Telemetry.Tracing;
using NewRelic.Telemetry.Transport;
using Xunit;

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
        [Fact]
        public async Task RequestTooLarge_SplitSuccess()
        {
            const int expectedCountSpans = 9;
            const int expectedCountCallsSendData = 7;
            const int expectedCountSuccessfulSpanBatches = 4;
            const string expectedTraceID = "TestTrace";

            var actualCountCallsSendData = 0;

            // Arrange
            var successfulSpanBatches = new List<NewRelicSpanBatch>();

            var dataSender = new TraceDataSender(new TelemetryConfiguration() { ApiKey = "123456" }, null);

            var okJsons = new List<string>();

            // Mock the behavior to return EntityTooLarge for any span batch with 4 or more spans.
            // Anything with less than 4 will return success.
            dataSender.WithCaptureSendDataAsyncDelegate((spanBatch, retryNum) =>
            {
                actualCountCallsSendData++;

                if (spanBatch.Spans.Count() < 4)
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
                { "testAttrib1", "testAttribValue1" },
            };

            var spans = new List<NewRelicSpan>();
            for (var i = 0; i < expectedCountSpans; i++)
            {
                spans.Add(new NewRelicSpan(null, i.ToString(), i, null, null));
            }

            var spanBatch = new NewRelicSpanBatch(spans, new NewRelicSpanBatchCommonProperties(expectedTraceID, attribs));

            // Act
            await dataSender.SendDataAsync(spanBatch);

            // Assert
            Assert.Equal(expectedCountCallsSendData, actualCountCallsSendData);

            // Test the Spans
            Assert.Equal(expectedCountSuccessfulSpanBatches, successfulSpanBatches.Count);
            Assert.Equal(expectedCountSpans, successfulSpanBatches.SelectMany(x => x.Spans).Count());
            Assert.Equal(expectedCountSpans, successfulSpanBatches.SelectMany(x => x.Spans).Select(x => x.Id).Distinct().Count());

            // Test the attributes on the NewRelicSpanBatch
            Assert.Single(successfulSpanBatches.Select(x => x.CommonProperties.TraceId).Distinct());
            Assert.Equal(expectedTraceID, successfulSpanBatches.FirstOrDefault().CommonProperties.TraceId);
            Assert.Single(successfulSpanBatches.Select(x => x.CommonProperties.Attributes).Distinct());
            Assert.Equal(attribs, successfulSpanBatches.Select(x => x.CommonProperties.Attributes).FirstOrDefault());
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
        [Fact]
        public async Task RequestTooLarge_SplitFail()
        {
            const int expectedCountCallsSendData = 7;
            const string traceID_Success = "OK";
            const string traceID_SplitBatch_Prefix = "TooLarge";

            var actualCountCallsSendData = 0;
            var successfulSpans = new List<NewRelicSpan>();

            var dataSender = new TraceDataSender(new TelemetryConfiguration() { ApiKey = "123456" }, null);

            var shouldSplitJsons = new List<string>();

            // Mock the behavior to return EntityTooLarge for any span batch that has a span with an 
            // id that starts with TooLarge.
            dataSender.WithCaptureSendDataAsyncDelegate((newRelicSpanBatch, retryNum) =>
            {
                actualCountCallsSendData++;

                if (newRelicSpanBatch.Spans == null)
                {
                    return;
                }

                if (newRelicSpanBatch.Spans.Any(x => x.Id.StartsWith(traceID_SplitBatch_Prefix)))
                {
                    shouldSplitJsons.Add(newRelicSpanBatch.ToJson());
                }
                else
                {
                    successfulSpans.AddRange(newRelicSpanBatch.Spans);
                }
            });

            dataSender.WithHttpHandlerImpl((json) =>
            {
                var response = shouldSplitJsons.Contains(json)
                    ? new HttpResponseMessage(System.Net.HttpStatusCode.RequestEntityTooLarge)
                    : new HttpResponseMessage(System.Net.HttpStatusCode.OK);

                return Task.FromResult(response);
            });

            var spans = new List<NewRelicSpan>();
            spans.Add(new NewRelicSpan(null, $"{traceID_SplitBatch_Prefix}1", 0, null, null));
            spans.Add(new NewRelicSpan(null, $"{traceID_SplitBatch_Prefix}2", 0, null, null));
            spans.Add(new NewRelicSpan(null, $"{traceID_SplitBatch_Prefix}3", 0, null, null));
            spans.Add(new NewRelicSpan(null, traceID_Success, 0, null, null));

            // Act
            var result = await dataSender.SendDataAsync(spans);

            // Assert
            Assert.Equal(NewRelicResponseStatus.Failure, result.ResponseStatus);
            Assert.Equal(expectedCountCallsSendData, actualCountCallsSendData);
            Assert.Single(successfulSpans);
            Assert.Equal(traceID_Success, successfulSpans[0].Id);
        }

        [Fact]
        public async Task RetryBackoffSequence_RetriesExceeded()
        {
            var expectedNumSendBatchAsyncCall = 9; // 1 first call + 8 calls from retries
            var expectedBackoffSequenceFromTestRun = new List<uint>()
            {
                5000,
                10000,
                20000,
                40000,
                80000,
                80000,
                80000,
                80000,
            };
            var actualBackoffSequenceFromTestRun = new List<uint>();
            var actualCountCallsSendData = 0;

            var dataSender = new TraceDataSender(new TelemetryConfiguration() { ApiKey = "123456" }, null);

            dataSender.WithDelayFunction(async (uint milliSecondsDelay) =>
            {
                actualBackoffSequenceFromTestRun.Add(milliSecondsDelay);
                await Task.Delay(0);
                return;
            });

            dataSender.WithCaptureSendDataAsyncDelegate((newRelicSpanBatch, retryNum) =>
            {
                actualCountCallsSendData++;
            });

            dataSender.WithHttpHandlerImpl((json) =>
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.RequestTimeout));
            });

            var spans = new List<NewRelicSpan>()
            {
                new NewRelicSpan(
                    traceId: null,
                    spanId: "Test Span",
                    timestamp: 12345,
                    parentSpanId: null,
                    attributes: null),
            };

            var spanBatch = new NewRelicSpanBatch(spans);

            var result = await dataSender.SendDataAsync(spanBatch);

            Assert.Equal(NewRelicResponseStatus.Failure, result.ResponseStatus);
            Assert.Equal(HttpStatusCode.RequestTimeout, result.HttpStatusCode);
            Assert.Equal(expectedNumSendBatchAsyncCall, actualCountCallsSendData);
            Assert.Equal(expectedBackoffSequenceFromTestRun, actualBackoffSequenceFromTestRun);
        }

        [Fact]
        public async Task RetryBackoffSequence_IntermittentTimeoutEventuallySucceeds()
        {
            var expectedNumSendBatchAsyncCall = 4; // 1 first call + 3 calls from retries
            var expectedBackoffSequenceFromTestRun = new List<uint>()
            {
                5000,
                10000,
                20000,
            };

            var actualBackoffSequenceFromTestRun = new List<uint>();

            var dataSender = new TraceDataSender(new TelemetryConfiguration() { ApiKey = "123456" }, null);
            dataSender.WithDelayFunction(async (uint milliSecondsDelay) =>
            {
                actualBackoffSequenceFromTestRun.Add(milliSecondsDelay);
                await Task.Delay(0);
                return;
            });

            var actualCountCallsSendData = 0;
            dataSender.WithCaptureSendDataAsyncDelegate((newRelicSpanBatch, retryNum) =>
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

            var spans = new List<NewRelicSpan>()
            {
                new NewRelicSpan(
                    traceId: null,
                    spanId: "Test Span",
                    timestamp: 12345,
                    parentSpanId: null,
                    attributes: null),
            };

            var spanBatch = new NewRelicSpanBatch(spans);

            var result = await dataSender.SendDataAsync(spanBatch);

            Assert.Equal(NewRelicResponseStatus.Success, result.ResponseStatus);
            Assert.Equal(expectedNumSendBatchAsyncCall, actualCountCallsSendData);
            Assert.Equal(expectedBackoffSequenceFromTestRun, actualBackoffSequenceFromTestRun);
        }

        [Fact]
        public async Task RetryOn429_RetriesExceeded()
        {
            const int delayMS = 10000;
            const int expectedNumSendBatchAsyncCall = 9; // 1 first call + 3 calls from retries
            var expectedBackoffSequenceFromTestRun = new List<uint>()
            {
                delayMS,
                delayMS,
                delayMS,
                delayMS,
                delayMS,
                delayMS,
                delayMS,
                delayMS,
            };

            var actualBackoffSequenceFromTestRun = new List<uint>();

            var dataSender = new TraceDataSender(new TelemetryConfiguration() { ApiKey = "123456" }, default);
            dataSender.WithDelayFunction(async (uint milliSecondsDelay) =>
            {
                actualBackoffSequenceFromTestRun.Add(milliSecondsDelay);
                await Task.Delay(0);
                return;
            });

            var actualCountCallsSendData = 0;
            dataSender.WithCaptureSendDataAsyncDelegate((newRelicSpanBatch, retryNum) =>
            {
                actualCountCallsSendData++;
            });

            dataSender.WithHttpHandlerImpl((json) =>
            {
                var httpResponse = new HttpResponseMessage((HttpStatusCode)429);
                httpResponse.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromMilliseconds(delayMS));

                return Task.FromResult(httpResponse);
            });

            var spans = new List<NewRelicSpan>()
            {
                new NewRelicSpan(
                    traceId: null,
                    spanId: "Test Span",
                    timestamp: 12345,
                    parentSpanId: null,
                    attributes: null),
            };

            var spanBatch = new NewRelicSpanBatch(spans);

            var result = await dataSender.SendDataAsync(spanBatch);

            Assert.Equal(NewRelicResponseStatus.Failure, result.ResponseStatus);
            Assert.Equal(expectedNumSendBatchAsyncCall, actualCountCallsSendData);
            Assert.Equal(expectedBackoffSequenceFromTestRun, actualBackoffSequenceFromTestRun);
        }

        [Fact]
        public async Task RetryOn429WithDuration_429HappensOnce()
        {
            const int delayMS = 10000;
            const int expectedNumSendBatchAsyncCall = 2;
            var expectedBackoffSequenceFromTestRun = new List<uint>()
            {
                delayMS,
            };

            var actualBackoffSequenceFromTestRun = new List<uint>();

            var dataSender = new TraceDataSender(new TelemetryConfiguration() { ApiKey = "123456" }, null);

            dataSender.WithDelayFunction(async (uint milliSecondsDelay) =>
            {
                actualBackoffSequenceFromTestRun.Add(milliSecondsDelay);
                await Task.Delay(0);
                return;
            });

            var actualCountCallsSendData = 0;
            dataSender.WithCaptureSendDataAsyncDelegate((newRelicSpanBatch, retryNum) =>
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

            var spans = new List<NewRelicSpan>()
            {
                new NewRelicSpan(
                    traceId: null,
                    spanId: "Test Span",
                    timestamp: 12345,
                    parentSpanId: null,
                    attributes: null),
            };

            var spanBatch = new NewRelicSpanBatch(spans);

            var result = await dataSender.SendDataAsync(spanBatch);

            Assert.Equal(NewRelicResponseStatus.Success, result.ResponseStatus);
            Assert.Equal(expectedNumSendBatchAsyncCall, actualCountCallsSendData);
            Assert.Equal(expectedBackoffSequenceFromTestRun, actualBackoffSequenceFromTestRun);
        }

        [Fact]
        public async Task RetryOn429WithSpecificDate_429HappensOnce()
        {
            const int delayMs = 10000;

            // The actual retry delay will be slightly less than delayMs since UtcNow is recalculated in RetryWithServerDelay()
            var errorMargin = TimeSpan.FromMilliseconds(1000).TotalMilliseconds;
            var actualResponseFromTestRun = new List<Response>();

            uint actualDelayFromTestRun = 0;

            var dataSender = new TraceDataSender(new TelemetryConfiguration() { ApiKey = "123456", MaxRetryAttempts = 1 }, null);

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

            var spans = new List<NewRelicSpan>()
            {
                new NewRelicSpan(
                    traceId: null,
                    spanId: "Test Span",
                    timestamp: 12345,
                    parentSpanId: null,
                    attributes: null),
            };

            var spanBatch = new NewRelicSpanBatch(spans);

            var result = await dataSender.SendDataAsync(spanBatch);

            Assert.True(actualDelayFromTestRun >= delayMs - errorMargin && actualDelayFromTestRun <= delayMs + errorMargin, $"Expected delay: {delayMs}, margin: +/-{errorMargin}, actual delay: {actualDelayFromTestRun}");
        }

        [Fact]
        public async Task SendDataAsyncThrowsHttpException()
        {
            const int expectedNumSendBatchAsyncCall = 1;

            var dataSender = new TraceDataSender(new TelemetryConfiguration() { ApiKey = "123456" }, null);

            dataSender.WithDelayFunction(async (uint milliSecondsDelay) =>
            {
                await Task.Delay(0);
                return;
            });

            var actualCountCallsSendData = 0;
            dataSender.WithCaptureSendDataAsyncDelegate((newRelicSpanBatch, retryNum) =>
            {
                actualCountCallsSendData++;
            });

            dataSender.WithHttpHandlerImpl((json) =>
            {
                throw new Exception("Server Error", new Exception("Inner exception message"));
            });

            var spans = new List<NewRelicSpan>()
            {
                new NewRelicSpan(
                    traceId: null,
                    spanId: "Test Span",
                    timestamp: 12345,
                    parentSpanId: null,
                    attributes: null),
            };

            var spanBatch = new NewRelicSpanBatch(spans);

            var result = await dataSender.SendDataAsync(spanBatch);

            Assert.Equal(NewRelicResponseStatus.Failure, result.ResponseStatus);
            Assert.Equal("Inner exception message", result.Message);
            Assert.Null(result.HttpStatusCode);
            Assert.Equal(expectedNumSendBatchAsyncCall, actualCountCallsSendData);
        }

        [Fact]
        public async Task SendDataAsyncThrowsNonHttpException()
        {
            const int expectedNumSendBatchAsyncCall = 1;
            const int expectedNumHttpHandlerCall = 0;

            var dataSender = new TraceDataSender(new TelemetryConfiguration() { ApiKey = "123456" }, null);

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

            var spans = new List<NewRelicSpan>()
            {
                new NewRelicSpan(
                    traceId: null,
                    spanId: "Test Span",
                    timestamp: 12345,
                    parentSpanId: null,
                    attributes: null),
            };

            var spanBatch = new NewRelicSpanBatch(spans);

            var result = await dataSender.SendDataAsync(spanBatch);

            Assert.Equal(NewRelicResponseStatus.Failure, result.ResponseStatus);
            Assert.Equal("Test Exception", result.Message);
            Assert.Null(result.HttpStatusCode);

            Assert.Equal(expectedNumSendBatchAsyncCall, actualCountCallsSendData);
            Assert.Equal(expectedNumHttpHandlerCall, actualCallsHttpHandler);
        }

        [Theory]
        [InlineData(null, "1.0.0")]
        [InlineData("productName", null)]
        [InlineData("", "")]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData(null, "")]
        [InlineData("productName", "1.0.0")]
        public void AddVersionInfo(string productName, string productVersion)
        {
            var dataSender = new TraceDataSender(new TelemetryConfiguration() { ApiKey = "123456" }, null);

            var expectedUserAgentValue = dataSender.UserAgent;

            if (!string.IsNullOrEmpty(productName) && !string.IsNullOrEmpty(productVersion))
            {
                expectedUserAgentValue += " " + $@"{productName}/{productVersion}";
            }

            dataSender.AddVersionInfo(productName, productVersion);

            var userAgentValueAfter = dataSender.UserAgent;

            Assert.Equal(expectedUserAgentValue, userAgentValueAfter);
        }
    }
}
