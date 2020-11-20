// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NewRelic.OpenTelemetry;

namespace OpenTelemetry.Trace
{
    /// <summary>
    /// Extension methods to help instantiate and configure the New Relic data exporter.
    /// </summary>
    public static class NewRelicExporterHelperExtensions
    {
        /// <summary>
        /// Adds New Relic exporter to the TracerProvider.
        /// </summary>
        /// <param name="builder"><see cref="TracerProviderBuilder"/> builder to use.</param>
        /// <param name="options">Exporter configuration options.</param>
        /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
        public static TracerProviderBuilder AddNewRelicExporter(this TracerProviderBuilder builder, Action<NewRelicExporterOptions> options)
        {
            return AddNewRelicExporter(builder, options, null!);
        }

        /// <summary>
        /// Adds New Relic exporter to the TracerProvider and enables the exporter to log to an ILogger.
        /// </summary>
        /// <param name="builder"><see cref="TracerProviderBuilder"/> builder to use.</param>
        /// <param name="configure">Exporter configuration options.</param>
        /// <param name="loggerFactory">ILoggerFactory instance for creating an ILogger.</param>
        /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
        public static TracerProviderBuilder AddNewRelicExporter(this TracerProviderBuilder builder, Action<NewRelicExporterOptions> configure, ILoggerFactory loggerFactory)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var options = new NewRelicExporterOptions();
            configure?.Invoke(options);
            var exporter = new NewRelicTraceExporter(options, loggerFactory);

            if (options.ExportProcessorType == ExportProcessorType.Simple)
            {
                return builder.AddProcessor(new SimpleExportProcessor<Activity>(exporter));
            }
            else
            {
                return builder.AddProcessor(new BatchExportProcessor<Activity>(
                    exporter,
                    options.BatchExportProcessorOptions.MaxQueueSize,
                    options.BatchExportProcessorOptions.ScheduledDelayMilliseconds,
                    options.BatchExportProcessorOptions.ExporterTimeoutMilliseconds,
                    options.BatchExportProcessorOptions.MaxExportBatchSize));
            }
        }
    }
}
