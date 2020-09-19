// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using NewRelic.Telemetry.Tracing;
using Xunit;

namespace NewRelic.Telemetry.Tests
{
    public class SpanBuilderTests
    {
        [Fact]
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

            Assert.Equal("spanId", span.Id);
            Assert.Equal("traceId", span.TraceId);
            Assert.Equal(1L, span.Timestamp);
            Assert.Equal("serviceName", span.Attributes?["service.name"]);
            Assert.Equal(true, span.Attributes?["error"]);
            Assert.Equal(67, span.Attributes?["duration.ms"]);
            Assert.Equal("name", span.Attributes?["name"]);
            Assert.Equal("parentId", span.Attributes?["parent.id"]);
            Assert.Equal("attrValue", span.Attributes?["attrKey"]);
        }
    }
}
