using System.Net;
using System.Threading.Tasks;
using NewRelic.Telemetry.Transport;

namespace NewRelic.Telemetry.Spans
{
    public interface ISpanBatchSender
    {
        Task<Response> SendDataAsync(SpanBatch spanBatch);
        string TraceUrl { get; }
    }

    public class SpanBatchSender : ISpanBatchSender
    {
        private IBatchDataSender _sender;
        public string TraceUrl => _sender.EndpointUrl;

        internal SpanBatchSender(IBatchDataSender sender) 
        {
            _sender = sender;
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
