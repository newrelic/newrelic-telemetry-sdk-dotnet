using System.Net;

namespace NewRelic.Telemetry
{
    public class Response
    {
        public bool DidSend;
        public HttpStatusCode StatusCode;
        public string Content;

        public Response(bool didSend, HttpStatusCode statusCode, string content)
        {
            DidSend = didSend;
            StatusCode = statusCode;
            Content = content;
        }
    }
}
