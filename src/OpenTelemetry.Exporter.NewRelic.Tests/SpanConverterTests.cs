using NUnit.Framework;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewRelic.Telemetry;
using NewRelic.Telemetry.Spans;
using System;

namespace OpenTelemetry.Exporter.NewRelic.Tests
{
    public class SpanConverterTests
    {
        const string testServiceName = "TestService";
        const string errorMessage = "This is a test error description";

        const int expected_CountSpans = 4;

        private List<TelemetrySpan> _otSpans = new List<TelemetrySpan>();
        private List<Span> _resultNRSpans = new List<Span>();

        private Dictionary<string, Span> resultNRSpansDic => _resultNRSpans.ToDictionary(x => x.Id);

        //  Creating the following spans                        Trace       Expected Outcome
        //  -----------------------------------------------------------------------------------
        //  0   Test Span 1                                     Trace 1     Included
        //  1       Test Span 2                                 Trace 1     Included
        //  2   Test Span 3                                     Trace 2     Included
        //  3   Test Span 4                                     Trace 3     Included
        //  4       Shoul be Filtered - HTTP Call to NR         Trace 3     Excluded
        //  5           Should be filtered - Child of HTTP      Trace 3     Excluded

        private static DateTimeOffset _traceStartTime = DateTime.UtcNow;
        private (int? Parent, string Name, DateTimeOffset Start, DateTimeOffset End, Status Status, bool IsCallToNewRelic)[] spanDefinitions = new (int?, string, DateTimeOffset, DateTimeOffset, Status, bool)[]
        {
            (null, "Test Span 1", _traceStartTime, _traceStartTime.AddMilliseconds(225), Status.Ok, false ),
            (0, "Test Span 2", _traceStartTime.AddMilliseconds(1), _traceStartTime.AddMilliseconds(100), Status.Aborted.WithDescription(errorMessage), false ),
            (null, "Test Span 3", _traceStartTime.AddMilliseconds(2), _traceStartTime.AddMilliseconds(375), Status.Ok, false ),
            (null, "Test Span 4", _traceStartTime.AddMilliseconds(3), _traceStartTime.AddMilliseconds(650), Status.Ok, false ),
            (3, "Should Be Filtered - HTTP Call to NR", _traceStartTime.AddMilliseconds(4), _traceStartTime.AddMilliseconds(600), Status.Ok, true ),
            (4, "Should Be Filtered - Child of HTTP", _traceStartTime.AddMilliseconds(5), _traceStartTime.AddMilliseconds(500), Status.Ok, false ),
        };

        [SetUp]
        public void Setup()
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

            using (var tracerFactory = TracerFactory.Create(
                                  builder => builder.AddProcessorPipeline(
                                        c => c.SetExporter(exporter))))
            {
                var tracer = tracerFactory.GetTracer("TestTracer");

                for (var i = 0; i < spanDefinitions.Length; ++i)
                {
                    var spanDefinition = spanDefinitions[i];
                    var span = !spanDefinition.Parent.HasValue
                        ? tracer.StartRootSpan(spanDefinition.Name, SpanKind.Server, new SpanCreationOptions { StartTimestamp = spanDefinition.Start })
                        : tracer.StartSpan(spanDefinition.Name, _otSpans[spanDefinition.Parent.Value], SpanKind.Server, new SpanCreationOptions { StartTimestamp = spanDefinition.Start });
                    if (spanDefinition.IsCallToNewRelic)
                    {
                        span.PutHttpRawUrlAttribute(config.TraceUrl);
                    }
                    span.Status = spanDefinition.Status;
                    span.End(spanDefinition.End);
                    _otSpans.Add(span);
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            _resultNRSpans.Clear();
            _otSpans.Clear();
        }

        [Test]
        public void Test_ExpectedSpansCreated()
        {
            Assert.AreEqual(expected_CountSpans, resultNRSpansDic.Count, "Unexpected number of spans");

            var resultNRSpan0 = resultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultNRSpan1 = resultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultNRSpan2 = resultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultNRSpan3 = resultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];

            Assert.IsNotNull(resultNRSpan0, $"Test Span not in output - testSpan0 - {_otSpans[0].Context.SpanId.ToHexString()}");
            Assert.IsNotNull(resultNRSpan1, $"Test Span not in output - testSpan1 - {_otSpans[1].Context.SpanId.ToHexString()}");
            Assert.IsNotNull(resultNRSpan2, $"Test Span not in output - testSpan2 - {_otSpans[2].Context.SpanId.ToHexString()}");
            Assert.IsNotNull(resultNRSpan3, $"Test Span not in output - testSpan2 - {_otSpans[2].Context.SpanId.ToHexString()}");
        }

        [Test]
        public void Test_ErrorAttribute()
        {
            var resultSpan0 = resultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultSpan1 = resultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultSpan2 = resultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultSpan3 = resultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];

