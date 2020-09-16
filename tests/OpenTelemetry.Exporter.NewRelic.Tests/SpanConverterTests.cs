// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NewRelic.Telemetry;
using NewRelic.Telemetry.Spans;
using OpenTelemetry.Trace;
using Xunit;

namespace OpenTelemetry.Exporter.NewRelic.Tests
{
    [Collection("newrelic-exporter")]
    public class SpanConverterTests : IDisposable
    {
        const string testServiceName = "TestService";
        const string errorMessage = "This is a test error description";

        const int expected_CountSpans = 4;

        private List<Activity> _otSpans = new List<Activity>();
        private List<Span> _resultNRSpans = new List<Span>();

        private Dictionary<string, Span> resultNRSpansDic => _resultNRSpans.ToDictionary(x => x.Id);

        //  Creating the following spans                        Trace       Expected Outcome
        //  -----------------------------------------------------------------------------------
        //  0   Test Span 1                                     Trace 1     Included
        //  1       Test Span 2                                 Trace 1     Included
        //  2   Test Span 3                                     Trace 2     Included
        //  3   Test Span 4                                     Trace 3     Included
        //  4       Should be Filtered - HTTP Call to NR        Trace 3     Excluded
        //  5           Should be filtered - Child of HTTP      Trace 3     Excluded

        private static DateTimeOffset _traceStartTime = DateTime.UtcNow;
        private (int? Parent, string Name, DateTimeOffset Start, DateTimeOffset End, Status Status, bool IsCallToNewRelic)[] _spanDefinitions = new (int?, string, DateTimeOffset, DateTimeOffset, Status, bool)[]
        {
            (null, "Test Span 1", _traceStartTime, _traceStartTime.AddMilliseconds(225), Status.Ok, false ),
            (0, "Test Span 2", _traceStartTime.AddMilliseconds(1), _traceStartTime.AddMilliseconds(100), Status.Aborted.WithDescription(errorMessage), false ),
            (null, "Test Span 3", _traceStartTime.AddMilliseconds(2), _traceStartTime.AddMilliseconds(375), Status.Ok, false ),
            (null, "Test Span 4", _traceStartTime.AddMilliseconds(3), _traceStartTime.AddMilliseconds(650), Status.Ok, false ),
            (3, "Should Be Filtered - HTTP Call to NR", _traceStartTime.AddMilliseconds(4), _traceStartTime.AddMilliseconds(600), Status.Ok, true ),
            (4, "Should Be Filtered - Child of HTTP", _traceStartTime.AddMilliseconds(5), _traceStartTime.AddMilliseconds(500), Status.Ok, false ),
        };

        public SpanConverterTests()
        {
            var config = new TelemetryConfiguration().WithApiKey("123456").WithServiceName(testServiceName);
            var mockDataSender = new SpanDataSender(config);

            //Capture the spans that were requested to be sent to New Relic.
            mockDataSender.WithCaptureSendDataAsyncDelegate((sb, retryId) =>
            {
                _resultNRSpans.AddRange(sb.Spans);
            });

            //Prevent actually sending those spans to New Relic.
            mockDataSender.WithHttpHandlerImpl((json) =>
            {
                return Task.FromResult(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK));
            });

            var exporter = new NewRelicTraceExporter(mockDataSender, config, null);
            var source = new ActivitySource("newrelic.test");

            using (var openTelemetrySdk = Sdk.CreateTracerProviderBuilder()
                    .AddSource("newrelic.test")
                    .AddProcessor(new BatchExportActivityProcessor(exporter))
                    .Build())
            {
                var tracer = openTelemetrySdk.GetTracer("TestTracer");

                for (var i = 0; i < _spanDefinitions.Length; ++i)
                {
                    var spanDefinition = _spanDefinitions[i];
                    var parentContext = spanDefinition.Parent.HasValue ? _otSpans[spanDefinition.Parent.Value].Context : default(ActivityContext);
                    var activity = source.StartActivity(spanDefinition.Name, ActivityKind.Server, parentContext);

                    activity.SetStartTime(spanDefinition.Start.UtcDateTime);
                    if (spanDefinition.IsCallToNewRelic)
                    {
                        activity.AddTag("http.url", config.TraceUrl);
                    }
                    activity.SetEndTime(spanDefinition.End.UtcDateTime);
                    activity.SetStatus(spanDefinition.Status);
                    activity?.Stop();

                    _otSpans.Add(activity);
                }
            }
        }

