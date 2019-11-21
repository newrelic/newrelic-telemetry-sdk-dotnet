using Microsoft.Extensions.Configuration;

namespace NewRelic.Telemetry
{
    public class Configuration
    {
        private static IConfiguration _configuration;

        public static string TraceUrl { get; internal set; } = "https://trace-api.newrelic.com/trace/v1";

        public static string ApiKey { get; internal set; }

        public static bool AuditLoggingEnabled { get; internal set; } = false;

        public IConfiguration Config
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
