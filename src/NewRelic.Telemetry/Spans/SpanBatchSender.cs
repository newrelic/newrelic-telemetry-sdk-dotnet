using System.Net;
using System.Threading.Tasks;
using NewRelic.Telemetry.Transport;

namespace NewRelic.Telemetry.Spans
{
    public interface ISpanBatchSender
    {
        Task<Response> SendDataAsync(SpanBatch spanBatch);
    }

    public class SpanBatchSender
    {
        private IBatchDataSender _sender;
        private ISpanBatchMarshaller _marshaller;

        internal SpanBatchSender(IBatchDataSender sender, ISpanBatchMarshaller marshaller) 
        {
            _sender = sender;
            _marshaller = marshaller;
        }

        public async Task<Response> SendDataAsync(SpanBatch spanBatch)
        {
            if (spanBatch?.Spans?.Count == 0)
            {
                return new Response(false, (HttpStatusCode)0);
            }

            var serializedPayload = _marshaller.ToJson(spanBatch);

            var response = await _sender.SendBatchAsync(serializedPayload);

            return new Response(true, response.StatusCode);
        }
    }
}
