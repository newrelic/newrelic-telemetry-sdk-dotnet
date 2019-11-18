using System.Net;

namespace NewRelic.Telemetry.Transport
{
    public class Response
    {
        public bool DidSend { get; }
        public HttpStatusCode StatusCode { get; }
        public string Content { get; }


        internal Response(bool didSend, HttpStatusCode statusCode, string content)
        {
            DidSend = didSend;
            StatusCode = statusCode;
            Content = content ;
        }
    }
}