            Assert.False(resultSpan0.Attributes.ContainsKey("error"), "resultSpan0 should NOT have Error Attribute");
            Assert.AreEqual(resultSpan1.Attributes["error"], true, "resultSpan1 should have Error Attribute = true");
            Assert.AreEqual(resultSpan1.Attributes["error.message"], errorMessage, $"resultSpan1 should have Error.Message Attribute = '{errorMessage}'");
            Assert.False(resultSpan2.Attributes.ContainsKey("error"), "resultSpan2 should NOT have Error Attribute");
            Assert.False(resultSpan3.Attributes.ContainsKey("error"), "resultSpan3 should NOT have Error Attribute");
        }

        [Test]
        public void Test_TraceId()
        {
            var resultNRSpan0 = resultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultNRSpan1 = resultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultNRSpan2 = resultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultNRSpan3 = resultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];

            Assert.AreEqual(resultNRSpan0.TraceId, _otSpans[0].Context.TraceId.ToHexString(), "Mismatch on TraceId - testSpan0");
            Assert.AreEqual(resultNRSpan1.TraceId, _otSpans[1].Context.TraceId.ToHexString(), "Mismatch on TraceId - testSpan1");
            Assert.AreEqual(resultNRSpan2.TraceId, _otSpans[2].Context.TraceId.ToHexString(), "Mismatch on TraceId - testSpan2");
            Assert.AreEqual(resultNRSpan0.TraceId, resultNRSpan1.TraceId, "Mismatch on TraceId - testSpan0 and testSpan1 should belong to the same trace");
            Assert.AreNotEqual(resultNRSpan0.TraceId, resultNRSpan2.TraceId, "Mismatch on TraceId - testSpan0 and testSpan2 should NOT belong to the same trace");
            Assert.AreNotEqual(resultNRSpan0.TraceId, resultNRSpan3.TraceId, "Mismatch on TraceId - testSpan0 and testSpan3 should NOT belong to the same trace");
            Assert.AreNotEqual(resultNRSpan2.TraceId, resultNRSpan3.TraceId, "Mismatch on TraceId - testSpan2 and testSpan3 should NOT belong to the same trace");

        }

        public void Test_ParentSpanId()
        {
            var resultNRSpan0 = resultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultNRSpan1 = resultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultNRSpan2 = resultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];
            var resultNRSpan3 = resultNRSpansDic[_otSpans[3].Context.SpanId.ToHexString()];

            Assert.IsNull(resultNRSpan0.ParentId, "Top Level Span should have NULL parentID");
            Assert.AreEqual(resultNRSpan1.ParentId, resultNRSpan0.Id, "Mismatch on ParentId - Span1 is a child of Span0");
            Assert.IsNull(resultNRSpan2.ParentId, "Top Level Span should have NULL parentID");
            Assert.IsNull(resultNRSpan3.ParentId, "Top Level Span should have NULL parentID");
        }

        [Test]
        public void Test_FilterOutNewRelicEndpoint()
        {
            //Assert.IsFalse(resultNRSpansDic.ContainsKey(_otSpans[3].Context.SpanId.ToHexString()), "Endpoint calls to New Relic should be excluded");
            Assert.IsFalse(resultNRSpansDic.ContainsKey(_otSpans[4].Context.SpanId.ToHexString()), "Endpoint calls to New Relic should be excluded");
            Assert.IsFalse(resultNRSpansDic.ContainsKey(_otSpans[5].Context.SpanId.ToHexString()), "Endpoint calls to New Relic should be excluded");
        }

        [Test]
        public void Test_Timestamps()
        {
            for (var i = 0; i < _otSpans.Count; ++i)
            {
                if (!resultNRSpansDic.TryGetValue(_otSpans[i].Context.SpanId.ToHexString(), out var nrSpan))
                {
                    continue;
                }

                var expectedStartTimestampUnixMs = spanDefinitions[i].Start.ToUnixTimeMilliseconds();
                var expectedEndTimestampUnixMs = spanDefinitions[i].End.ToUnixTimeMilliseconds();
                var expectedDurationMs = expectedEndTimestampUnixMs - expectedStartTimestampUnixMs;

                Assert.AreEqual(expectedStartTimestampUnixMs, nrSpan.Timestamp, $"{spanDefinitions[i].Name} - Open Telemetry StartTime should translate to {expectedStartTimestampUnixMs}");
                Assert.AreEqual(expectedDurationMs, nrSpan.Attributes["duration.ms"]);
            }
        }

        [Test]
        public void Test_InstrumentationProvider()
        {
            foreach (var otSpan in _otSpans)
            {
                if (!resultNRSpansDic.TryGetValue(otSpan.Context.SpanId.ToHexString(), out var nrSpan))
                {
                    continue;
                }

                Assert.IsNotNull(nrSpan.Attributes);
                Assert.IsTrue(nrSpan.Attributes.ContainsKey("instrumentation.provider"));
                Assert.AreEqual("opentelemetry", nrSpan.Attributes["instrumentation.provider"]);

            }

        }
    }
}
