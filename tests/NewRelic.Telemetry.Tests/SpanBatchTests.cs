// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using NewRelic.Telemetry.Tracing;
using Xunit;

namespace NewRelic.Telemetry.Tests
{
    public class SpanBatchTests
    {
        [Fact]
        public void TraceIdIsSet()
        {
            var traceId = "myId";

            var spanBatch = new NewRelicSpanBatch(
                commonProperties: new NewRelicSpanBatchCommonProperties(traceId: traceId),
                spans: new NewRelicSpan[0]);

            Assert.Equal(traceId, spanBatch.CommonProperties.TraceId);
        }

        [Fact]
        public void TraceIdIsNotSet()
        {
            var spanBatch = new NewRelicSpanBatch(
                spans: new NewRelicSpan[0]);

            Assert.Null(spanBatch.CommonProperties.TraceId);
        }
    }
}
