using System;
using System.Net.Http;

namespace NewRelic.Telemetry.Sdk
{
    public class SpanBatchSender
    {
        BatchDataSender _sender;
        SpanBatchMarshaller _marshaller;

        public SpanBatchSender(BatchDataSender sender, SpanBatchMarshaller marshaller) 
        {
            _sender = sender;
            _marshaller = marshaller;
        }

        public Response SendData(SpanBatch spanBatch)
        {
            if (spanBatch?.Spans?.Count == 0)
            {
                return new Response(false, null);
            }

            var serializedPayload = _marshaller.ToJson(spanBatch);

            // TODO: try/catch is for troubleshooting during dev; may not be needed for released code

            //            var response = _sender.SendBatch(serializedPayload);
//            return new Response(true, response);

            try
            {
                var response = _sender.SendBatch(serializedPayload);
                return new Response(true, response);
            }
            catch (Exception ex)
            {
                var y = ex;
                return new Response();
            }
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
