using NUnit.Framework;
using System;
using System.Collections.Generic;
using NewRelic.Telemetry.Spans;

namespace NewRelic.Telemetry.Tests
{
    public class SpanBuilderTests
    {
        [Test]
        public void BuildSpan()
        {
            var attributes = new Dictionary<string, object> { { "attrKey", "attrValue" } };

            var spanBuilder = new SpanBuilder("spanId");
            spanBuilder.TraceId("traceId").TimeStamp(1L).ServiceName("serviceName").DurationMs(67d).Name("name")
                .ParentId("parentId").Error(true).Attributes(attributes);
            var span = spanBuilder.Build();
            Assert.AreEqual("spanId", span.Id);
            Assert.AreEqual("traceId", span.TraceId);
            Assert.AreEqual(1L, span.Timestamp);
            Assert.AreEqual("serviceName", span.ServiceName);
            Assert.AreEqual(67d, span.DurationMs);
            Assert.AreEqual("name", span.Name);
            Assert.AreEqual("parentId", span.ParentId);
            Assert.AreEqual(true, span.Error);
            Assert.AreSame(attributes, span.Attributes);
        }

        [Test]
        public void ThrowExceptionIfNullId()
        {
            Assert.Throws<NullReferenceException>(new TestDelegate(() => new SpanBuilder(null)));
        }
    }
}