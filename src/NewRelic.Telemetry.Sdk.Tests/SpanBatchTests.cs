using NUnit.Framework;
using System.Collections.Generic;

namespace NewRelic.Telemetry.Sdk.Tests
{
    public class SpanBatchTests
    {
        [Test]
        public void TraceIdIsSet()
        {
            var traceId = "myId";
            var spanBatch = new SpanBatch(new List<Span>(), new Dictionary<string, object>(), traceId);
            Assert.AreEqual(traceId, spanBatch.TraceId);
        }

        [Test]
        public void TraceIdIsNotSet()
        {
            var spanBatch = new SpanBatch(new List<Span>(), new Dictionary<string, object>());
            Assert.Null(spanBatch.TraceId);
        }
    }
}