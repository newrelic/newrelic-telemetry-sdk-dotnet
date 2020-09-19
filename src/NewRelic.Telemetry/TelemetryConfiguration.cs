// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.Configuration;

namespace NewRelic.Telemetry
{
    /// <summary>
    /// Configuration settings for the New Relic Telemetry SDK.
    /// </summary>
    public class TelemetryConfiguration
    {
        /// <summary>
        /// REQUIRED: Your Insights Insert API Key.  This value is required in order to communicate with the
        /// New Relic Endpoint. 
        /// </summary>
        /// <see cref="https://docs.newrelic.com/docs/insights/insights-data-sources/custom-data/introduction-event-api#register">for more information.</see>
        public string? ApiKey { get; set; }

        /// <summary>
        /// The New Relic endpoint where Trace/Span information is sent.
        /// </summary>
        public string TraceUrl { get; set; } = "https://trace-api.newrelic.com/trace/v1";

        /// <summary>
        /// The New Relic endpoint where Metric information is sent.
        /// </summary>
        public string MetricUrl { get; set; } = "https://metric-api.newrelic.com/metric/v1";

        /// <summary>
        /// Logs messages sent-to and received-by the New Relic endpoints.  This setting
        /// is useful for troubleshooting, but is not recommended in production environments.
        /// </summary>
        public bool AuditLoggingEnabled { get; set; } = false;

        /// <summary>
        /// The number of seconds that the DataSender will wait for a response from
        /// a New Relic endpoint.  Requests that exceed this limit will be assumed to have
        /// failed.
        /// </summary>
        public int SendTimeout { get; set; } = 5;

        /// <summary>
        /// In the event of a failure, the DataSender will wait a certain amount of time (back-off) and retry.
        /// This setting indicates how many times the DataSender will re-attempt to send information
        /// prior to failing.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 8;

        /// <summary>
        /// Between each retry, the DataSender waits a variable amount of time.  This is a back-off period.
        /// This setting indicates the maximum wait time for a back-off.
        /// </summary>
        public int BackoffMaxSeconds { get; set; } = 80;

        /// <summary>
        /// Each time the DataSender retries, it backs-off and waits for a longer period of time.
        /// The amount of time grows exponentially with each attempt until it exceeds the <see cref="BackoffMaxSeconds"/>.
        /// This setting identifies the factor by which the backoff is exponentially increased.
        /// </summary>
        /// <example>
        /// With a BackOffDelayFactorSeconds of 5 and BackoffMaxSeconds of 80. 
        /// Backoffs would be 5s (5^1), 25s (5^2), 80s (2^3=125 -> 80), 80s (2^4 = 625 -> 80), etc. 
        /// </example>
        public int BackoffDelayFactorSeconds { get; set; } = 5;

        /// <summary>
        /// Identifies the name of a service for which information is being reported to New Relic.
        /// </summary>
        public string? ServiceName { get; set; }

        /// <summary>
        /// Identifies the source of information that is being sent to New Relic.
        /// </summary>
        public string? InstrumentationProvider { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryConfiguration"/> class.
        /// Creates the Configuration object accepting all default settings.
        /// </summary>
        public TelemetryConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryConfiguration"/> class.
        /// Constructs a new configuration object using a configuration provider. Allows for an overall
        /// New Relic Value
        /// by <see cref="Microsoft.Extensions.Configuration">Microsoft.Extensions.Configuration</see>.
        /// </summary>
        /// <param name="configProvider"></param>
        public TelemetryConfiguration(IConfiguration configProvider)
            : this(configProvider, null)
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryConfiguration"/> class.
        /// Constructs a new configuration object using a configuration provider. Allows for an overall
        /// New Relic Value and can be overriden with a Product Specific Value.
        /// by <see cref="Microsoft.Extensions.Configuration">Microsoft.Extensions.Configuration</see>.
        /// </summary>
        /// <param name="configProvider"></param>
        public TelemetryConfiguration(IConfiguration configProvider, string? productSpecificConfig)
        {
            var newRelicConfigSection = configProvider
                .GetSection("NewRelic");

            if (newRelicConfigSection == null)
            {
                return;
            }

            IConfigurationSection? productConfigSection = null;
            if (!string.IsNullOrWhiteSpace(productSpecificConfig))
            {
                productConfigSection = newRelicConfigSection.GetSection(productSpecificConfig);
            }

            ApiKey = GetValueString("ApiKey", productConfigSection, newRelicConfigSection) ?? ApiKey;
            ServiceName = GetValueString("ServiceName", productConfigSection, newRelicConfigSection) ?? ServiceName;
            TraceUrl = GetValueString("TraceUrlOverride", productConfigSection, newRelicConfigSection) ?? TraceUrl;
            MetricUrl = GetValueString("MetricUrlOverride", productConfigSection, newRelicConfigSection) ?? MetricUrl;
            AuditLoggingEnabled = GetValueBool("AuditLoggingEnabled", productConfigSection, newRelicConfigSection) ?? AuditLoggingEnabled;
            SendTimeout = GetValueInt("SendTimeoutSeconds", productConfigSection, newRelicConfigSection) ?? SendTimeout;
            MaxRetryAttempts = GetValueInt("MaxRetryAttempts", productConfigSection, newRelicConfigSection) ?? MaxRetryAttempts;
            BackoffMaxSeconds = GetValueInt("BackoffMaxSeconds", productConfigSection, newRelicConfigSection) ?? BackoffMaxSeconds;
            BackoffDelayFactorSeconds = GetValueInt("BackoffDelayFactorSeconds", productConfigSection, newRelicConfigSection) ?? BackoffDelayFactorSeconds;
        }

        private string? GetValueString(string key, IConfigurationSection? productConfigSection, IConfigurationSection newRelicConfigSection)
        {
            return productConfigSection?[key] ?? newRelicConfigSection[key];
        }

        private bool? GetValueBool(string key, IConfigurationSection? productConfigSection, IConfigurationSection newRelicConfigSection)
        {
            string valStr = productConfigSection?[key] ?? newRelicConfigSection[key];
            if (!string.IsNullOrEmpty(valStr) && bool.TryParse(valStr, out var valBool))
            {
                return valBool;
            }

            return null;
        }

        private int? GetValueInt(string key, IConfigurationSection? productConfigSection, IConfigurationSection newRelicConfigSection)
        {
            string valStr = productConfigSection?[key] ?? newRelicConfigSection[key];
            if (!string.IsNullOrEmpty(valStr) && int.TryParse(valStr, out var valInt))
            {
                return valInt;
            }

            return null;
        }
    }
}
