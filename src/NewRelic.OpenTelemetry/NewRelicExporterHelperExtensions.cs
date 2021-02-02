// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
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
        /// <param name="configure">A method to configure the exporter options.</param>
        /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
        public static TracerProviderBuilder AddNewRelicExporter(this TracerProviderBuilder builder, Action<NewRelicExporterOptions> configure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var options = new NewRelicExporterOptions();
            configure?.Invoke(options);
            var exporter = new NewRelicTraceExporter(options);

            if (options.ExportProcessorType == ExportProcessorType.Simple)
            {
                return builder.AddProcessor(new SimpleActivityExportProcessor(exporter));
            }
            else
            {
                return builder.AddProcessor(new BatchActivityExportProcessor(
                    exporter,
                    options.BatchExportProcessorOptions.MaxQueueSize,
                    options.BatchExportProcessorOptions.ScheduledDelayMilliseconds,
                    options.BatchExportProcessorOptions.ExporterTimeoutMilliseconds,
                    options.BatchExportProcessorOptions.MaxExportBatchSize));
            }
        }
    }
}
