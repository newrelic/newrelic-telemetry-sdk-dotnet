﻿// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using NewRelic.Telemetry;
using OpenTelemetry;

namespace NewRelic.OpenTelemetry
{
    /// <summary>
    /// New Relic Exporter options.
    /// </summary>
    public class NewRelicExporterOptions
    {
        /// <summary>
        /// REQUIRED: Your Insights Insert API Key.  This value is required in order to communicate with the
        /// New Relic Endpoint. 
        /// </summary>
        /// <see cref="https://docs.newrelic.com/docs/insights/insights-data-sources/custom-data/introduction-event-api#register">for more information.</see>
        public string? ApiKey
        {
            get => TelemetryConfiguration.ApiKey;
            set => TelemetryConfiguration.ApiKey = value;
        }

        /// <summary>
        /// The New Relic endpoint where information is sent.
        /// </summary>
        public Uri EndpointUrl
        {
            get => TelemetryConfiguration.TraceUrl;
            set => TelemetryConfiguration.TraceUrl = value;
        }

        /// <summary>
        /// Logs messages sent-to and received-by the New Relic endpoints.  This setting
        /// is useful for troubleshooting, but is not recommended in production environments.
        /// </summary>
        public bool AuditLoggingEnabled
        {
            get => TelemetryConfiguration.AuditLoggingEnabled;
            set => TelemetryConfiguration.AuditLoggingEnabled = value;
        }

        /// <summary>
        /// Identifies the name of a service for which information is being reported to New Relic.
        /// </summary>
        public string? ServiceName
        {
            get => TelemetryConfiguration.ServiceName;
            set => TelemetryConfiguration.ServiceName = value;
        }

        /// <summary>
        /// Gets or sets the export processor type to be used.
        /// </summary>
        public ExportProcessorType ExportProcessorType { get; set; } = ExportProcessorType.Batch;

        /// <summary>
        /// Gets or sets the BatchExportProcessor options. Ignored unless ExportProcessorType is Batch.
        /// </summary>
        public BatchExportProcessorOptions<Activity> BatchExportProcessorOptions { get; set; } = new BatchExportProcessorOptions<Activity>();

        internal TelemetryConfiguration TelemetryConfiguration { get; } = new TelemetryConfiguration();
    }
}
