// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if !INTERNALIZE_TELEMETRY_SDK
using Microsoft.Extensions.Configuration;
#endif
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry.Transport;

namespace NewRelic.Telemetry.Tracing
{
#if INTERNALIZE_TELEMETRY_SDK
    internal
#else
    public
#endif
    class TraceDataSender : DataSender<NewRelicSpanBatch>
    {
        protected override Uri EndpointUrl => _config.TraceUrl;

        protected override bool ContainsNoData(NewRelicSpanBatch dataToCheck)
        {
            return !dataToCheck.Spans.Any();
        }

        private static readonly NewRelicSpanBatch[] _emptySpanBatchArray = new NewRelicSpanBatch[0];

        protected override NewRelicSpanBatch[] Split(NewRelicSpanBatch spanBatch)
        {
            var countSpans = spanBatch.Spans.Count();
            if (countSpans <= 1)
            {
                return _emptySpanBatchArray;
            }

            var targetSpanCount = countSpans / 2;
            var batch0Spans = spanBatch.Spans.Take(targetSpanCount).ToList();
            var batch1Spans = spanBatch.Spans.Skip(targetSpanCount).ToList();

            var result = new[]
            {
                new NewRelicSpanBatch(batch0Spans, spanBatch.CommonProperties),
                new NewRelicSpanBatch(batch1Spans, spanBatch.CommonProperties),
            };

            return result;
        }

        public TraceDataSender(TelemetryConfiguration config, ILoggerFactory? loggerFactory)
            : base(config, loggerFactory)
        {
        }

        internal TraceDataSender(TelemetryConfiguration config, ILoggerFactory? loggerFactory, string telemetrySdkVersionOverride)
            : base(config, loggerFactory, telemetrySdkVersionOverride)
        {
        }

#if !INTERNALIZE_TELEMETRY_SDK
        public TraceDataSender(IConfiguration config, ILoggerFactory? loggerFactory)
            : base(config, loggerFactory)
        {
        }
#endif

        public async Task<Response> SendDataAsync(IEnumerable<NewRelicSpan> spans)
        {
            var batch = new NewRelicSpanBatch(spans);

            return await SendDataAsync(batch);
        }
    }
}
