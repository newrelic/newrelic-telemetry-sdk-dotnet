// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry.Transport;
using NewRelic.Telemetry.Spans;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Exporter.NewRelic
{
    /// <summary>
    /// An exporter used to send Trace/Span information to New Relic.
    /// </summary>
    public class NewRelicTraceExporter : ActivityExporter
    {
        private const string _productName = "NewRelic-Dotnet-OpenTelemetry";
        private const string _attribName_url = "http.url";

        private static readonly ActivitySpanId EmptyActivitySpanId = ActivitySpanId.CreateFromBytes(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, });
        private static readonly string _productVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<PackageVersionAttribute>().PackageVersion;

        private readonly NewRelicExporterOptions _options;
        private readonly SpanDataSender _spanDataSender;
        private readonly ILogger? _logger;
        private readonly string[] _nrEndpoints;

        /// <summary>
        /// Configures the Trace Exporter accepting settings from any configuration provider supported by Microsoft.Extensions.Configuration.
        /// </summary>
        /// <param name="configProvider"></param>
        public NewRelicTraceExporter(NewRelicExporterOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;

            var telemetryConfig = _options.ToTelemetryConfiguration();

            _spanDataSender = new SpanDataSender(telemetryConfig, _options.LoggerFactory);
            _spanDataSender.AddVersionInfo(_productName, _productVersion);

            _nrEndpoints = telemetryConfig.NewRelicEndpoints.Select(x => x.ToLower()).ToArray();

            _logger = _options.LoggerFactory?.CreateLogger("NewRelicTraceExporter");
        }

        internal NewRelicTraceExporter(NewRelicExporterOptions options, SpanDataSender spanDataSender)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;

            var telemetryConfig = _options.ToTelemetryConfiguration();

            _spanDataSender = spanDataSender;
            _spanDataSender.AddVersionInfo(_productName, _productVersion);

            _nrEndpoints = telemetryConfig.NewRelicEndpoints.Select(x => x.ToLower()).ToArray();

            _logger = _options.LoggerFactory?.CreateLogger("NewRelicTraceExporter");
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

            Response? response = null;
            Task.Run(async () => response = await _spanDataSender.SendDataAsync(nrSpanBatch)).GetAwaiter().GetResult();

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

        private SpanBatch ToNewRelicSpanBatch(in Batch<Activity> otSpans)
        {
            var nrSpans = new List<Span>();
            var spanIdsToFilter = new List<string>();

            foreach (var otSpan in otSpans)
            {
                if(otSpan == null)
                {
                    continue;
                }

                try
                {
                    var nrSpan = ToNewRelicSpan(otSpan);
                    if(nrSpan == null)
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
                        catch { }

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
            if(spanIdsToFilter.Count == 0)
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

            var newSpanIdsToFilter = newSpansToFilter.Select(x=>x.Id).ToArray();

            spanIdsToFilter = spanIdsToFilter.Union(newSpanIdsToFilter).ToList();

            spans = spans.Except(newSpansToFilter).ToList();
            
            return FilterSpans(spans, spanIdsToFilter);
        }

        private Span? ToNewRelicSpan(Activity openTelemetrySpan)
        {
            if (openTelemetrySpan == default) throw new ArgumentException(nameof(openTelemetrySpan));
            if (openTelemetrySpan.Context == default) throw new ArgumentException($"{nameof(openTelemetrySpan)}.Context");

            var newRelicSpanBuilder = SpanBuilder.Create(openTelemetrySpan.Context.SpanId.ToHexString())
                   .WithTraceId(openTelemetrySpan.Context.TraceId.ToHexString())
                   .WithExecutionTimeInfo(openTelemetrySpan.StartTimeUtc, openTelemetrySpan.Duration)
                   .WithName(openTelemetrySpan.DisplayName);

            var status = openTelemetrySpan.GetStatus();
            if (!status.IsOk)
            {
                //this will set HasError = true and the description if available
                newRelicSpanBuilder.HasError(status.Description);
            }

            if (!string.IsNullOrWhiteSpace(_options.ServiceName))
            {
                newRelicSpanBuilder.WithServiceName(_options.ServiceName);
            }

            if (openTelemetrySpan.ParentSpanId != EmptyActivitySpanId)
            {
                newRelicSpanBuilder.WithParentId(openTelemetrySpan.ParentSpanId.ToHexString());
            }

            if (openTelemetrySpan.Tags != null)
            {
                foreach (var spanAttrib in openTelemetrySpan.Tags)
                {
                    //Filter out calls to New Relic endpoint as these will cause an infinite loop
                    if (string.Equals(spanAttrib.Key, _attribName_url, StringComparison.OrdinalIgnoreCase) && _nrEndpoints.Contains(spanAttrib.Value?.ToString().ToLower()))
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
