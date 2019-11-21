using NUnit.Framework;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;
using System.Threading;

namespace OpenTelemetry.Exporter.NewRelic.Tests
{

    public class SpanConverterTests
	{
        const string testServiceName = "TestService";
        const int expected_CountSpans = 3;

        private TestBatchSender _mockSender;

        private ISpan _otSpan0;
        private ISpan _otSpan1;
        private ISpan _otSpan2;
        private ISpan _otSpan3;

        [SetUp]
		public void Setup()
		{
            _mockSender = new TestBatchSender();

            var exporter = new NewRelicTraceExporter(_mockSender)
                .WithServiceName(testServiceName);

            using (var tracerFactory = TracerFactory.Create(
                                  builder => builder.AddProcessorPipeline(
                                        c => c.SetExporter(exporter))))
            {
                var tracer = tracerFactory.GetTracer("TestTracer");

                _otSpan0 = tracer.StartRootSpan("Test Span 1");
                _otSpan1 = tracer.StartSpan("Test Span 2", _otSpan0);
                _otSpan2 = tracer.StartRootSpan("Test Span 3");
                _otSpan3 = tracer.StartRootSpan("xsdf").PutHttpRawUrlAttribute(_mockSender.TraceUrl);

                _otSpan0.Status = Status.Ok;
                _otSpan1.Status = Status.Aborted;
                _otSpan2.Status = Status.Ok;
                _otSpan3.Status = Status.Ok;

                Thread.Sleep(100);
                _otSpan1.End();
                Thread.Sleep(125);
                _otSpan0.End();
                Thread.Sleep(150);
                _otSpan2.End();
                Thread.Sleep(175);
                _otSpan3.End();
            }
        }

		[Test]
		public void Test_ExpectedSpansCreated()
		{
            var resultNRSpansDic = _mockSender.CapturedSpansDic;

            Assert.AreEqual(expected_CountSpans, resultNRSpansDic.Count, "Unexpected number of spans");

            var resultSpan0 = resultNRSpansDic[_otSpan0.Context.SpanId.ToHexString()];
            var resultSpan1 = resultNRSpansDic[_otSpan1.Context.SpanId.ToHexString()];
            var resultSpan2 = resultNRSpansDic[_otSpan2.Context.SpanId.ToHexString()];

            Assert.IsNotNull(resultSpan0, $"Test Span not in output - testSpan0 - {_otSpan0.Context.SpanId.ToHexString()}");
            Assert.IsNotNull(resultSpan1, $"Test Span not in output - testSpan1 - {_otSpan1.Context.SpanId.ToHexString()}");
            Assert.IsNotNull(resultSpan2, $"Test Span not in output - testSpan2 - {_otSpan2.Context.SpanId.ToHexString()}");
        }

        [Test]
        public void Test_ErrorAttribute()
        {
            var resultNRSpansDic = _mockSender.CapturedSpansDic;

            var resultSpan0 = resultNRSpansDic[_otSpan0.Context.SpanId.ToHexString()];
            var resultSpan1 = resultNRSpansDic[_otSpan1.Context.SpanId.ToHexString()];
            var resultSpan2 = resultNRSpansDic[_otSpan2.Context.SpanId.ToHexString()];

            Assert.False(resultSpan0.Attributes.ContainsKey("error"), "resultSpan0 should NOT have Error Attribute");
            Assert.AreEqual(resultSpan1.Attributes["error"], true, "resultSpan1 should have Error Attribute = true");
            Assert.False(resultSpan2.Attributes.ContainsKey("error"), "resultSpan2 should NOT have Error Attribute");
        }

        [Test]
        public void Test_TraceId()
        {
            var resultNRSpansDic = _mockSender.CapturedSpansDic;

            var resultSpan0 = resultNRSpansDic[_otSpan0.Context.SpanId.ToHexString()];
            var resultSpan1 = resultNRSpansDic[_otSpan1.Context.SpanId.ToHexString()];
            var resultSpan2 = resultNRSpansDic[_otSpan2.Context.SpanId.ToHexString()];

            Assert.AreEqual(resultSpan0.TraceId, _otSpan0.Context.TraceId.ToHexString(), "Mismatch on TraceId - testSpan0");
            Assert.AreEqual(resultSpan1.TraceId, _otSpan1.Context.TraceId.ToHexString(), "Mismatch on TraceId - testSpan1");
            Assert.AreEqual(resultSpan2.TraceId, _otSpan2.Context.TraceId.ToHexString(), "Mismatch on TraceId - testSpan2");
            Assert.AreEqual(resultSpan1.TraceId, resultSpan1.TraceId, "Mismatch on TraceId - testSpan0 and testSpan1 should belong to the same trace");
            Assert.AreNotEqual(resultSpan0.TraceId, resultSpan2.TraceId, "Mismatch on TraceId - testSpan0 and testSpan2 should NOT belong to the same trace");
        }

        [Test]
        public void Test_FilterOutNewRelicEndpoint()
        {
            var resultNRSpansDic = _mockSender.CapturedSpansDic;

            Assert.IsFalse(resultNRSpansDic.ContainsKey(_otSpan3.Context.SpanId.ToHexString()), "Endpoint calls to New Relic should be excluded");
        }

        [Test]
        public void Test_Timestamps()
        {
            var resultNRSpansDic = _mockSender.CapturedSpansDic;

            var otSpans = new Span[]
            {
                _otSpan0 as Span,
                _otSpan1 as Span,
                _otSpan2 as Span,
            };

            foreach(var otSpan in otSpans)
            {
                var nrSpan = resultNRSpansDic[otSpan.Context.SpanId.ToHexString()];

                var expectedStartTimestampUnixMs = otSpan.StartTimestamp.ToUnixTimeMilliseconds();
                var expectedEndTimestampUnixMs = otSpan.EndTimestamp.ToUnixTimeMilliseconds();
                var expectedDurationMs = expectedEndTimestampUnixMs - expectedStartTimestampUnixMs;

                Assert.AreEqual(expectedStartTimestampUnixMs, nrSpan.Timestamp,$"{otSpan.Name} - Open Telemetry StartTime should translate to {expectedStartTimestampUnixMs}");
                Assert.AreEqual(expectedDurationMs, nrSpan.Attributes["duration.ms"]);
            }
        }
    }
}