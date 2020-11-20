// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NewRelic.Telemetry;
using NewRelic.Telemetry.Tracing;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Xunit;

namespace NewRelic.OpenTelemetry.Tests
{
    [Collection("newrelic-exporter")]
    public class SpanConverterTests : IDisposable
    {
        private const string TestServiceName = "TestService";
        private const string ErrorMessage = "This is a test error description";
        private const string SampleOkMessage = "This is a test Ok description";
        private const string AttributeValueToIgnore = "IgnoreMe";

        private const int ExpectedCountSpans = 7;
        private const string AttrNameParentID = "parent.Id";

        private NewRelicExporterOptions _options;
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
         *  6   Test Span 5                                     Trace 4     Included
        */
        private static DateTimeOffset _traceStartTime = DateTime.UtcNow;
        private readonly (int? Parent, string Name, DateTimeOffset Start, DateTimeOffset End, Status? Status, bool IsCallToNewRelic, bool IncludeTags)[] _spanDefinitions = new (int?, string, DateTimeOffset, DateTimeOffset, Status?, bool, bool)[]
        {
            (null, "Test Span 1", _traceStartTime, _traceStartTime.AddMilliseconds(225), Status.Unset, false, true),
            (0, "Test Span 2", _traceStartTime.AddMilliseconds(1), _traceStartTime.AddMilliseconds(100), Status.Error.WithDescription(ErrorMessage), false, false),
            (null, "Test Span 3", _traceStartTime.AddMilliseconds(2), _traceStartTime.AddMilliseconds(375), Status.Ok, false, false),
            (null, "Test Span 4", _traceStartTime.AddMilliseconds(3), _traceStartTime.AddMilliseconds(650), Status.Ok.WithDescription(SampleOkMessage), false, false),
            (3, "Should Be Filtered - HTTP Call to NR", _traceStartTime.AddMilliseconds(4), _traceStartTime.AddMilliseconds(600), Status.Ok, true, false),
            (4, "Should Be Filtered - Child of HTTP", _traceStartTime.AddMilliseconds(5), _traceStartTime.AddMilliseconds(500), Status.Ok, false, false),
            (null, "Test Span 5", _traceStartTime.AddMilliseconds(6), _traceStartTime.AddMilliseconds(750), null, false, false),
        };

        public SpanConverterTests()
        {
            _options = new NewRelicExporterOptions()
            {
                ApiKey = "12345",
            };
            var mockDataSender = new TraceDataSender(_options.TelemetryConfiguration, null);

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

            var exporter = new NewRelicTraceExporter(mockDataSender, _options, null);
            var source = new ActivitySource("newrelic.test");

            using (var openTelemetrySdk = Sdk.CreateTracerProviderBuilder()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(TestServiceName))
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
                        activity.AddTag("http.url", _options.EndpointUrl);
                    }

                    activity.SetEndTime(spanDefinition.End.UtcDateTime);

                    if (spanDefinition.Status.HasValue)
                    {
                        activity.SetStatus(spanDefinition.Status.Value);
                    }

                    if (spanDefinition.IncludeTags)
                    {
                        activity.SetTag("foo", "bar");
                        activity.SetTag(NewRelicConsts.Tracing.AttribNameDurationMs, AttributeValueToIgnore);
                        activity.SetTag(NewRelicConsts.Tracing.AttribNameName, AttributeValueToIgnore);
                        activity.SetTag(NewRelicConsts.Tracing.AttribNameErrorMsg, AttributeValueToIgnore);
                        activity.SetTag(NewRelicConsts.Tracing.AttribSpanKind, AttributeValueToIgnore);
                        activity.SetTag(NewRelicConsts.Tracing.AttribNameParentId, AttributeValueToIgnore);
                        activity.SetTag(NewRelicConsts.AttributeInstrumentationName, AttributeValueToIgnore);
                        activity.SetTag(NewRelicConsts.AttributeInstrumentationVersion, AttributeValueToIgnore);
                    }

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
            Assert.True(ResultNRSpansDic.ContainsKey(_otSpans[6].Context.SpanId.ToHexString()));
        }

        [Fact]
        public void Test_ErrorMessageAttribute()
        {
            var resultSpan0 = ResultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultSpan1 = ResultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultSpan2 = ResultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultSpan3 = ResultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];
            var resultSpan6 = ResultNRSpansDic[_otSpans[6].Context.SpanId.ToHexString()];

