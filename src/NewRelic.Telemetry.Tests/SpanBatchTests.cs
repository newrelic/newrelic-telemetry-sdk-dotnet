using NUnit.Framework;
using System.Collections.Generic;
using NewRelic.Telemetry.Spans;

namespace NewRelic.Telemetry.Tests
{
    public class SpanBatchTests
    {
        [Test]
        public void TraceIdIsSet()
        {
            var traceId = "myId";
            //var spanBatch = new SpanBatch(new List<Span>(), new Dictionary<string, object>(), traceId);
            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId(traceId)
                .Build();
            
            Assert.AreEqual(traceId, spanBatch.CommonProperties.TraceId);
        }

        [Test]
        public void TraceIdIsNotSet()
        {
            var spanBatch = SpanBatchBuilder.Create().Build();

            //var spanBatch = new SpanBatch(new List<Span>(), new Dictionary<string, object>());
            Assert.Null(spanBatch.CommonProperties?.TraceId);
        }
    }
}