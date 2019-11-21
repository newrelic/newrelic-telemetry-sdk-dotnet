using System;
using System.Net;
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

        //internal SpanBatchSender(IBatchDataSender sender) 
        //{
        //    _sender = sender;
        //}
        internal SpanBatchSender()
        {
            _sender = new BatchDataSender(Configuration.ApiKey, Configuration.TraceUrl, Configuration.AuditLoggingEnabled, TimeSpan.FromSeconds(5));
        }

        public async Task<Response> SendDataAsync(SpanBatch spanBatch)
        {
            if ((spanBatch?.Spans?.Count).GetValueOrDefault(0) == 0)
            {
                return new Response(false, (HttpStatusCode)0);
            }

            var serializedPayload = spanBatch.ToJson();

            var response = await _sender.SendBatchAsync(serializedPayload);

            return new Response(true, response.StatusCode);
        }
    }
}
