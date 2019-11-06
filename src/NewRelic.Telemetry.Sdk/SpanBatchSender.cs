using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NewRelic.Telemetry.Sdk
{
    public class SpanBatchSender
    {
        private BatchDataSender _sender;
        private SpanBatchMarshaller _marshaller;

        public SpanBatchSender(BatchDataSender sender, SpanBatchMarshaller marshaller) 
        {
            _sender = sender;
            _marshaller = marshaller;
        }

        public async Task<Response> SendDataAsync(SpanBatch spanBatch)
        {
            if (spanBatch?.Spans?.Count == 0)
            {
                return new Response(false, null);
            }

            var serializedPayload = _marshaller.ToJson(spanBatch);

            var response = await _sender.SendBatch(serializedPayload);
            return new Response(true, response);
        }
    }

    public struct Response
    {
        public bool DidSend;
        public HttpResponseMessage Message;

        public Response(bool didSend, HttpResponseMessage message)
        {
            DidSend = didSend;
            Message = message;
        }
    }
}
