using NUnit.Framework;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewRelic.Telemetry;
using NRSpans = NewRelic.Telemetry.Spans;


namespace OpenTelemetry.Exporter.NewRelic.Tests
{ 
    public class SpanConverterTests
	{
        const string testServiceName = "TestService";
        const int expected_CountSpans = 3;

        private List<ISpan> _otSpans = new List<ISpan>();
        private List<NRSpans.Span> _resultNRSpans = new List<NRSpans.Span>();

        private Dictionary<string, NRSpans.Span> resultNRSpansDic => _resultNRSpans.ToDictionary(x => x.Id);

        [SetUp]
		public void Setup()
		{
            var config = new TelemetryConfiguration().WithAPIKey("123456").WithServiceName(testServiceName);
            var mockDataSender = new NRSpans.SpanDataSender(config);

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

                _otSpans.Add(tracer.StartRootSpan("Test Span 1"));
                _otSpans.Add(tracer.StartSpan("Test Span 2", _otSpans[0]));
                _otSpans.Add(tracer.StartRootSpan("Test Span 3"));
                _otSpans.Add(tracer.StartRootSpan("xsdf").PutHttpRawUrlAttribute(config.TraceUrl));

                _otSpans[0].Status = Status.Ok;
                _otSpans[1].Status = Status.Aborted;
                _otSpans[2].Status = Status.Ok;
                _otSpans[3].Status = Status.Ok;

                Thread.Sleep(100);
                _otSpans[1].End();
                Thread.Sleep(125);
                _otSpans[0].End();
                Thread.Sleep(150);
                _otSpans[2].End();
                Thread.Sleep(175);
                _otSpans[3].End();
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

            Assert.IsNotNull(resultNRSpan0, $"Test Span not in output - testSpan0 - {_otSpans[0].Context.SpanId.ToHexString()}");
            Assert.IsNotNull(resultNRSpan1, $"Test Span not in output - testSpan1 - {_otSpans[1].Context.SpanId.ToHexString()}");
            Assert.IsNotNull(resultNRSpan2, $"Test Span not in output - testSpan2 - {_otSpans[2].Context.SpanId.ToHexString()}");
        }

        [Test]
        public void Test_ErrorAttribute()
        {
            var resultSpan0 = resultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultSpan1 = resultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultSpan2 = resultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];

            Assert.False(resultSpan0.Attributes.ContainsKey("error"), "resultSpan0 should NOT have Error Attribute");
            Assert.AreEqual(resultSpan1.Attributes["error"], true, "resultSpan1 should have Error Attribute = true");
            Assert.False(resultSpan2.Attributes.ContainsKey("error"), "resultSpan2 should NOT have Error Attribute");
        }

        [Test]
        public void Test_TraceId()
        {
            var resultNRSpan0 = resultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultNRSpan1 = resultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultNRSpan2 = resultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];


            Assert.AreEqual(resultNRSpan0.TraceId, _otSpans[0].Context.TraceId.ToHexString(), "Mismatch on TraceId - testSpan0");
            Assert.AreEqual(resultNRSpan1.TraceId, _otSpans[1].Context.TraceId.ToHexString(), "Mismatch on TraceId - testSpan1");
            Assert.AreEqual(resultNRSpan2.TraceId, _otSpans[2].Context.TraceId.ToHexString(), "Mismatch on TraceId - testSpan2");
            Assert.AreEqual(resultNRSpan0.TraceId, resultNRSpan1.TraceId, "Mismatch on TraceId - testSpan0 and testSpan1 should belong to the same trace");
            Assert.AreNotEqual(resultNRSpan0.TraceId, resultNRSpan2.TraceId, "Mismatch on TraceId - testSpan0 and testSpan2 should NOT belong to the same trace");
        }

        [Test]
        public void Test_ParentSpanId()
        {
            var resultNRSpan0 = resultNRSpansDic[_otSpans[0].Context.SpanId.ToHexString()];
            var resultNRSpan1 = resultNRSpansDic[_otSpans[1].Context.SpanId.ToHexString()];
            var resultNRSpan2 = resultNRSpansDic[_otSpans[2].Context.SpanId.ToHexString()];

            Assert.IsNull(resultNRSpan0.ParentId, "Top Level Span should have NULL parentID");
            Assert.AreEqual(resultNRSpan1.ParentId, resultNRSpan0.Id, "Mismatch on ParentId - Span1 is a child of Span0");
            Assert.IsNull(resultNRSpan2.ParentId, "Top Level Span should have NULL parentID");
        }

        [Test]
        public void Test_FilterOutNewRelicEndpoint()
        {
            Assert.IsFalse(resultNRSpansDic.ContainsKey(_otSpans[3].Context.SpanId.ToHexString()), "Endpoint calls to New Relic should be excluded");
        }

        [Test]
        public void Test_Timestamps()
        {
            foreach (var otISpan in _otSpans)
            {
                var otSpan = otISpan as Span;

                if (!resultNRSpansDic.TryGetValue(otSpan.Context.SpanId.ToHexString(), out var nrSpan))
                {
                    continue;
                }

                var expectedStartTimestampUnixMs = otSpan.StartTimestamp.ToUnixTimeMilliseconds();
                var expectedEndTimestampUnixMs = otSpan.EndTimestamp.ToUnixTimeMilliseconds();
                var expectedDurationMs = expectedEndTimestampUnixMs - expectedStartTimestampUnixMs;

                Assert.AreEqual(expectedStartTimestampUnixMs, nrSpan.Timestamp, $"{otSpan.Name} - Open Telemetry StartTime should translate to {expectedStartTimestampUnixMs}");
                Assert.AreEqual(expectedDurationMs, nrSpan.Attributes["duration.ms"]);
            }
        }
    }
}
