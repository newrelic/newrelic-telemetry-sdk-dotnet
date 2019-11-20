using Microsoft.Extensions.Configuration;

namespace NewRelic.Telemetry
{
    public static class Configuration
    {
        private static IConfiguration _configuration;

        internal static string TraceUrl { get; private set; } = "https://trace-api.newrelic.com/trace/v1";

        internal static string ApiKey { get; private set; }

        internal static bool AuditLoggingEnabled { get; private set; } = false;

        public static IConfiguration Config
        {
            set
            {
                _configuration = value;

                // TODO: is there a better place to set these properties?
                string overrideUrl;
                if (!string.IsNullOrEmpty(overrideUrl = _configuration["Newrelic.Telemetry.OverrideTraceUrl"]))
                {
                    TraceUrl = overrideUrl;
                }

                ApiKey = _configuration["Newrelic.Telemetry.ApiKey"];

                string auditLoggingEnabled;
                 if (!string.IsNullOrEmpty(auditLoggingEnabled = _configuration["Newrelic.Telemetry.AuditLoggingEnabled"]))
                {
                    AuditLoggingEnabled = bool.Parse(auditLoggingEnabled);
                }
            }
        }
    }
}
