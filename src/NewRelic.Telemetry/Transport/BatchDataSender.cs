using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NewRelic.Telemetry.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100198f2915b649f8774e7937c4e37e39918db1ad4e83109623c1895e386e964f6aa344aeb61d87ac9bd1f086a7be8a97d90f9ad9994532e5fb4038d9f867eb5ed02066ae24086cf8a82718564ebac61d757c9cbc0cc80f69cc4738f48f7fc2859adfdc15f5dde3e05de785f0ed6b6e020df738242656b02c5c596a11e628752bd0")]

// Created by Moq in SpanBatchSenderTests
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

namespace NewRelic.Telemetry.Transport
{
    internal interface IBatchDataSender
    {
        Task<HttpResponseMessage> SendBatchAsync(string serializedPayload);
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

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, _uri);
                requestMessage.Content = streamContent;
                requestMessage.Headers.Add("User-Agent", _userAgent + _implementationVersion);
                requestMessage.Headers.Add("Api-Key", ApiKey);
                requestMessage.Method = HttpMethod.Post;

                return await _httpClient.SendAsync(requestMessage);
            }
        }
    }
}
