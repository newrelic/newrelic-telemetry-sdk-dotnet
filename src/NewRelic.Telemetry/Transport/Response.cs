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
        public readonly static Response ResponseFailure = new Response(NewRelicResponseStatus.SendFailure);
        public readonly static Response ResponseDidNotSend = new Response(NewRelicResponseStatus.DidNotSend);
        public readonly static Response Success = new Response(NewRelicResponseStatus.SendSuccess);

        public NewRelicResponseStatus ResponseStatus { get; private set; }

        internal Response(NewRelicResponseStatus responseStatus)
        {
            ResponseStatus = responseStatus;
        }
    }
}
