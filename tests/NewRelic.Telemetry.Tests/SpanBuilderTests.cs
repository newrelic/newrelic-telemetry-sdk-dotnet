// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using NewRelic.Telemetry.Tracing;
using NUnit.Framework;

namespace NewRelic.Telemetry.Tests
{
    public class SpanBuilderTests
    {
        [Test]
        public void BuildSpan()
        {
            var span = new NewRelicSpan(
                traceId: "traceId",
                spanId: "spanId",
                timestamp: 1L,
                parentSpanId: "parentId",
                attributes: new Dictionary<string, object>()
                {
                    { "attrKey", "attrValue" },
                    { "adsfasdf", 12 },
                    { NewRelicConsts.Tracing.AttribNameDurationMs, 67 },
                    { NewRelicConsts.Tracing.AttribNameServiceName, "serviceName" },
                    { NewRelicConsts.Tracing.AttribNameHasError, true },
                    { NewRelicConsts.Tracing.AttribNameName, "name" },
                });

            Assert.AreEqual("spanId", span.Id);
            Assert.AreEqual("traceId", span.TraceId);
            Assert.AreEqual(1L, span.Timestamp);
            Assert.AreEqual("serviceName", span.Attributes?["service.name"]);
            Assert.AreEqual(true, span.Attributes?["error"]);
            Assert.AreEqual(67, span.Attributes?["duration.ms"]);
            Assert.AreEqual("name", span.Attributes?["name"]);
            Assert.AreEqual("parentId", span.Attributes?["parent.id"]);
            Assert.AreEqual("attrValue", span.Attributes?["attrKey"]);
        }
    }
}
