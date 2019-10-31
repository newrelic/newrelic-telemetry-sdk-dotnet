using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

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

        public BatchDataSender(
          string apiKey, string endpointUrl, bool auditLoggingEnabled)
        {
            ApiKey = apiKey;
            EndpointUrl = endpointUrl;
            AuditLoggingEnabled = auditLoggingEnabled;
        }

        public virtual HttpResponseMessage SendBatch(string serializedPayload)
        {
            using (var client = new HttpClient())
            {
                var httpRequestMessage = new HttpRequestMessage();
                httpRequestMessage.Method = HttpMethod.Post;
                httpRequestMessage.RequestUri = new Uri(EndpointUrl);
                httpRequestMessage.Headers.Add("Api-Key", ApiKey);
                httpRequestMessage.Headers.Add("User-Agent", "NewRelic-Dotnet-TelemetrySDK/1");

                // TODO: should marshaller return bytes?
                // TODO: is compression dependent on length?
                // TODO: troubleshoot compression, currently sending uncompressed

                //var compressedPayload = Compress(serializedPayload);
                //httpRequestMessage.Content = compressedPayload;
                httpRequestMessage.Content = new StringContent(serializedPayload, Encoding.UTF8, "application/json");

                return client.SendAsync(httpRequestMessage).Result;
            }
        }

        private StringContent Compress(string data)
        {
            var bytes = new UTF8Encoding().GetBytes(data);
            using (var stream = new MemoryStream(bytes.Length))
            using (var outputStream = new GZipOutputStream(stream))
            {
                outputStream.Write(bytes, 0, bytes.Length);
                outputStream.Flush();
                outputStream.Finish();
                //                return new StreamContent(outputStream, (int)outputStream.Length);
                return new StringContent(outputStream.ToString(), Encoding.UTF8, "application/json");
            }
        }
    }
}
