using NewRelic.Telemetry.Spans;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NewRelic.Telemetry.Client
{
    public class TelemetryClient
    {
        private ISpanBatchSender _spanBatchSender;
        private const int BACKOFF_FACTOR_SECONDS = 5; // In seconds.
        private const int BACKOFF_MAX_SECONDS = 80; // In seconds. 
        private const int MAX_RETRIES = 8;
        private readonly Func<int, Task> _delayer;

        private static readonly Func<int, Task> _defaultDelayer = new Func<int, Task>(async (int milliseconds) => await Task.Delay(milliseconds));

        public TelemetryClient(ISpanBatchSender spanBatchSender) : this(spanBatchSender, _defaultDelayer)
        {
        }

        internal TelemetryClient(ISpanBatchSender spanBatchSender, Func<int, Task> delayer)
        {
            _spanBatchSender = spanBatchSender;
            _delayer = delayer;
        }

        public async Task SendBatchAsync(SpanBatch spanBatch) 
        {
            await SendBatchAsyncInternal(spanBatch, 0);
        }

        private async Task SendBatchAsyncInternal(SpanBatch spanBatch, int retryNum)
        {
            var response = await _spanBatchSender.SendDataAsync(spanBatch);

            switch (response.StatusCode)
            {
                case HttpStatusCode code when code >= HttpStatusCode.OK && code <= (HttpStatusCode)299:
                    Logging.LogDebug($@"Response from New Relic ingest API: code: {response.StatusCode}, body: {response.Content} ");
                    return;
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.NotFound:
                case HttpStatusCode.MethodNotAllowed:
                case HttpStatusCode.LengthRequired:
                    //drop data and log out error
                    Logging.LogError($@"Response from New Relic ingest API: code: {response.StatusCode}, body: {response.Content} ");
                    break;
                case HttpStatusCode.RequestTimeout:
                    Logging.LogWarning($@"Response from New Relic ingest API: code: {response.StatusCode}, body: {response.Content} ");
                    await Retry(spanBatch, retryNum);
                    break;
                case HttpStatusCode.RequestEntityTooLarge:
                    //split payload.
                    break;
                case (HttpStatusCode)429:
                    //handle 429 error according to the spec.
                    break;
                default:
                    Logging.LogError($@"Response from New Relic ingest API: code: {response.StatusCode}, body: {response.Content}");
                    break;
            }
        }

        private async Task Retry(SpanBatch spanBatch, int retryNum)
        {
            retryNum++;
            if (retryNum > MAX_RETRIES)
            {
                Logging.LogWarning($@"Number of retries exceeded.");
                return;
            }

            Logging.LogWarning($@"Retry({retryNum}).");

            var waitTime = (int) Math.Min(BACKOFF_MAX_SECONDS, BACKOFF_FACTOR_SECONDS * Math.Pow(2, retryNum - 1)) * 1000;

            await _delayer(waitTime);
            await SendBatchAsyncInternal(spanBatch, retryNum);
            return;
        }
    }
}
