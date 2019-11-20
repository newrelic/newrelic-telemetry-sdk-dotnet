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
using OpenTelemetry.Context.Propagation;

namespace OpenTelemetry.Exporter.NewRelic.Tests
{
    public class TestBatchSender : NRSpans.ISpanBatchSender
    {
        public readonly List<NRSpans.SpanBatch> CapturedSpanBatches = new List<NRSpans.SpanBatch>();
        
        public Task<Response> SendDataAsync(NRSpans.SpanBatch spanBatch)
        {
            CapturedSpanBatches.Add(spanBatch);

            return Task.FromResult(new Response(true, System.Net.HttpStatusCode.OK));
        }
    }

    public class SpanConverterTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void Test1()
		{
            const string testServiceName = "TestService";

            var mockSender = new TestBatchSender();
            var exporter = new NewRelicTraceExporter(mockSender)
                .WithServiceName(testServiceName);


            using (var tracerFactory = TracerFactory.Create(
                                  builder => builder.AddProcessorPipeline(
                                  c => c.SetExporter(new NewRelicTraceExporter(mockSender)))))
            {
                var tracer = tracerFactory.GetTracer("TestTracer");

                var rootSpan = tracer.StartRootSpan("Jason");
                var tootSpan = tracer.StartSpan("Ian", rootSpan);
                var pooSpan = tracer.StartSpan("Feingold", tootSpan);
                pooSpan.End();
                tootSpan.End();
                rootSpan.End();
            }

            var l = mockSender.CapturedSpanBatches;
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