// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry;
using NewRelic.Telemetry.Tracing;
using NewRelic.Telemetry.Transport;
using OpenTelemetry.Trace;
using TelemetrySdk = NewRelic.Telemetry;

namespace OpenTelemetry.Exporter.NewRelic
{
    /// <summary>
    /// An exporter used to send Trace/Span information to New Relic.
    /// </summary>
    public class NewRelicTraceExporter : ActivityExporter
    {
        private const string ProductName = "NewRelic-Dotnet-OpenTelemetry";

        private static readonly ActivitySpanId EmptyActivitySpanId = ActivitySpanId.CreateFromBytes(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, });
        private static readonly string _productVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<PackageVersionAttribute>().PackageVersion;

        private readonly TraceDataSender _spanDataSender;
        private readonly ILogger? _logger;
        private readonly TelemetrySdk.TelemetryConfiguration _config;
        private readonly string[] _nrEndpoints;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewRelicTraceExporter"/> class.
        /// Configures the Trace Exporter accepting settings from any configuration provider supported by Microsoft.Extensions.Configuration.
        /// </summary>
        /// <param name="configProvider"></param>
        public NewRelicTraceExporter(IConfiguration configProvider)
            : this(configProvider, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewRelicTraceExporter"/> class.
        /// Configures the Trace Exporter accepting settings from any configuration provider supported by Microsoft.Extensions.Configuration.
        /// Also accepts any logging infrastructure supported by Microsoft.Extensions.Logging.
        /// </summary>
        /// <param name="configProvider"></param>
        /// <param name="loggerFactory"></param>
        public NewRelicTraceExporter(IConfiguration configProvider, ILoggerFactory? loggerFactory)
            : this(new TelemetrySdk.TelemetryConfiguration(configProvider), loggerFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewRelicTraceExporter"/> class.
        /// Configures the Trace Exporter accepting configuration settings from an instance of the New Relic Telemetry SDK configuration object.
        /// </summary>
        /// <param name="config"></param>
        public NewRelicTraceExporter(TelemetrySdk.TelemetryConfiguration config)
            : this(config, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewRelicTraceExporter"/> class.
        /// Configures the Trace Exporter accepting configuration settings from an instance of the New Relic Telemetry SDK configuration object.  Also
        /// accepts a logger factory supported by Microsoft.Extensions.Logging.
        /// </summary>
        /// <param name="config"></param>
        public NewRelicTraceExporter(TelemetrySdk.TelemetryConfiguration config, ILoggerFactory? loggerFactory)
            : this(new TraceDataSender(config, loggerFactory), config, loggerFactory)
        {
        }

        internal NewRelicTraceExporter(TraceDataSender spanDataSender, TelemetrySdk.TelemetryConfiguration config, ILoggerFactory? loggerFactory)
        {
            _spanDataSender = spanDataSender;
            spanDataSender.AddVersionInfo(ProductName, _productVersion);

            _config = config;

            _config.WithInstrumentationProviderName("opentelemetry");

            _nrEndpoints = config.NewRelicEndpoints.Select(x => x.ToLower()).ToArray();

            if (loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger("NewRelicTraceExporter");
            }
        }

        /// <inheritdoc />
        public override ExportResult Export(in Batch<Activity> batch)
        {
            // Prevent exporter's HTTP operations from being instrumented.
            using var scope = SuppressInstrumentationScope.Begin();

            var nrSpans = ToNewRelicSpans(batch);

            if (nrSpans.Count() == 0)
            {
                return ExportResult.Success;
            }

            Response? response = null;
            Task.Run(async () => response = await _spanDataSender.SendDataAsync(nrSpans)).GetAwaiter().GetResult();

            switch (response?.ResponseStatus)
            {
                case NewRelicResponseStatus.DidNotSend_NoData:
                case NewRelicResponseStatus.Success:
                    return ExportResult.Success;

                case NewRelicResponseStatus.Failure:
                default:
                    return ExportResult.Failure;
            }
        }

        private List<NewRelicSpan> ToNewRelicSpans(in Batch<Activity> otSpans)
        {
            var nrSpans = new List<NewRelicSpan>();
            var spanIdsToFilter = new List<string>();

            foreach (var otSpan in otSpans)
            {
                try
                {
                    var nrSpan = ToNewRelicSpan(otSpan);
                    nrSpans.Add(nrSpan);
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                    {
                        var otSpanId = "<unknown>";
                        try
                        {
                            otSpanId = otSpan.Context.SpanId.ToHexString();
                        }
                        catch
                        {
                        }

                        _logger.LogError(null, ex, $"Error translating Open Telemetry Span {otSpanId} to New Relic Span.");
                    }
                }
            }

            return nrSpans;
        }

        private NewRelicSpan ToNewRelicSpan(Activity openTelemetrySpan)
        {
            if (openTelemetrySpan == default)
            {
                throw new ArgumentException(nameof(openTelemetrySpan));
            }

            if (openTelemetrySpan.Context == default)
            {
                throw new ArgumentException($"{nameof(openTelemetrySpan)}.Context");
            }

            // Build attributes with required items
            var newRelicSpanAttribs = new Dictionary<string, object>()
            {
                { NewRelicConsts.Tracing.AttribNameDurationMs, openTelemetrySpan.Duration.TotalMilliseconds },
            };

            if (!string.IsNullOrWhiteSpace(openTelemetrySpan.DisplayName))
            {
                newRelicSpanAttribs.Add(NewRelicConsts.Tracing.AttribNameName, openTelemetrySpan.DisplayName);
            }

            var status = openTelemetrySpan.GetStatus();
            if (!status.IsOk)
            {
                newRelicSpanAttribs.Add(NewRelicConsts.Tracing.AttribNameHasError, true);

                if (!string.IsNullOrWhiteSpace(status.Description))
                {
                    newRelicSpanAttribs.Add(NewRelicConsts.Tracing.AttribNameErrorMsg, status.Description);
                }
            }

            if (_config.ServiceName != null && !string.IsNullOrWhiteSpace(_config.ServiceName))
            {
                newRelicSpanAttribs.Add(NewRelicConsts.Tracing.AttribNameServiceName, _config.ServiceName);
            }

            var parentSpanId = null as string;
            if (openTelemetrySpan.ParentSpanId != EmptyActivitySpanId)
            {
                parentSpanId = openTelemetrySpan.ParentSpanId.ToHexString();
            }

            if (openTelemetrySpan.Tags != null)
            {
                foreach (var spanAttrib in openTelemetrySpan.Tags)
                {
                    if (spanAttrib.Value == null)
                    {
                        continue;
                    }

                    newRelicSpanAttribs.Add(spanAttrib.Key, spanAttrib.Value);
                }
            }

            var newRelicSpan = new NewRelicSpan(
                traceId: openTelemetrySpan.Context.TraceId.ToHexString(),
                spanId: openTelemetrySpan.Context.SpanId.ToHexString(),
                parentSpanId: parentSpanId,
                timestamp: openTelemetrySpan.StartTimeUtc.ToUnixTimeMilliseconds(),
                attributes: newRelicSpanAttribs);

            return newRelicSpan;
        }
    }
}
