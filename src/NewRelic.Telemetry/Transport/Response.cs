using System;
using System.Collections.Generic;
using System.Net;

namespace NewRelic.Telemetry.Transport
{
    public class Response

    {
        public bool DidSend { get; }
        
        public HttpStatusCode StatusCode { get; }
        
        public string Content { get; }
        
        public TimeSpan? RetryAfter { get; }

        internal Response(bool didSend, HttpStatusCode statusCode, TimeSpan? retryAfter)
        {
            DidSend = didSend;
            StatusCode = statusCode;
            RetryAfter = retryAfter;
        }
    }
}
