using NUnit.Framework;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Telerik.JustMock;
using OpenTelemetry.Exporter.NewRelic;

namespace OpenTelemetry.Exporter.NewRelic.Tests
{

  
   

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
            var testSpan = tracer.StartSpan("test");

            testSpan.Status = Status.Ok;

            testSpan.UpdateName("test");

            SpanConverter.ToNewRelicSpan();

            var traceId = ActivityTraceId.CreateRandom();
            var spanId = ActivitySpanId.CreateRandom();
            SpanContext ctx = new SpanContext(traceId, spanId, ActivityTraceFlags.None);
		}

        [Test]
        public void Validation_OpenTraceSpan_Required()
        {


            var traceId = ActivityTraceId.CreateFromString(null);
            var spanId = ActivitySpanId.CreateRandom();

            SpanContext ctx = new SpanContext(traceId, spanId, ActivityTraceFlags.None);
            var testSpan = Mock.Create<ISpan>();

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