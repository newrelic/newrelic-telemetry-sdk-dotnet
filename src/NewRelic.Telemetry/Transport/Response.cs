using System;
using System.Net;

namespace NewRelic.Telemetry.Transport
{
    public enum NewRelicResponseStatus
    {
        /// <summary>
        /// Data was not sent
        /// </summary>
        DidNotSend_NoData,

        /// <summary>
        /// An attempt was made to send data to New Relic endpoint, but it was not accepted by the endpoint
        /// Represents Http Response Codes other than 2xx
        /// </summary>
        Failure,

        /// <summary>
        /// Data was sent to New Relic endpoint. It was accepted.
        /// Represents Https Response Codes 2xx
        /// </summary>
        Success
    }

    public class Response
    {
        public readonly static Response DidNotSend = new Response(NewRelicResponseStatus.DidNotSend_NoData);
        public readonly static Response Success = new Response(NewRelicResponseStatus.Success);

        public static Response Failure(HttpStatusCode? httpStatusCode, string responseMessage)
        {
            var result = new Response(NewRelicResponseStatus.Failure);
            result.HttpStatusCode = httpStatusCode;
            result.Message = responseMessage;

            return result;
        }

        public static Response Failure(string responseMessage)
        {
            return Failure(null, responseMessage);
        }

        public static Response Exception(Exception ex)
        {
            var result = new Response(NewRelicResponseStatus.Failure);

            result.Message = ex.Message;

            return result;
        }


        public NewRelicResponseStatus ResponseStatus { get; private set; }

        public HttpStatusCode? HttpStatusCode { get; private set; }

        public string Message { get; private set; }

        internal Response(NewRelicResponseStatus responseStatus)
        {
            ResponseStatus = responseStatus;
        }
    }
}
