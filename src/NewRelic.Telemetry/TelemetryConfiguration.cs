using Microsoft.Extensions.Configuration;

namespace NewRelic.Telemetry
{
    /// <summary>
    /// Configuration settings for the New Relic Telemetry SDK.
    /// </summary>
    public class TelemetryConfiguration
    {
        /// <summary>
        /// Requird: Your API Key.  This value is required in order to communicate with the
        /// New Relic Endpoint.
        /// </summary>
        public string ApiKey { get; private set; }

        /// <summary>
        /// The New Relic endpoint where Trace/Span information is sent.
        /// </summary>
        public string TraceUrl { get; private set; } = "https://trace-api.newrelic.com/trace/v1";

        /// <summary>
        /// Logs the messages sent-to and received-by the New Relic endpoint.  This setting
        /// is useful for troubleshooting, but is not recommended in production environments.
        /// </summary>
        public bool AuditLoggingEnabled { get; private set; } = false;

        /// <summary>
        /// The number of seconds that the data sender will wait for a response from
        /// a New Relic endpoint.  Requesets that exceed this limit will be assumed to have
        /// failed.
        /// </summary>
        public int SendTimeout { get; private set; } = 5;

        /// <summary>
        /// In the event of a failure, the DataSender will wait a certain amount of time (back-off) and retry.
        /// This setting indicates how many times the Data Sender will re-attempt to send information
        /// prior to failing.
        /// </summary>
        public int MaxRetryAttempts { get; private set; } = 8;

        /// <summary>
        /// Between each retry, the DataSender waits a certain amount of time.  This is a back-off period.
        /// This setting indicates the maximum wait time
        /// </summary>
        public int BackoffMaxSeconds { get; private set; } = 80;

        /// <summary>
        /// Each time the DataSender retries, it backs-off and waits for a longer period of time.
        /// The amount of time grows exponentially with each attempt until it exceeds the BackOffMaxSseconds.
        /// This setting identifies the factor by which the backoff is exponentially increased.
        /// </summary>
        /// <example>
        /// BackOffDelayFactorSeconds = 2.  Backoffs would be 2s (2^1), 4s (2^2), 8s (2^3), 16s (2^4), etc. 
        /// </example>
        public int BackoffDelayFactorSeconds { get; private set; } = 5;

        /// <summary>
        /// Identifies the service for which information is being reported to New Relic.
        /// </summary>
        public string ServiceName { get; private set; }


        /// <summary>
        /// A list of the New Relic endpoints where information is sent.  This collection may be used
        /// to filter our communications with New Relic when during analysis.
        /// </summary>
        public string[] NewRelicEndpoints => new []
        {
            TraceUrl
        };

        /// <summary>
        /// Creates the Configuration object accepting all default settings.
        /// </summary>
        public TelemetryConfiguration()
        {
        }

        /// <summary>
        /// Creates the Configuration object using a configuration provider as defined
        /// by Microsoft.Extensions.Configuration.
        /// </summary>
        /// <param name="configProvider"></param>
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

        /// <summary>
        /// Allows overriding the endpoint to which trace/span information is sent.
        /// This value should not be changed from the default unless you are in a test scenario.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public TelemetryConfiguration WithOverrideEndpointUrl_Trace(string url)
        {
            TraceUrl = url;
            return this;
        }
 
        /// <summary>
        /// Allows programmatic setting of the API key.  The API Key is required when communicating
        /// with the New Relic endpoints.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public TelemetryConfiguration WithAPIKey(string apiKey)
        {
            ApiKey = apiKey;
            return this;
        }

        /// <summary>
        /// Allows detailed logging of the information sent-to/received from New Relic endpoints.
        /// This setting is useful for testing and should not be enabled in production environments.
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public TelemetryConfiguration WithAuditLoggingEnabled(bool enabled)
        {
            AuditLoggingEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Sets how long the Data Sender will wait for a response from a New Relic endpoint before
        /// considering the request as timed-out.
        /// </summary>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        public TelemetryConfiguration WithSendTimeoutSeconds(int timeoutSeconds)
        {
            SendTimeout = timeoutSeconds;
            return this;
        }

        /// <summary>
        /// Identifies the number of times a request will be retried in the event of failure.
        /// </summary>
        /// <param name="retryAttempts"></param>
        /// <returns></returns>
        public TelemetryConfiguration WithMaxRetryAttempts(int retryAttempts)
        {
            MaxRetryAttempts = retryAttempts;
            return this;
        }

        /// <summary>
        /// Identifies the maximum amount of time that the DataSender will wait between
        /// retry attempts.
        /// </summary>
        /// <param name="backoffMaxSeconds"></param>
        /// <returns></returns>
        public TelemetryConfiguration WithBackoffMaxSeconds(int backoffMaxSeconds)
        {
            BackoffMaxSeconds = backoffMaxSeconds;
            return this;
        }

        /// <summary>
        /// Each time the DataSender retries, it backs-off and waits for a longer period of time.
        /// The amount of time grows exponentially with each attempt until it exceeds the BackOffMaxSseconds.
        /// This setting identifies the factor by which the backoff is exponentially increased.
        /// </summary>
        /// <example>
        /// BackOffDelayFactorSeconds = 2.  Backoffs would be 2s (2^1), 4s (2^2), 8s (2^3), 16s (2^4), etc. 
        /// </example>
        public TelemetryConfiguration WithBackoffDelayFactorSeconds(int delayFactorSeconds)
        {
            BackoffDelayFactorSeconds = delayFactorSeconds;
            return this;
        }

        /// <summary>
        /// Identifies the name of the service being reported on to the New Relic endpoints.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public TelemetryConfiguration WithServiceName(string serviceName)
        {
            ServiceName = serviceName;
            return this;
        }
    }
}
