using NUnit.Framework;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Telerik.JustMock;
using OpenTelemetry.Exporter.NewRelic;
using OpenTelemetry.Trace.Export;

namespace OpenTelemetry.Exporter.NewRelic.Tests
{

    

    public class TestSpanProcessor : SimpleSpanProcessor
    {
        public TestSpanProcessor(SpanExporter exporter) : base(exporter)
        {
        }

        public override void OnEnd(Span span)
        {
            base.OnEnd(span);
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
            var tracer = TracerFactory.Create(tb => { }).GetTracer("test");
            var testSpan = (tracer.StartSpan("test") as Span);
            testSpan.Status = Status.Ok;
            testSpan.SetAttribute("jason", "feingold");



            //BatchSpanProcessor
            var l = new SimpleSpanProcessor();
            l.


            testSpan.UpdateName("test");

            var nrSpan = SpanConverter.ToNewRelicSpan(testSpan,"boger");

            var traceId = ActivityTraceId.CreateRandom();
            var spanId = ActivitySpanId.CreateRandom();
            SpanContext ctx = new SpanContext(traceId, spanId, ActivityTraceFlags.None);
		}

        [Test]
        public void Validation_OpenTraceSpan_Required()
        {
            Assert.Throws<ArgumentNullException>(()=>SpanConverter.ToNewRelicSpan(null, null));
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