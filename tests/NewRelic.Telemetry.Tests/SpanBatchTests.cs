// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using NewRelic.Telemetry.Tracing;
using NUnit.Framework;

namespace NewRelic.Telemetry.Tests
{
    public class SpanBatchTests
    {
        [Test]
        public void TraceIdIsSet()
        {
            var traceId = "myId";

            var spanBatch = new NewRelicSpanBatch(
                commonProperties: new NewRelicSpanBatchCommonProperties(traceId: traceId),
                spans: new NewRelicSpan[0]);

            Assert.AreEqual(traceId, spanBatch.CommonProperties.TraceId);
        }

        [Test]
        public void TraceIdIsNotSet()
        {
            var spanBatch = new NewRelicSpanBatch(
                spans: new NewRelicSpan[0]);

            Assert.Null(spanBatch.CommonProperties.TraceId);
        }
    }
}
