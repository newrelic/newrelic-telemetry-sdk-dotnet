using System;
using System.Net;

namespace NewRelic.Telemetry.Transport
{
    public enum NewRelicResponseStatus
    {
        /// <summary>
        /// A request was made to send data to New Relic with an empty payload.  This is not an
        /// error condition, but may represent an issue with the usage of the Telemetry SDK.
        /// </summary>
        DidNotSend_NoData,

        /// <summary>
        /// An attempt was made to send data to New Relic endpoint, but an unexpected failure occurred.
        /// </summary>
        Failure,

        /// <summary>
        /// Data was sent to New Relic endpoint and was accepted for processing.
        /// Represents Http Response Codes 2xx
        /// </summary>
        Success
    }

    /// <summary>
    /// Provides information regarding the outcome of a request to send data to a New Relic endpoint.
    /// </summary>
    public class Response
    {
        internal readonly static Response DidNotSend = new Response(NewRelicResponseStatus.DidNotSend_NoData);
        internal readonly static Response Success = new Response(NewRelicResponseStatus.Success);

        internal static Response Failure(HttpStatusCode? httpStatusCode, string responseMessage)
        {
            var result = new Response(NewRelicResponseStatus.Failure);
            result.HttpStatusCode = httpStatusCode;
            result.Message = responseMessage;

            return result;
        }

        internal static Response Failure(string responseMessage)
        {
            return Failure(null, responseMessage);
        }

        internal static Response Exception(Exception ex)
        {
            var result = new Response(NewRelicResponseStatus.Failure);

            result.Message = ex.Message;

            return result;
        }


        /// <summary>
        /// Summarizes the outcome of the request.  See <see cref="NewRelicResponseStatus"/> for the possible outcomes.
        /// </summary>
        public NewRelicResponseStatus ResponseStatus { get; private set; }

        /// <summary>
        /// If able to communicate with the New Relic endpoint, this is the HTTP response code returned by the endpoint.
        /// This value will be NULL if a failure occurred prior to or during the communication with New Relic.
        /// </summary>
        public HttpStatusCode? HttpStatusCode { get; private set; }

        /// <summary>
        /// Provides additional contextual information about the outcome.
        /// </summary>
        public string? Message { get; private set; }

        internal Response(NewRelicResponseStatus responseStatus)
        {
            ResponseStatus = responseStatus;
        }
    }
}
