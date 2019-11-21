using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NewRelic.Telemetry.Transport;

namespace NewRelic.Telemetry.Spans
{
    public interface ISpanBatchSender
    {
        Task<Response> SendDataAsync(SpanBatch spanBatch);
    }

    public class SpanBatchSender : ISpanBatchSender
    {
        private IBatchDataSender _sender;

        internal SpanBatchSender(IBatchDataSender sender) 
        {
            _sender = sender;
        }

        public async Task<Response> SendDataAsync(SpanBatch spanBatch)
        {
            if ((spanBatch?.Spans?.Count).GetValueOrDefault(0) == 0)
            {
                return new Response(false, 0, null);
            }

            var serializedPayload = spanBatch.ToJson();

            var response = await _sender.SendBatchAsync(serializedPayload);

            var retryAfterAPeriod = response.Headers?.RetryAfter?.Delta;
            var retryAfterASpecificTime = response.Headers?.RetryAfter?.Date;

            if (!retryAfterAPeriod.HasValue && retryAfterASpecificTime.HasValue) 
            {
                retryAfterAPeriod = retryAfterASpecificTime - DateTimeOffset.Now;
            }

            return new Response(true, response.StatusCode, retryAfterAPeriod);
        }
    }
}
