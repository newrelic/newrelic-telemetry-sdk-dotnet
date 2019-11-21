using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NewRelic.Telemetry.Transport
{
    internal interface IBatchDataSender
    {
        Task<HttpResponseMessage> SendBatchAsync(string serializedPayload);
        string EndpointUrl { get; }
    }

    internal class BatchDataSender : IBatchDataSender
    {
        public string ApiKey { get; }
        public string EndpointUrl { get; }
        public bool AuditLoggingEnabled { get; }

        private const string _dataFormat = "newrelic";
        private const string _dataFormatVersion = "1";
        private const string _userAgent = "NewRelic-Dotnet-TelemetrySDK";
        private const string _implementationVersion = "/1.0.0";

        private HttpClient _httpClient;

        public Uri EndpointUri { get; private set; }

        internal BatchDataSender(string apiKey, string endpointUrl, bool auditLoggingEnabled, TimeSpan timeout)
        {
            ApiKey = apiKey;
            EndpointUrl = endpointUrl;
            AuditLoggingEnabled = auditLoggingEnabled;

            EndpointUri = new Uri(endpointUrl);
            _httpClient = new HttpClient();
            _httpClient.Timeout = timeout;
            var sp = System.Net.ServicePointManager.FindServicePoint(EndpointUri);
            sp.ConnectionLeaseTimeout = 60000;  // 1 minute
        }

        public async Task<HttpResponseMessage> SendBatchAsync(string serializedPayload)
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

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, EndpointUri);
                requestMessage.Content = streamContent;
                requestMessage.Headers.Add("User-Agent", _userAgent + _implementationVersion);
                requestMessage.Headers.Add("Api-Key", ApiKey);
                requestMessage.Method = HttpMethod.Post;

                var response = await _httpClient.SendAsync(requestMessage);

                if (AuditLoggingEnabled)
                {
                    Logging.LogDebug($@"Sent payload: '{serializedPayload}'");
                }

                return response;
            }
        }
    }
}
