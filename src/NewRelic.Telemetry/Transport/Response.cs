using System.Net;

namespace NewRelic.Telemetry.Transport
{
    public enum NewRelicResponseStatus
    {
        /// <summary>
        /// Data was not sent
        /// </summary>
        DidNotSend,

        /// <summary>
        /// An attempt was made to send data to NR endpoint, but it was not accepted by the endpoint
        /// Represents Http Response Codes other than 2xx
        /// </summary>
        SendFailure,

        /// <summary>
        /// Data was sent to NR endpoint. It was accepted.
        /// Represents Https Response Codes 2xx
        /// </summary>
        SendSuccess
    }

    public class Response
    {
       
        public readonly static Response DidNotSend = new Response(NewRelicResponseStatus.DidNotSend);
        public readonly static Response Success = new Response(NewRelicResponseStatus.SendSuccess);

        public static Response Failure(HttpStatusCode httpStatusCode, string responseMessage)
        {
            var result = new Response(NewRelicResponseStatus.SendFailure);
            result.HttpStatusCode = httpStatusCode;
            result.Body = responseMessage;

            return result;
        }


        public NewRelicResponseStatus ResponseStatus { get; private set; }

        public HttpStatusCode HttpStatusCode { get; private set; }

        public string Body { get; private set; }

        internal Response(NewRelicResponseStatus responseStatus)
        {
            ResponseStatus = responseStatus;
        }
    }
}
