using NewRelic.Telemetry;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace.Configuration;

namespace OpenTelemetry.Exporter.NewRelic
{
    public static class NewRelicOpenTelemetryExtensions
    {
        public static TracerBuilder UseNewRelic(this TracerBuilder builder, TelemetryConfiguration config, ILoggerFactory loggerFactory)
        {
            builder.AddProcessorPipeline(c => c.SetExporter(new NewRelicTraceExporter(config, loggerFactory)));
            return builder;
        }

        public static TracerBuilder UseNewRelic(this TracerBuilder builder, TelemetryConfiguration config)
        {
            return UseNewRelic(builder, config, null);
        }

        public static TracerBuilder UseNewRelic(this TracerBuilder builder, string apiKey)
        {
            return UseNewRelic(builder, apiKey, null);
        }

        public static TracerBuilder UseNewRelic(this TracerBuilder builder, string apiKey, ILoggerFactory loggerFactory)
        {
            builder.AddProcessorPipeline(c => c.SetExporter(new NewRelicTraceExporter(new TelemetryConfiguration().WithAPIKey(apiKey),loggerFactory)));
            return builder;
        }
    }
}
