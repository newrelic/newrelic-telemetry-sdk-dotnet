using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("NewRelic.Telemetry.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100198f2915b649f8774e7937c4e37e39918db1ad4e83109623c1895e386e964f6aa344aeb61d87ac9bd1f086a7be8a97d90f9ad9994532e5fb4038d9f867eb5ed02066ae24086cf8a82718564ebac61d757c9cbc0cc80f69cc4738f48f7fc2859adfdc15f5dde3e05de785f0ed6b6e020df738242656b02c5c596a11e628752bd0")]

namespace NewRelic.Telemetry
{
    public class SpanBatchSender
    {
        private BatchDataSender _sender;
        private SpanBatchMarshaller _marshaller;

        internal SpanBatchSender(BatchDataSender sender, SpanBatchMarshaller marshaller) 
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