        public void Dispose()
        {
            _resultNRSpans.Clear();
            _otSpans.Clear();
        }

        [Fact]
        public void Test_ExpectedSpansCreated()
        {
            Assert.Equal(expected_CountSpans, resultNRSpansDic.Count);

            var resultNRSpan0 = resultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultNRSpan1 = resultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultNRSpan2 = resultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultNRSpan3 = resultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];

            Assert.NotNull(resultNRSpan0);
            Assert.NotNull(resultNRSpan1);
            Assert.NotNull(resultNRSpan2);
            Assert.NotNull(resultNRSpan3);
        }

        [Fact]
        public void Test_ErrorAttribute()
        {
            var resultSpan0 = resultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultSpan1 = resultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultSpan2 = resultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultSpan3 = resultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];

            Assert.False(resultSpan0.Attributes.ContainsKey("error"));
            Assert.True((bool)resultSpan1.Attributes["error"]);
            Assert.Equal(errorMessage, resultSpan1.Attributes["error.message"]);
            Assert.False(resultSpan2.Attributes.ContainsKey("error"));
            Assert.False(resultSpan3.Attributes.ContainsKey("error"));
        }

        [Fact]
        public void Test_TraceId()
        {
            var resultNRSpan0 = resultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultNRSpan1 = resultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultNRSpan2 = resultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultNRSpan3 = resultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];

            Assert.Equal(resultNRSpan0.TraceId, _otSpans[0].Context.TraceId.ToHexString());
            Assert.Equal(resultNRSpan1.TraceId, _otSpans[1].Context.TraceId.ToHexString());
            Assert.Equal(resultNRSpan2.TraceId, _otSpans[2].Context.TraceId.ToHexString());
            Assert.Equal(resultNRSpan0.TraceId, resultNRSpan1.TraceId);
            Assert.NotEqual(resultNRSpan0.TraceId, resultNRSpan2.TraceId);
            Assert.NotEqual(resultNRSpan0.TraceId, resultNRSpan3.TraceId);
            Assert.NotEqual(resultNRSpan2.TraceId, resultNRSpan3.TraceId);

        }

        [Fact]
        public void Test_ParentSpanId()
        {
            var resultNRSpan0 = resultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultNRSpan1 = resultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultNRSpan2 = resultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultNRSpan3 = resultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];

            Assert.Null(resultNRSpan0.ParentId);
            Assert.Equal(resultNRSpan1.ParentId, resultNRSpan0.Id);
            Assert.Null(resultNRSpan2.ParentId);
            Assert.Null(resultNRSpan3.ParentId);
        }

        [Fact]
        public void Test_FilterOutNewRelicEndpoint()
        {
            Assert.False(resultNRSpansDic.ContainsKey(_otSpans[4].Context.SpanId.ToHexString()));
            Assert.False(resultNRSpansDic.ContainsKey(_otSpans[5].Context.SpanId.ToHexString()));
        }

        [Fact]
        public void Test_Timestamps()
        {
            for (var i = 0; i < _otSpans.Count; ++i)
            {
                if (!resultNRSpansDic.TryGetValue(_otSpans[i].Context.SpanId.ToHexString(), out var nrSpan))
                {
                    continue;
                }

                var expectedStartTimestampUnixMs = _spanDefinitions[i].Start.ToUnixTimeMilliseconds();
                var expectedEndTimestampUnixMs = _spanDefinitions[i].End.ToUnixTimeMilliseconds();
                var expectedDurationMs = expectedEndTimestampUnixMs - expectedStartTimestampUnixMs;

                Assert.Equal(expectedStartTimestampUnixMs, nrSpan.Timestamp);
                Assert.Equal((double)expectedDurationMs, nrSpan.Attributes["duration.ms"]);
            }
        }

        [Fact]
        public void Test_InstrumentationProvider()
        {
            foreach (var otSpan in _otSpans)
            {
                if (!resultNRSpansDic.TryGetValue(otSpan.Context.SpanId.ToHexString(), out var nrSpan))
                {
                    continue;
                }

                Assert.NotNull(nrSpan.Attributes);
                Assert.True(nrSpan.Attributes.ContainsKey("instrumentation.provider"));
                Assert.Equal("opentelemetry", nrSpan.Attributes["instrumentation.provider"]);

            }

        }
    }
}
