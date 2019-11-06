using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NewRelic.Telemetry.Sdk
{
    public class BatchDataSender
    {
        public string ApiKey { get; }
        public string EndpointUrl { get; }
        public bool AuditLoggingEnabled { get; }

        private const string _dataFormat = "newrelic";
        private const string _dataFormatVersion = "1";
        private const string _userAgent = "NewRelic-Dotnet-TelemetrySDK";
        private const string _sdkImplementationVersion = "/1.0.0";

        private HttpClient _httpClient;
        private Uri _uri;

        internal BatchDataSender(
          string apiKey, string endpointUrl, bool auditLoggingEnabled)
        {
            ApiKey = apiKey;
            EndpointUrl = endpointUrl;
            AuditLoggingEnabled = auditLoggingEnabled;

            _uri = new Uri(endpointUrl);
            _httpClient = new HttpClient();
            var sp = System.Net.ServicePointManager.FindServicePoint(_uri);
            sp.ConnectionLeaseTimeout = 5000;
        }

        public virtual async Task<HttpResponseMessage> SendBatch(string serializedPayload)
        {
            var serializedBytes = new UTF8Encoding().GetBytes(serializedPayload);

            using (var memoryStream = new MemoryStream())
            {
                var outStream = Compress(serializedBytes, memoryStream);

                StreamContent streamContent = new StreamContent(outStream);
                streamContent.Headers.Add("Content-Type", "application/json; charset=utf-8");
                streamContent.Headers.Add("Content-Encoding", "gzip");
                streamContent.Headers.ContentLength = outStream.Length;

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, _uri);
                requestMessage.Content = streamContent;
                requestMessage.Headers.Add("User-Agent", _userAgent + _sdkImplementationVersion);
                requestMessage.Headers.Add("Api-Key", ApiKey);
                requestMessage.Method = HttpMethod.Post;

                return await _httpClient.SendAsync(requestMessage);
            }
        }

        private MemoryStream Compress(byte[] bytes, MemoryStream memoryStream)
        {
            using (var gzipStream = new System.IO.Compression.GZipStream(memoryStream,
                System.IO.Compression.CompressionMode.Compress, true))
            {
                gzipStream.Write(bytes, 0, bytes.Length);
            }

            memoryStream.Position = 0;
            var compressedBytes = new byte[memoryStream.Length];
            memoryStream.Read(compressedBytes, 0, compressedBytes.Length);

            var outStream = new MemoryStream(compressedBytes);
            return outStream;
        }
    }
}
