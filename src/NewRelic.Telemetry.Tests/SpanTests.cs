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
            var attributes = new Dictionary<string, object> 
            { { "attrKey", "attrValue" } };

            var spanBuilder = SpanBuilder.Create("spanId")
                .WithTraceId("traceId")
                .WithTimestamp(1L)
                .WithServiceName("serviceName")
                .WithDurationMs(67)
                .WithName("name")
                .WithParentId("parentId")
                .HasError(true)
                .WithAttribute("adsfasdf",12)
                .WithAttributes(attributes);

            var span = spanBuilder.Build();
            Assert.AreEqual("spanId", span.Id);
            Assert.AreEqual("traceId", span.TraceId);
            Assert.AreEqual(1L, span.Timestamp);
            Assert.AreEqual("serviceName", span.Attributes["service.name"]);
            Assert.AreEqual(67, span.Attributes["duration.ms"]);
            Assert.AreEqual("name", span.Attributes["name"]);
            Assert.AreEqual("parentId", span.Attributes["parent.id"]);
            Assert.AreEqual(true, span.Error);
            Assert.AreEqual("attrValue", span.Attributes["attrKey"]);
        }

        [Test]
        public void ThrowExceptionIfNullId()
        {
            Assert.Throws<NullReferenceException>(new TestDelegate(() => SpanBuilder.Create(null)));
        }
    }
}