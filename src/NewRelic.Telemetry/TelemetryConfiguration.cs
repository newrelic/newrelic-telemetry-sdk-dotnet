using Microsoft.Extensions.Configuration;

namespace NewRelic.Telemetry
{
    // TOODO:
    //      2.  Inject Logging Factory (Replace Logging Singleton?)
    //      3.  Remove the Metrics Example (config too)
    //      4.  See if we can preserve GZIP/JSON on retry (STRETCH)
    //      5.  Need to determine if we should throw exception on NULL api key or just return response.failure.
    //      6.  More info on the NR response (response code and content)
    //      7.  HttpClient - should we dispose it? (strange case of IDisposable - read about it online for the reuse-usecase)
    //      8.  Lazy Instantiate the HttpClient on first use.
    //      9.  Clean up templated code in the Test CustomLogger

    public class TelemetryConfiguration
    {
        public string TraceUrl { get; private set; } = "https://trace-api.newrelic.com/trace/v1";
        public string MetricsUrl { get; private set; } = "https://trace-api.newrelic.com/metrics/v1";
        public string ApiKey { get; private set; }
        public bool AuditLoggingEnabled { get; private set; } = false;
        public int SendTimeout { get; private set; } = 5;
        public int MaxRetryAttempts { get; private set; } = 8;
        public int BackoffMaxSeconds { get; private set; } = 80;
        public int BackoffDelayFactorSeconds { get; private set; } = 5;

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
        }

        public TelemetryConfiguration WithEndpointURL_Trace(string url)
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
    }
}
