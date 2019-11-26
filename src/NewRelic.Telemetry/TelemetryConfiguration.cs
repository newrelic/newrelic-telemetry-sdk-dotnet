using Microsoft.Extensions.Configuration;

namespace NewRelic.Telemetry
{
    public class TelemetryConfiguration
    {
        public string ApiKey { get; private set; }

        public string TraceUrl { get; private set; } = "https://trace-api.newrelic.com/trace/v1";

        public bool AuditLoggingEnabled { get; private set; } = false;

        public int SendTimeout { get; private set; } = 5;
        
        public int MaxRetryAttempts { get; private set; } = 8;

        public int BackoffMaxSeconds { get; private set; } = 80;

        public int BackoffDelayFactorSeconds { get; private set; } = 5;

        public string ServiceName { get; private set; }

        public string[] NewRelicEndpoints => new []
        {
            TraceUrl
        };

        public TelemetryConfiguration()
        {
        }

        public TelemetryConfiguration(IConfiguration configProvider)
        {
            string overrideUrl;
            if (!string.IsNullOrEmpty(overrideUrl = configProvider["Newrelic.Telemetry.OverrideTraceUrl"]))
            {
                TraceUrl = overrideUrl;
            }

            string apiKey;
            if (!string.IsNullOrEmpty(apiKey = configProvider["Newrelic.Telemetry.ApiKey"]))
            {
                ApiKey = apiKey;
            }

            string auditLoggingEnabledStr;
            if (!string.IsNullOrEmpty(auditLoggingEnabledStr = configProvider["Newrelic.Telemetry.AuditLoggingEnabled"]))
            {
                if (bool.TryParse(auditLoggingEnabledStr, out var auditLoggingEnabled))
                {
                    AuditLoggingEnabled = auditLoggingEnabled;
                }
            }

            string sendTimeoutStr;
            if (!string.IsNullOrEmpty(sendTimeoutStr = configProvider["Newrelic.Telemetry.SendTimeoutSeconds"]))
            {
                if (int.TryParse(sendTimeoutStr, out var sendTimeoutEnabled))
                {
                    SendTimeout = sendTimeoutEnabled;
                }
            }

            string maxRetryAttemptsStr;
            if (!string.IsNullOrEmpty(maxRetryAttemptsStr = configProvider["Newrelic.Telemetry.MaxRetryAttempts"]))
            {
                if (int.TryParse(maxRetryAttemptsStr, out var maxRetryAttempts))
                {
                    MaxRetryAttempts = maxRetryAttempts;
                }
            }


            string backoffMaxSecondsStr;
            if (!string.IsNullOrEmpty(backoffMaxSecondsStr = configProvider["Newrelic.Telemetry.BackoffMaxSeconds"]))
            {
                if (int.TryParse(backoffMaxSecondsStr, out var backoffMaxSeconds))
                {
                    BackoffMaxSeconds = backoffMaxSeconds;
                }
            }

            string backoffDelayFactorSecondsStr;
            if (!string.IsNullOrEmpty(backoffDelayFactorSecondsStr = configProvider["Newrelic.Telemetry.BackoffDelayFactorSeconds"]))
            {
                if (int.TryParse(backoffDelayFactorSecondsStr, out var backoffDelayFactorSeconds))
                {
                    BackoffDelayFactorSeconds = backoffDelayFactorSeconds;
                }
            }

            string serviceName;
            if (!string.IsNullOrEmpty(serviceName = configProvider["Newrelic.Telemetry.ServiceName"]))
            {
                ServiceName = serviceName;
            }
        }

        public TelemetryConfiguration WithOverrideEndpointUrl_Trace(string url)
        {
            TraceUrl = url;
            return this;
        }
 
        public TelemetryConfiguration WithAPIKey(string apiKey)
        {
            ApiKey = apiKey;
            return this;
        }

        public TelemetryConfiguration WithAuditLoggingEnabled(bool enabled)
        {
            AuditLoggingEnabled = enabled;
            return this;
        }

        public TelemetryConfiguration WithSendTimeoutSeconds(int timeoutSeconds)
        {
            SendTimeout = timeoutSeconds;
            return this;
        }

        public TelemetryConfiguration WithMaxRetryAttempts(int retryAttempts)
        {
            MaxRetryAttempts = retryAttempts;
            return this;
        }

        public TelemetryConfiguration WithBackoffMaxSeconds(int backoffMaxSeconds)
        {
            BackoffMaxSeconds = backoffMaxSeconds;
            return this;
        }

        public TelemetryConfiguration WithBackoffDelayFactorSeconds(int delayFactorSeconds)
        {
            BackoffDelayFactorSeconds = delayFactorSeconds;
            return this;
        }

        public TelemetryConfiguration WithServiceName(string serviceName)
        {
            ServiceName = serviceName;
            return this;
        }
    }
}
