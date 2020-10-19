// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NewRelic.Telemetry;
using NewRelic.Telemetry.Tracing;
using OpenTelemetry.Trace;
using Xunit;

namespace OpenTelemetry.Exporter.NewRelic.Tests
{
    [Collection("newrelic-exporter")]
    public class SpanConverterTests : IDisposable
    {
        private const string TestServiceName = "TestService";
        private const string ErrorMessage = "This is a test error description";

        private const int ExpectedCountSpans = 6;
        private const string AttrNameParentID = "parent.Id";

        private TelemetryConfiguration _config;
        private List<Activity> _otSpans = new List<Activity>();
        private List<NewRelicSpan> _resultNRSpans = new List<NewRelicSpan>();
        private NewRelicSpanBatch? _resultNRSpanBatch;

        private Dictionary<string, NewRelicSpan> ResultNRSpansDic => _resultNRSpans.ToDictionary(x => x.Id);

        /*
         *  Creating the following spans                        Trace       Expected Outcome
         *  -----------------------------------------------------------------------------------
         *  0   Test Span 1                                     Trace 1     Included
         *  1       Test Span 2                                 Trace 1     Included
         *  2   Test Span 3                                     Trace 2     Included
         *  3   Test Span 4                                     Trace 3     Included
         *  4       Should be Filtered - HTTP Call to NR        Trace 3     Excluded
         *  5           Should be filtered - Child of HTTP      Trace 3     Excluded
        */
        private static DateTimeOffset _traceStartTime = DateTime.UtcNow;
        private readonly (int? Parent, string Name, DateTimeOffset Start, DateTimeOffset End, Status Status, bool IsCallToNewRelic)[] _spanDefinitions = new (int?, string, DateTimeOffset, DateTimeOffset, Status, bool)[]
        {
            (null, "Test Span 1", _traceStartTime, _traceStartTime.AddMilliseconds(225), Status.Ok, false),
            (0, "Test Span 2", _traceStartTime.AddMilliseconds(1), _traceStartTime.AddMilliseconds(100), Status.Error.WithDescription(ErrorMessage), false),
            (null, "Test Span 3", _traceStartTime.AddMilliseconds(2), _traceStartTime.AddMilliseconds(375), Status.Ok, false),
            (null, "Test Span 4", _traceStartTime.AddMilliseconds(3), _traceStartTime.AddMilliseconds(650), Status.Ok, false),
            (3, "Should Be Filtered - HTTP Call to NR", _traceStartTime.AddMilliseconds(4), _traceStartTime.AddMilliseconds(600), Status.Ok, true),
            (4, "Should Be Filtered - Child of HTTP", _traceStartTime.AddMilliseconds(5), _traceStartTime.AddMilliseconds(500), Status.Ok, false),
        };

        public SpanConverterTests()
        {
            _config = new TelemetryConfiguration()
            {
                ApiKey = "12345",
                ServiceName = TestServiceName,
            };
            var mockDataSender = new TraceDataSender(_config, null);

            // Capture the spans that were requested to be sent to New Relic.
            mockDataSender.WithCaptureSendDataAsyncDelegate((sb, retryId) =>
            {
                if (sb.Spans == null)
                {
                    return;
                }

                _resultNRSpanBatch = sb;
                _resultNRSpans.AddRange(sb.Spans);
            });

            // Prevent actually sending those spans to New Relic.
            mockDataSender.WithHttpHandlerImpl((json) =>
            {
                return Task.FromResult(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK));
            });

            var exporter = new NewRelicTraceExporter(mockDataSender, _config, null);
            var source = new ActivitySource("newrelic.test");

