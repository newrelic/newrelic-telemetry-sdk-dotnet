﻿// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.Logging;
using NewRelic.Telemetry;
using OpenTelemetry.Exporter.NewRelic;

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
        /// <param name="config">Exporter configuration options.</param>
        /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
        public static TracerProviderBuilder UseNewRelic(this TracerProviderBuilder builder, TelemetryConfiguration config)
        {
            return UseNewRelic(builder, config, null);
        }

        /// <summary>
        /// Adds New Relic exporter to the TracerProvider and enables the exporter to log to an ILogger.
        /// </summary>
        /// <param name="builder"><see cref="TracerProviderBuilder"/> builder to use.</param>
        /// <param name="config">Exporter configuration options.</param>
        /// <param name="loggerFactory">ILoggerFactory instance for creating an ILogger.</param>
        /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
        public static TracerProviderBuilder UseNewRelic(this TracerProviderBuilder builder, TelemetryConfiguration config, ILoggerFactory loggerFactory)
        {
            builder.AddProcessor(new BatchExportActivityProcessor(new NewRelicTraceExporter(config, loggerFactory)));
            return builder;
        }
    }
}
