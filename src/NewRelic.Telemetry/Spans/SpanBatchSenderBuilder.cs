using System;
using NewRelic.Telemetry.Transport;

namespace NewRelic.Telemetry.Spans
{
    public class SpanBatchSenderBuilder
    {
        private string _apiKey;
        private bool _auditLoggingEnabled = false;

        public string TraceUrl { get; private set; } = "https://trace-api.newrelic.com/trace/v1";

        public static SpanBatchSenderBuilder Create()
        {
            return new SpanBatchSenderBuilder();
        }

        public SpanBatchSender Build()
        {
            if (_apiKey == null)
            {
                throw new ArgumentNullException("apiKey");
            }


            IBatchDataSender sender = new BatchDataSender(_apiKey, TraceUrl, _auditLoggingEnabled, TimeSpan.FromSeconds(5));

            return new SpanBatchSender(sender);
        }

        public SpanBatchSenderBuilder WithUrlOverride(string urlOverride)
        {
            TraceUrl = urlOverride;
            return this;
        }

        public SpanBatchSenderBuilder WithApiKey(string apiKey)
        {
            _apiKey = apiKey;
            return this;
        }

        public SpanBatchSenderBuilder WithAuditLoggingEnabled()
        {
            _auditLoggingEnabled = true;
            return this;
        }
    }
 }