            using (var openTelemetrySdk = Sdk.CreateTracerProviderBuilder()
                    .AddSource("newrelic.test")
                    .AddProcessor(new BatchExportProcessor<Activity>(exporter))
                    .Build())
            {
                var tracer = openTelemetrySdk.GetTracer("TestTracer");

                for (var i = 0; i < _spanDefinitions.Length; ++i)
                {
                    var spanDefinition = _spanDefinitions[i];
                    var parentContext = spanDefinition.Parent.HasValue ? _otSpans[spanDefinition.Parent.Value].Context : default(ActivityContext);
                    var activity = source.StartActivity(spanDefinition.Name, ActivityKind.Server, parentContext);

                    if (activity == null)
                    {
                        continue;
                    }

                    activity.SetStartTime(spanDefinition.Start.UtcDateTime);
                    if (spanDefinition.IsCallToNewRelic)
                    {
                        activity.AddTag("http.url", _config.TraceUrl);
                    }

                    activity.SetEndTime(spanDefinition.End.UtcDateTime);
                    activity.SetStatus(spanDefinition.Status);
                    activity.Stop();

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
            Assert.Equal(ExpectedCountSpans, ResultNRSpansDic.Count);
            Assert.True(ResultNRSpansDic.ContainsKey(_otSpans[0].Context.SpanId.ToHexString()));
            Assert.True(ResultNRSpansDic.ContainsKey(_otSpans[1].Context.SpanId.ToHexString()));
            Assert.True(ResultNRSpansDic.ContainsKey(_otSpans[2].Context.SpanId.ToHexString()));
            Assert.True(ResultNRSpansDic.ContainsKey(_otSpans[3].Context.SpanId.ToHexString()));
        }

        [Fact]
        public void Test_ErrorAttribute()
        {
            var resultSpan0 = ResultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultSpan1 = ResultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultSpan2 = ResultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultSpan3 = ResultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];

            Assert.False(resultSpan0.Attributes?.ContainsKey("error"));
            Assert.True((bool?)resultSpan1.Attributes?["error"]);
            Assert.Equal(ErrorMessage, resultSpan1.Attributes?["error.message"]);
            Assert.False(resultSpan2.Attributes?.ContainsKey("error"));
            Assert.False(resultSpan3.Attributes?.ContainsKey("error"));
        }

        [Fact]
        public void Test_TraceId()
        {
            var resultNRSpan0 = ResultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultNRSpan1 = ResultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultNRSpan2 = ResultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultNRSpan3 = ResultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];

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
            var resultNRSpan0 = ResultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultNRSpan1 = ResultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultNRSpan2 = ResultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultNRSpan3 = ResultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];

            Assert.False(resultNRSpan0.Attributes?.ContainsKey(AttrNameParentID));
            Assert.Equal(resultNRSpan1.Attributes?[NewRelicConsts.Tracing.AttribNameParentId], resultNRSpan0.Id);
            Assert.False(resultNRSpan2.Attributes?.ContainsKey(AttrNameParentID));
            Assert.False(resultNRSpan3.Attributes?.ContainsKey(AttrNameParentID));
        }

        [Fact]
        public void Test_Timestamps()
        {
            for (var i = 0; i < _otSpans.Count; ++i)
            {
                if (!ResultNRSpansDic.TryGetValue(_otSpans[i].Context.SpanId.ToHexString(), out var nrSpan))
                {
                    continue;
                }

                var expectedStartTimestampUnixMs = _spanDefinitions[i].Start.ToUnixTimeMilliseconds();
                var expectedEndTimestampUnixMs = _spanDefinitions[i].End.ToUnixTimeMilliseconds();
                var expectedDurationMs = expectedEndTimestampUnixMs - expectedStartTimestampUnixMs;

                Assert.Equal(expectedStartTimestampUnixMs, nrSpan.Timestamp);
                Assert.Equal((double)expectedDurationMs, nrSpan.Attributes?["duration.ms"]);
            }
        }

        [Fact]
        public void Test_InstrumentationProvider()
        {
            Assert.Equal(_otSpans.Count, _resultNRSpans.Count);
            Assert.NotNull(_resultNRSpanBatch?.CommonProperties.Attributes);
            Assert.True(_resultNRSpanBatch?.CommonProperties.Attributes.ContainsKey("instrumentation.provider"));
            Assert.Equal(_config.InstrumentationProvider, _resultNRSpanBatch?.CommonProperties.Attributes["instrumentation.provider"]);
        }
    }
}
