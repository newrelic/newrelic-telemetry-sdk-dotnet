using NUnit.Framework;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Telerik.JustMock;
using OpenTelemetry.Exporter.NewRelic;
using OpenTelemetry.Trace.Export;
using NRSpans = NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;
using System.Threading.Tasks;
using System.Linq;
using OpenTelemetry.Context.Propagation;

namespace OpenTelemetry.Exporter.NewRelic.Tests
{
    public class TestBatchSender : NRSpans.ISpanBatchSender
    {
        public readonly List<NRSpans.SpanBatch> CapturedSpanBatches = new List<NRSpans.SpanBatch>();
        public Dictionary<string, NRSpans.Span> CapturedSpansDic => CapturedSpanBatches.SelectMany(x => x.Spans).ToDictionary(x => x.Id);
        
        public Task<Response> SendDataAsync(NRSpans.SpanBatch spanBatch)
        {
            CapturedSpanBatches.Add(spanBatch);

            return Task.FromResult(new Response(true, System.Net.HttpStatusCode.OK));
        }
    }

    public class SpanConverterTests
	{
        const string testServiceName = "TestService";
        const int expected_CountSpans = 3;

        private TestBatchSender _mockSender;

        private ISpan _otSpan0;
        private ISpan _otSpan1;
        private ISpan _otSpan2;

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

                var l = tracer.StartSpan("xsdf");
                l.PutHttpRawUrlAttribute("https://cnn.com");
               


                _otSpan0.Status = Status.Ok;
                _otSpan1.Status = Status.Aborted;
                _otSpan2.Status = Status.Ok;

                _otSpan1.End();
                _otSpan0.End();
                _otSpan2.End();
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


        /*
         * OTSpanRequired
         * OTSpanRequiresContext
         * SpanId
         * StartTime
         * NR Endpoint not returned
         * Execution Time - Start
         * Executions Time - start + End also has duration
         * Parent
         * Attributes
         * 
         */
    }
}