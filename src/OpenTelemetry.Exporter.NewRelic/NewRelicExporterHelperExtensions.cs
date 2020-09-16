// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

﻿using System;
using OpenTelemetry.Exporter.NewRelic;

namespace OpenTelemetry.Trace
{
    /// <summary>
    /// Extension methods to help instantiate and configure the New Relic data exporter.
    /// </summary>
    public static class NewRelicExporterHelperExtensions
    {
        /// <summary>
        /// Advanced Configuration the New Relic Data Exporter providing configuration provider and a logger factory
        /// factory.
        /// </summary>
        /// <param name="builder"><see cref="TracerProviderBuilder"/> builder to use.</param>
        /// <param name="configure">Exporter configuration options.</param>
        /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
        public static TracerProviderBuilder AddNewRelicExporter(this TracerProviderBuilder builder, Action<NewRelicExporterOptions>? configure = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var exporterOptions = new NewRelicExporterOptions();
            configure?.Invoke(exporterOptions);
            var newRelicExporter = new NewRelicTraceExporter(exporterOptions);

            return builder.AddProcessor(new BatchExportActivityProcessor(newRelicExporter));
        }
    }
}
