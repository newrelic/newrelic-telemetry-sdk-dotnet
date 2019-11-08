using System.Net;

namespace NewRelic.Telemetry.Sdk
{
    public class Response
    {
        public bool DidSend;
        public HttpStatusCode StatusCode;

        public Response(bool didSend, HttpStatusCode statusCode)
        {
            DidSend = didSend;
            StatusCode = statusCode;
        }
    }
}