            Assert.False(resultSpan0.Attributes?.ContainsKey("error.message"));
            Assert.Equal(ErrorMessage, resultSpan1.Attributes?["error.message"]);
            Assert.False(resultSpan2.Attributes?.ContainsKey("error.message"));
            Assert.False(resultSpan3.Attributes?.ContainsKey("error.message"));
            Assert.False(resultSpan6.Attributes?.ContainsKey("error.message"));
        }

        [Fact]
        public void Test_TraceId()
        {
            var resultNRSpan0 = ResultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultNRSpan1 = ResultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultNRSpan2 = ResultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultNRSpan3 = ResultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];
            var resultNRSpan6 = ResultNRSpansDic[_otSpans[6].Context.SpanId.ToHexString()];

            Assert.Equal(resultNRSpan0.TraceId, _otSpans[0].Context.TraceId.ToHexString());
            Assert.Equal(resultNRSpan1.TraceId, _otSpans[1].Context.TraceId.ToHexString());
            Assert.Equal(resultNRSpan2.TraceId, _otSpans[2].Context.TraceId.ToHexString());
            Assert.Equal(resultNRSpan6.TraceId, _otSpans[6].Context.TraceId.ToHexString());
            Assert.Equal(resultNRSpan0.TraceId, resultNRSpan1.TraceId);
            Assert.NotEqual(resultNRSpan0.TraceId, resultNRSpan2.TraceId);
            Assert.NotEqual(resultNRSpan0.TraceId, resultNRSpan3.TraceId);
            Assert.NotEqual(resultNRSpan2.TraceId, resultNRSpan3.TraceId);
            Assert.NotEqual(resultNRSpan6.TraceId, resultNRSpan3.TraceId);
            Assert.NotEqual(resultNRSpan6.TraceId, resultNRSpan2.TraceId);
            Assert.NotEqual(resultNRSpan6.TraceId, resultNRSpan0.TraceId);
        }

        [Fact]
        public void Test_ParentSpanId()
        {
            var resultNRSpan0 = ResultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultNRSpan1 = ResultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultNRSpan2 = ResultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultNRSpan3 = ResultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];
            var resultNRSpan6 = ResultNRSpansDic[_otSpans[6].Context.SpanId.ToHexString()];

            Assert.False(resultNRSpan0.Attributes?.ContainsKey(AttrNameParentID));
            Assert.Equal(resultNRSpan1.Attributes?[NewRelicConsts.Tracing.AttribNameParentId], resultNRSpan0.Id);
            Assert.False(resultNRSpan2.Attributes?.ContainsKey(AttrNameParentID));
            Assert.False(resultNRSpan3.Attributes?.ContainsKey(AttrNameParentID));
            Assert.False(resultNRSpan6.Attributes?.ContainsKey(AttrNameParentID));
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
        public void Test_CommonProperties()
        {
            Assert.Equal(_otSpans.Count, _resultNRSpans.Count);
            Assert.NotNull(_resultNRSpanBatch?.CommonProperties.Attributes);
            Assert.Equal("newrelic-opentelemetry-exporter", _resultNRSpanBatch?.CommonProperties.Attributes["collector.name"]);
            Assert.Equal("opentelemetry", _resultNRSpanBatch?.CommonProperties.Attributes["instrumentation.provider"]);
        }

        [Fact]
        public void Test_Status()
        {
            const string statusCodeAttributeName = "otel.status_code";
            const string statusDescriptionAttributeName = "otel.status_description";

            var resultNRSpan0 = ResultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultNRSpan1 = ResultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultNRSpan2 = ResultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultNRSpan3 = ResultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];
            var resultNRSpan6 = ResultNRSpansDic[_otSpans[6].Context.SpanId.ToHexString()];

            Assert.False(resultNRSpan0.Attributes?.ContainsKey(statusCodeAttributeName));
            Assert.False(resultNRSpan0.Attributes?.ContainsKey(statusDescriptionAttributeName));
            Assert.Equal("Error", resultNRSpan1.Attributes?[statusCodeAttributeName]);
            Assert.Equal(ErrorMessage, resultNRSpan1.Attributes?[statusDescriptionAttributeName]);
            Assert.Equal("Ok", resultNRSpan2.Attributes?[statusCodeAttributeName]);
            Assert.False(resultNRSpan2.Attributes?.ContainsKey(statusDescriptionAttributeName));
            Assert.Equal("Ok", resultNRSpan3.Attributes?[statusCodeAttributeName]);
            Assert.Equal(SampleOkMessage, resultNRSpan3.Attributes?[statusDescriptionAttributeName]);
            Assert.False(resultNRSpan6.Attributes?.ContainsKey(statusCodeAttributeName));
            Assert.False(resultNRSpan6.Attributes?.ContainsKey(statusDescriptionAttributeName));
        }

        [Fact]
        public void Test_Tags()
        {
            var resultNRSpan0 = ResultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var allAttributeNames = resultNRSpan0.Attributes?.Keys ?? Enumerable.Empty<string>();

            Assert.Equal("bar", resultNRSpan0.Attributes?["foo"]);

            foreach (var attributeName in allAttributeNames)
            {
                Assert.False(resultNRSpan0.Attributes?[attributeName].Equals(AttributeValueToIgnore), $"Span attribute {attributeName} contained an unexpected value of '{resultNRSpan0.Attributes?[attributeName]}'.");
            }
        }
    }
}
