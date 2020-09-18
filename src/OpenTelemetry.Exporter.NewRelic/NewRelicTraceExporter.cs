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
using NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Exporter.NewRelic
{
    /// <summary>
    /// An exporter used to send Trace/Span information to New Relic.
    /// </summary>
    public class NewRelicTraceExporter : ActivityExporter
    {
        private const string ProductName = "NewRelic-Dotnet-OpenTelemetry";
        private const string AttribNameUrl = "http.url";

        private static readonly ActivitySpanId _emptyActivitySpanId = ActivitySpanId.CreateFromBytes(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, });
        private static readonly string _productVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<PackageVersionAttribute>().PackageVersion;

        private readonly SpanDataSender _spanDataSender;
        private readonly ILogger _logger;
        private readonly TelemetryConfiguration _config;
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
        public NewRelicTraceExporter(IConfiguration configProvider, ILoggerFactory loggerFactory)
            : this(new TelemetryConfiguration(configProvider), loggerFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewRelicTraceExporter"/> class.
        /// Configures the Trace Exporter accepting configuration settings from an instance of the New Relic Telemetry SDK configuration object.
        /// </summary>
        /// <param name="config"></param>
        public NewRelicTraceExporter(TelemetryConfiguration config)
            : this(config, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewRelicTraceExporter"/> class.
        /// Configures the Trace Exporter accepting configuration settings from an instance of the New Relic Telemetry SDK configuration object.  Also
        /// accepts a logger factory supported by Microsoft.Extensions.Logging.
        /// </summary>
        /// <param name="config"></param>
        public NewRelicTraceExporter(TelemetryConfiguration config, ILoggerFactory loggerFactory)
            : this(new SpanDataSender(config, loggerFactory), config, loggerFactory)
        {
        }

        internal NewRelicTraceExporter(SpanDataSender spanDataSender, TelemetryConfiguration config, ILoggerFactory loggerFactory)
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

            var nrSpanBatch = ToNewRelicSpanBatch(batch);

            if (nrSpanBatch.Spans?.Count == 0)
            {
                return ExportResult.Success;
            }

            Response response = null;
            Task.Run(async () => response = await _spanDataSender.SendDataAsync(nrSpanBatch)).GetAwaiter().GetResult();

            switch (response.ResponseStatus)
            {
                case NewRelicResponseStatus.DidNotSend_NoData:
                case NewRelicResponseStatus.Success:
                    return ExportResult.Success;
               
                case NewRelicResponseStatus.Failure:
                default:
                    return ExportResult.Failure;
            }
        }

        private SpanBatch ToNewRelicSpanBatch(in Batch<Activity> otSpans)
        {
            var nrSpans = new List<Span>();
            var spanIdsToFilter = new List<string>();

            foreach (var otSpan in otSpans)
            {
                if (otSpan == null)
                {
                    continue;
                }

                try
                {
                    var nrSpan = ToNewRelicSpan(otSpan);
                    if (nrSpan == null)
                    {
                        spanIdsToFilter.Add(otSpan.Context.SpanId.ToHexString());
                        _logger?.LogDebug(null, $"The following span was filtered because it describes communication with a New Relic endpoint: Trace={otSpan.Context.TraceId}, Span={otSpan.Context.SpanId}, ParentSpan={otSpan.ParentSpanId}");
                    }
                    else
                    {
                        nrSpans.Add(nrSpan);
                    }
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

            nrSpans = FilterSpans(nrSpans, spanIdsToFilter);

            var spanBatchBuilder = SpanBatchBuilder.Create();

            spanBatchBuilder.WithSpans(nrSpans);

            var nrSpanBatch = spanBatchBuilder.Build();

            return nrSpanBatch;
        }

        private List<Span> FilterSpans(List<Span> spans, List<string> spanIdsToFilter)
        {
            if (spanIdsToFilter.Count == 0)
            {
                return spans;
            }

            var newSpansToFilter = spans.Where(x => spanIdsToFilter.Contains(x.ParentId)).ToArray();
            
            if (newSpansToFilter.Length == 0)
            {
                return spans;
            } 
            
            if (_logger?.IsEnabled(LogLevel.Debug) == true)
            {
                foreach (var spanToFilter in newSpansToFilter)
                {
                    _logger?.LogDebug(null, $"The following span was filtered because it is a descendant of a span that describes communication with a New Relic endpoint: Trace={spanToFilter.TraceId}, Span={spanToFilter.Id}, ParentSpan={spanToFilter.ParentId ?? "<NULL>"}");
                }
            }

            var newSpanIdsToFilter = newSpansToFilter.Select(x => x.Id).ToArray();

            spanIdsToFilter = spanIdsToFilter.Union(newSpanIdsToFilter).ToList();

            spans = spans.Except(newSpansToFilter).ToList();
            
            return FilterSpans(spans, spanIdsToFilter);
        }

        private Span ToNewRelicSpan(Activity openTelemetrySpan)
        {
            if (openTelemetrySpan == default)
            {
                throw new ArgumentException(nameof(openTelemetrySpan));
            }

            if (openTelemetrySpan.Context == default)
            {
                throw new ArgumentException($"{nameof(openTelemetrySpan)}.Context");
            }

            var newRelicSpanBuilder = SpanBuilder.Create(openTelemetrySpan.Context.SpanId.ToHexString())
                   .WithTraceId(openTelemetrySpan.Context.TraceId.ToHexString())
                   .WithExecutionTimeInfo(openTelemetrySpan.StartTimeUtc, openTelemetrySpan.Duration)
                   .WithName(openTelemetrySpan.DisplayName);

            var status = openTelemetrySpan.GetStatus();
            if (!status.IsOk)
            {
                // this will set HasError = true and the description if available
                newRelicSpanBuilder.HasError(status.Description);
            }

            if (!string.IsNullOrWhiteSpace(_config.ServiceName))
            {
                newRelicSpanBuilder.WithServiceName(_config.ServiceName);
            }

            if (openTelemetrySpan.ParentSpanId != default && openTelemetrySpan.ParentSpanId != _emptyActivitySpanId)
            {
                newRelicSpanBuilder.WithParentId(openTelemetrySpan.ParentSpanId.ToHexString());
            }

            if (openTelemetrySpan.Tags != null)
            {
                foreach (var spanAttrib in openTelemetrySpan.Tags)
                {
                    // Filter out calls to New Relic endpoint as these will cause an infinite loop
                    if (string.Equals(spanAttrib.Key, AttribNameUrl, StringComparison.OrdinalIgnoreCase) && _nrEndpoints.Contains(spanAttrib.Value?.ToString().ToLower()))
                    {
                        _logger?.LogDebug(null, $"The following span was filtered because it was identified as communication with a New Relic endpoint: Trace={openTelemetrySpan.Context.TraceId}, Span={openTelemetrySpan.Context.SpanId}, ParentSpan={openTelemetrySpan.ParentSpanId}. url={spanAttrib.Value}");

                        return null;
                    }

                    newRelicSpanBuilder.WithAttribute(spanAttrib.Key, spanAttrib.Value);
                }
            }

            return newRelicSpanBuilder.Build();
        }
    }
}
