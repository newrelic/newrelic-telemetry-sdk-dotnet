using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NewRelic.Telemetry.Transport
{
    public abstract class DataSender<TData> where TData : ITelemetryDataType
    {
        private const string _userAgent = "NewRelic-Dotnet-TelemetrySDK";
        private const string _implementationVersion = "/1.0.0";

        protected readonly TelemetryConfiguration _config;
        protected readonly TelemetryLogging _logger;
        private readonly HttpClient _httpClient;

        //Delegate functions in support of testing
        private Func<string, Task<HttpResponseMessage>> _httpHandlerImpl;
        private Func<int, Task> _delayerImpl = new Func<int, Task>(async (int milliseconds) => await Task.Delay(milliseconds));
        private Action<TData, int> _captureSendDataAsyncCallDelegate = null;


        protected abstract string EndpointUrl { get; }

        protected abstract TData[] Split(TData dataToSplit);

        protected abstract bool ContainsNoData(TData dataToCheck);

        internal DataSender<TData> WithDelayFunction(Func<int, Task> delayerImpl)
        {
            _delayerImpl = delayerImpl;
            return this;
        }

        internal void WithHttpHandlerImpl(Func<string, Task<HttpResponseMessage>> httpHandler)
        {
            _httpHandlerImpl = httpHandler;
        }

        internal void WithCaptureSendDataAsyncDelegate(Action<TData, int> captureTestDataImpl)
        {
            _captureSendDataAsyncCallDelegate = captureTestDataImpl;
        }

        protected DataSender(IConfiguration configProvider) : this(configProvider, null)
        {
        }

        protected DataSender(IConfiguration configProvider, ILoggerFactory loggerFactory) : this(new TelemetryConfiguration(configProvider), loggerFactory)
        {
        }

        
        protected DataSender(TelemetryConfiguration config) : this(config, null)
        {
            _config = config;

            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(_config.SendTimeout);

            //Ensures that DNS expires regularly.
            var sp = ServicePointManager.FindServicePoint(new Uri(EndpointUrl));
            sp.ConnectionLeaseTimeout = 60000;  // 1 minute

            _httpHandlerImpl = SendDataAsync;
        }

        protected DataSender(TelemetryConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = new TelemetryLogging(loggerFactory);
        }

        private async Task<Response> RetryWithSplit(TData data)
        {
            var newBatches = Split(data);

            if (newBatches == null)
            {
                _logger.Error($@"Cannot send data because it exceeds the size limit and cannot be split.");
                return Response.Failure(HttpStatusCode.RequestEntityTooLarge,"Cannot send data because it exceeds size limit and cannot be further split.");
            }

            _logger.Warning("Splitting the data and retrying.");

            var taskList = new Task<Response>[newBatches.Length];

            for (var i = 0; i < newBatches.Length; i++)
            {
                taskList[i] = SendDataAsync(newBatches[i]);
            }
            
            var responses = await Task.WhenAll(taskList);

            if(responses.All(x=>x.ResponseStatus == NewRelicResponseStatus.Success))
            {
                return Response.Success;
            }

            return Response.Failure(HttpStatusCode.Ambiguous, $"{responses.Count(x=>x.ResponseStatus != NewRelicResponseStatus.Success)} of {responses.Length} requests were NOT successful.");
        }
 
        private async Task<Response> RetryWithDelay(TData data, int retryNum, int? waitTimeInSeconds = null)
        {
            retryNum++;
            if (retryNum > _config.MaxRetryAttempts)
            {
                _logger.Error($@"Send Data failed after {_config.MaxRetryAttempts} attempts.");
                return Response.Failure(HttpStatusCode.RequestTimeout, $"Send Data failed after {_config.MaxRetryAttempts} attempts");
            }

            waitTimeInSeconds = waitTimeInSeconds ?? (int)Math.Min(_config.BackoffMaxSeconds, _config.BackoffDelayFactorSeconds * Math.Pow(2, retryNum - 1));

            _logger.Warning($@"Attempting retry({retryNum}) after {waitTimeInSeconds} seconds.");

            await _delayerImpl(waitTimeInSeconds.Value * 1000);

            var result = await SendDataAsync(data, retryNum);

            return result;
        }

        private async Task<Response> RetryWithServerDelay(TData dataToSend, int retryNum, HttpResponseMessage httpResponse)
        {
            retryNum++;
            if (retryNum > _config.MaxRetryAttempts)
            {
                _logger.Error($@"Send Data failed after {_config.MaxRetryAttempts} attempts.");
                return Response.Failure(HttpStatusCode.RequestTimeout, $"Send Data failed after {_config.MaxRetryAttempts} attempts");
            }

            var retryAfterDelay = httpResponse.Headers?.RetryAfter?.Delta;
            var retryAtSpecificDate = httpResponse.Headers?.RetryAfter?.Date;

            if (!retryAfterDelay.HasValue && retryAtSpecificDate.HasValue)
            {
                retryAfterDelay = retryAtSpecificDate - DateTimeOffset.UtcNow;
            }

            var delayMs = (int)retryAfterDelay.Value.TotalMilliseconds;

            //Perform the delay using the waiter delegate
            await _delayerImpl(delayMs);

            return await SendDataAsync(dataToSend, retryNum);
        }

        /// <summary>
        /// Method used to send a data to New Relic.  Handles the communication with the New Relic endpoints
        /// </summary>
        /// <param name="dataToSend">The data to send to New Relic</param>
        /// <returns>New Relic response indicating the outcome and additional information about the interaction with the New Relic endpoint.</returns>
        public async Task<Response> SendDataAsync(TData dataToSend)
        {
            if(string.IsNullOrWhiteSpace(_config.ApiKey))
            {
                _logger.Exception(new ArgumentNullException("Configuration requires API key"));
                return Response.Failure("API Key was not available");
            }

            return await SendDataAsync(dataToSend, 0);
        }

        private async Task<Response> SendDataAsync(TData dataToSend, int retryNum)
        {

            HttpResponseMessage httpResponse;

            try
            {
                _captureSendDataAsyncCallDelegate?.Invoke(dataToSend, retryNum);

                if (ContainsNoData(dataToSend))
                {
                    return Response.DidNotSend;
                }

                var serializedPayload = dataToSend.ToJson();

                httpResponse = await _httpHandlerImpl(serializedPayload);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex.InnerException ?? ex);
                return Response.Exception(ex.InnerException ?? ex);
            }

            switch (httpResponse.StatusCode)
            {
                //Success is any 2xx response
                case HttpStatusCode code when code >= HttpStatusCode.OK && code <= (HttpStatusCode)299:
                    _logger.Debug($@"Response from New Relic ingest API: code: {httpResponse.StatusCode}");
                    return Response.Success;

                case HttpStatusCode.RequestEntityTooLarge:
                    _logger.Warning($@"Response from New Relic ingest API: code: {httpResponse.StatusCode}. Response indicates payload is too large.");
                    return await RetryWithSplit(dataToSend);

                case HttpStatusCode.RequestTimeout:
                    _logger.Warning($@"Response from New Relic ingest API: code: {httpResponse.StatusCode}");
                    return await RetryWithDelay(dataToSend, retryNum);

                case (HttpStatusCode)429:
                    _logger.Warning($@"Response from New Relic ingest API: code: {httpResponse.StatusCode}. ");
                    return await RetryWithServerDelay(dataToSend, retryNum, httpResponse);

                //Anything else is interpreted as a failure condition.  No further attempts are made.
                default:
                    _logger.Error($@"Response from New Relic ingest API: code: {httpResponse.StatusCode}");
                    return Response.Failure(httpResponse.StatusCode, httpResponse.Content?.ToString());
            }
        }

        private async Task<HttpResponseMessage> SendDataAsync(string serializedPayload)
        {
            var serializedBytes = new UTF8Encoding().GetBytes(serializedPayload);

            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    gzipStream.Write(serializedBytes, 0, serializedBytes.Length);
                }

                memoryStream.Position = 0;

                var streamContent = new StreamContent(memoryStream);
                streamContent.Headers.Add("Content-Type", "application/json; charset=utf-8");
                streamContent.Headers.Add("Content-Encoding", "gzip");
                streamContent.Headers.ContentLength = memoryStream.Length;

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, EndpointUrl);
                requestMessage.Content = streamContent;
                requestMessage.Headers.Add("User-Agent", _userAgent + _implementationVersion);
                requestMessage.Headers.Add("Api-Key", _config.ApiKey);
                requestMessage.Method = HttpMethod.Post;

                var response = await _httpClient.SendAsync(requestMessage);

                if (_config.AuditLoggingEnabled)
                {
                    _logger.Debug($@"Sent payload: '{serializedPayload}'");
                }

                return response;
            }
        }
    }
}
