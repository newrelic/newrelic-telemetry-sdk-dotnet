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
            var spanBatch = SpanBatch.Create()
                .WithTraceId(traceId);
            
            Assert.AreEqual(traceId, spanBatch.CommonProperties?.TraceId);
        }

        [Test]
        public void TraceIdIsNotSet()
        {
            var spanBatch = SpanBatch.Create();

            Assert.Null(spanBatch.CommonProperties?.TraceId);
        }
    }
}
