// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry;

namespace OpenTelemetry.Exporter.NewRelic
{
    /// <summary>
    /// Zipkin trace exporter options.
    /// </summary>
    public sealed class NewRelicExporterOptions
    {
        /// <summary>
        /// New Relic Insights Insert API Key.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the service reporting telemetry.
        /// </summary>
        public string ServiceName { get; set; } = "OpenTelemetry Exporter";

        /// <summary>
        /// Gets or sets New Relic endpoint address.
        /// </summary>
        public Uri TraceUrl { get; set; } = new Uri("https://trace-api.newrelic.com/trace/v1");

        /// <summary>
        /// A LoggerFactory for enabling logging from the New Relic exporter.
        /// </summary>
        public ILoggerFactory? LoggerFactory { get; set; }

        internal TelemetryConfiguration ToTelemetryConfiguration()
        {
            var config = new TelemetryConfiguration();
            config
                .WithApiKey(ApiKey)
                .WithServiceName(ServiceName)
                .WithOverrideEndpointUrlTrace(TraceUrl.ToString())
                .WithInstrumentationProviderName("opentelemetry");

            return config;
        }
    }
}
