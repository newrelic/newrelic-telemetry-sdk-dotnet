using System.Net;

namespace NewRelic.Telemetry
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
