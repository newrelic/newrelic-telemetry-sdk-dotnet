using NewRelic.Telemetry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Trace.Export;
using OpenTelemetry.Trace.Configuration;

namespace OpenTelemetry.Exporter.NewRelic
{
    /// <summary>
    /// Extension methods to help instantiate and configure the New Relic data exporter.
    /// </summary>
    public static class NewRelicOpenTelemetryExtensions
    {

        /// <summary>
        /// Advanced Configuration the New Relic Data Exporter providing configuration provider and a logger factory
        /// factory.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configProvider"></param>
        /// <param name="loggerFactory">Logger Factory supported by Microsoft.Extensions.Logging</param>
        /// <returns></returns>
        public static TracerBuilder UseNewRelic(this TracerBuilder builder, IConfiguration configProvider, ILoggerFactory loggerFactory)
        {
            builder.AddProcessorPipeline(c => c.SetExporter(new NewRelicTraceExporter(configProvider, loggerFactory)));
            return builder;
        }

        /// <summary>
        /// Advanced Configuration the New Relic Data Exporter that is configured using a Configuration Provider.
        /// factory.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configProvider"></param>
        /// <returns></returns>
        public static TracerBuilder UseNewRelic(this TracerBuilder builder, IConfiguration configProvider)
        {
            return UseNewRelic(builder, configProvider, null);
        }

        /// <summary>
        /// Advanced Configuration the New Relic Data Exporter that is configured using an instance of TelemetryConfiguration and a Logger Factory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="config"></param>
        /// <param name="loggerFactory">Logger Factory supported by Microsoft.Extensions.Logging</param>
        /// <returns></returns>
        public static TracerBuilder UseNewRelic(this TracerBuilder builder, TelemetryConfiguration config, ILoggerFactory loggerFactory)
        {
            builder.AddProcessorPipeline(c => c.SetExporter(new NewRelicTraceExporter(config, loggerFactory))
                                               .SetExportingProcessor(e => new BatchingSpanProcessor(e)));
            return builder;
        }

        /// <summary>
        /// Advanced Configuration of the New Relic Data Exporter providing an instance of TelemetryConfiguration settings without logging.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TracerBuilder UseNewRelic(this TracerBuilder builder, TelemetryConfiguration config)
        {
            return UseNewRelic(builder, config, null);
        }

        /// <summary>
        /// Configure the New Relic Data Exporter with default settings.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static TracerBuilder UseNewRelic(this TracerBuilder builder, string apiKey)
        {
            return UseNewRelic(builder, apiKey, null);
        }

        /// <summary>
        /// Configure the New Relic Data Exporter with default settings providing a logger factory.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static TracerBuilder UseNewRelic(this TracerBuilder builder, string apiKey, ILoggerFactory loggerFactory)
        {
            return UseNewRelic(builder, new TelemetryConfiguration().WithAPIKey(apiKey), loggerFactory);
        }
    }
}
