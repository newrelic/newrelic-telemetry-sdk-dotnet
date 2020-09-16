// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

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
            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId(traceId)
                .Build();
            
            Assert.AreEqual(traceId, spanBatch.CommonProperties.TraceId);
        }

        [Test]
        public void TraceIdIsNotSet()
        {
            var spanBatch = SpanBatchBuilder.Create().Build();
            Assert.Null(spanBatch.CommonProperties?.TraceId);
        }
    }
}
