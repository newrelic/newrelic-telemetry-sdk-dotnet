using System;
using NewRelic.Telemetry.Transport;

namespace NewRelic.Telemetry.Spans
{
    public class SpanBatchSenderBuilder
    {
        private string _traceUrl = "https://trace-api.newrelic.com/trace/v1";
        private string _apiKey;
        private bool _auditLoggingEnabled = false;
        
        public SpanBatchSender Build()
        {
            if (_apiKey == null)
            {
                throw new ArgumentNullException("apiKey");
            }

            IBatchDataSender sender = new BatchDataSender(_apiKey, _traceUrl, _auditLoggingEnabled);
            return new SpanBatchSender(sender);
        }

        public SpanBatchSenderBuilder UrlOverride(string urlOverride)
        {
            _traceUrl = urlOverride;
            return this;
        }

        public SpanBatchSenderBuilder ApiKey(string apiKey)
        {
            _apiKey = apiKey;
            return this;
        }

        public SpanBatchSenderBuilder EnableAuditLogging()
        {
            _auditLoggingEnabled = true;
            return this;
        }
    }
 }
