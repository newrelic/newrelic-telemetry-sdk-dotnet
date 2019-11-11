using System.Net;

namespace NewRelic.Telemetry.Transport
{
    public class Response
    {
        public bool DidSend { get; }
        public HttpStatusCode StatusCode { get; }

        internal Response(bool didSend, HttpStatusCode statusCode)
        {
            DidSend = didSend;
            StatusCode = statusCode;
        }
    }
}
