// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using NewRelic.Telemetry.Spans;
using NUnit.Framework;

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
            Assert.AreEqual(true, span.Attributes["error"]);
            Assert.AreEqual(67, span.Attributes["duration.ms"]);
            Assert.AreEqual("name", span.Attributes["name"]);
            Assert.AreEqual("parentId", span.Attributes["parent.id"]);
            Assert.AreEqual("attrValue", span.Attributes["attrKey"]);
        }

        [Test]
        public void HasErrorTest()
        {
            var attributes = new Dictionary<string, object>
            { { "attrKey", "attrValue" } };

            var span0 = SpanBuilder.Create("spanId")
                .WithTraceId("traceId")
                .WithAttributes(attributes)
                .Build();

            var span1 = SpanBuilder.Create("spanId")
                .WithTraceId("traceId")
                .WithAttributes(attributes)
                .HasError(true)
                .Build();

            var span2 = SpanBuilder.Create("spanId")
                .WithTraceId("traceId")
                .WithAttributes(attributes)
                .HasError("This was a bad error on span 2")
                .Build();

            var span3 = SpanBuilder.Create("spanId")
                .WithTraceId("traceId")
                .WithAttributes(attributes)
                .HasError("This was a bad error on span 3")
                .HasError(false)
                .Build();

            Assert.IsFalse(span0.Attributes.ContainsKey("error"));
            Assert.IsFalse(span0.Attributes.ContainsKey("error.message"));

            Assert.AreEqual(true, span1.Attributes["error"]);
            Assert.IsFalse(span1.Attributes.ContainsKey("error.message"));

            Assert.AreEqual(true, span2.Attributes["error"]);
            Assert.IsTrue(span2.Attributes.ContainsKey("error.message"));
            Assert.AreEqual("This was a bad error on span 2", span2.Attributes["error.message"]);

            Assert.IsFalse(span3.Attributes.ContainsKey("error"));
            Assert.IsFalse(span3.Attributes.ContainsKey("error.message"));
        }

        [Test]
        public void ThrowExceptionIfNullId()
        {
            Assert.Throws<NullReferenceException>(new TestDelegate(() => SpanBuilder.Create(null)));
        }
    }
}