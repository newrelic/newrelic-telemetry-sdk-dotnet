using NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NewRelic.Telemetry.Client
{
    public class TelemetryClient
    {
        private ISpanBatchSender _spanBatchSender;
        private const int BACKOFF_FACTOR_SECONDS = 5; // In seconds.
        private const int BACKOFF_MAX_SECONDS = 80; // In seconds.
        private const int USING_BACKOFF_SEQUENCE = -1;
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
                    Logging.LogDebug($@"Response from New Relic ingest API: code: {response.StatusCode}");
                    return;
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.NotFound:
                case HttpStatusCode.MethodNotAllowed:
                case HttpStatusCode.LengthRequired:
                    //drop data and log out error
                    Logging.LogError($@"Response from New Relic ingest API: code: {response.StatusCode}");
                    break;
                case HttpStatusCode.RequestTimeout:
                    Logging.LogWarning($@"Response from New Relic ingest API: code: {response.StatusCode}");
                    await Retry(spanBatch, retryNum, USING_BACKOFF_SEQUENCE);
                    break;

                case HttpStatusCode.RequestEntityTooLarge:
                    Logging.LogWarning($@"Response from New Relic ingest API: code: {response.StatusCode}. Response indicates payload is too large.");
                    await RetryWithSplit(spanBatch);

                    break;
                case (HttpStatusCode)429:
                    Logging.LogWarning($@"Response from New Relic ingest API: code: {response.StatusCode}. ");
                    await RetryOn429StatusCode(spanBatch, retryNum, response);
                    break;
                default:
                    Logging.LogError($@"Response from New Relic ingest API: code: {response.StatusCode}");
                    break;
            }
        }

        private async Task RetryWithSplit(SpanBatch spanBatch)
        {
            var newBatches = SpanBatchBuilder.Split(spanBatch);
            if (newBatches == null)
            {
                Logging.LogError($@"Cannot send the span batch because it has a single span that exceeds the size limit.");
                return;
            }

            Logging.LogWarning("Splitting the span batch and retrying.");

            var taskList = new Task[newBatches.Length];

            for (var i = 0; i < newBatches.Length; i++)
            {
                taskList[i] = SendBatchAsyncInternal(newBatches[i], 0);
            }

            await Task.WhenAll(taskList);
        }

        private async Task RetryOn429StatusCode(SpanBatch spanBatch, int retryNum, Response responseMessage)
        {
            var waitTimeInMilliSeconds = (int?)responseMessage.RetryAfter?.TotalMilliseconds;
            await Retry(spanBatch, retryNum, waitTimeInMilliSeconds ?? USING_BACKOFF_SEQUENCE);
        }

        private async Task Retry(SpanBatch spanBatch, int retryNum, int waitTime)
        {
            retryNum++;
            if (retryNum > MAX_RETRIES)
            {
                Logging.LogError($@"SendBatchAsync(SpanBatch spanBatch) timed out after {MAX_RETRIES} attempts.");
                return;
            }

            if (waitTime < 0)
            {
                waitTime = (int)Math.Min(BACKOFF_MAX_SECONDS, BACKOFF_FACTOR_SECONDS * Math.Pow(2, retryNum - 1)) * 1000;
            }

            Logging.LogWarning($@"Retry({retryNum}) after {waitTime} seconds.");

            await _delayer(waitTime);
            await SendBatchAsyncInternal(spanBatch, retryNum);
            return;
        }
    }
}
