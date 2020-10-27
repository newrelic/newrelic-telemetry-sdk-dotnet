﻿// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry;
using NewRelic.Telemetry.Tracing;
using NewRelic.Telemetry.Transport;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TelemetrySdk = NewRelic.Telemetry;

namespace NewRelic.OpenTelemetry
{
    /// <summary>
    /// An exporter used to send Trace/Span information to New Relic.
    /// </summary>
    public class NewRelicTraceExporter : BaseExporter<Activity>
    {
        private const string ProductName = "NewRelic-Dotnet-OpenTelemetry";

        private static readonly string _productVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<PackageVersionAttribute>().PackageVersion;

        private readonly TraceDataSender _spanDataSender;
        private readonly ILogger? _logger;
        private readonly TelemetrySdk.TelemetryConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewRelicTraceExporter"/> class.
        /// Configures the Trace Exporter accepting configuration settings from an instance of the New Relic Exporter options object.
        /// </summary>
        /// <param name="options"></param>
        public NewRelicTraceExporter(NewRelicExporterOptions options)
            : this(options, null!)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewRelicTraceExporter"/> class.
        /// Configures the Trace Exporter accepting configuration settings from an instance of the New Relic Exporter options object.  Also
        /// accepts a logger factory supported by Microsoft.Extensions.Logging.
        /// </summary>
        /// <param name="options"></param>
        public NewRelicTraceExporter(NewRelicExporterOptions options, ILoggerFactory loggerFactory)
            : this(new TraceDataSender(options.TelemetryConfiguration, loggerFactory), options, loggerFactory)
        {
        }

        internal NewRelicTraceExporter(TraceDataSender spanDataSender, NewRelicExporterOptions options, ILoggerFactory? loggerFactory)
        {
            _spanDataSender = spanDataSender;
            spanDataSender.AddVersionInfo(ProductName, _productVersion);

            _config = options.TelemetryConfiguration;

            if (loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger("NewRelicTraceExporter");
            }
        }

        /// <inheritdoc />
        public override ExportResult Export(in Batch<Activity> activityBatch)
        {
            // Prevent exporter's HTTP operations from being instrumented.
            using var scope = SuppressInstrumentationScope.Begin();

            var spanBatches = ToNewRelicSpanBatches(activityBatch);

            if (spanBatches.Count() == 0)
            {
                return ExportResult.Success;
            }

            Response? response = null;
            Task.Run(async () => response = await _spanDataSender.SendDataAsync(spanBatches)).GetAwaiter().GetResult();

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

        private static string? ActivityKindToString(ActivityKind kind)
        {
            return kind switch
            {
                ActivityKind.Consumer => "CONSUMER",
                ActivityKind.Client => "CLIENT",
                ActivityKind.Internal => "INTERNAL",
                ActivityKind.Producer => "PRODUCER",
                ActivityKind.Server => "SERVER",
                _ => null,
            };
        }

        private IEnumerable<NewRelicSpanBatch> ToNewRelicSpanBatches(in Batch<Activity> activityBatch)
        {
            var spansByResource = GroupByResource(activityBatch);
            var spanBatches = new List<NewRelicSpanBatch>(spansByResource.Count);

            foreach (var resource in spansByResource)
            {
                string? serviceName = null;
                string? serviceNamespace = null;
                Dictionary<string, object>? commonProperties = new Dictionary<string, object>();

                commonProperties.Add(NewRelicConsts.AttribNameCollectorName, "newrelic-opentelemetry-exporter");
                commonProperties.Add(NewRelicConsts.AttribNameInstrumentationProvider, "opentelemetry");

                foreach (var label in resource.Key.Attributes)
                {
                    switch (label.Key)
                    {
                        case Resource.ServiceNameKey:
                            serviceName = label.Value as string;
                            continue;
                        case Resource.ServiceNamespaceKey:
                            serviceNamespace = label.Value as string;
                            continue;
                    }

                    commonProperties[label.Key] = label.Value;
                }

                if (!string.IsNullOrWhiteSpace(serviceName))
                {
                    serviceName = serviceNamespace != null
                        ? serviceNamespace + "." + serviceName
                        : serviceName;
                }
                else
                {
                    serviceName = _config.ServiceName;
                }

                if (!string.IsNullOrWhiteSpace(serviceName))
                {
                    commonProperties.Add(NewRelicConsts.Tracing.AttribNameServiceName, serviceName!);
                }

                var spanBatchCommonProperties = new NewRelicSpanBatchCommonProperties(null, commonProperties);
                var spanBatch = new NewRelicSpanBatch(resource.Value, spanBatchCommonProperties);
                spanBatches.Add(spanBatch);
            }

            return spanBatches;
        }

        private Dictionary<Resource, List<NewRelicSpan>> GroupByResource(in Batch<Activity> activityBatch)
        {
            var result = new Dictionary<Resource, List<NewRelicSpan>>();
            foreach (var activity in activityBatch)
            {
                var resource = activity.GetResource();
                if (!result.TryGetValue(resource, out var spans))
                {
                    spans = new List<NewRelicSpan>();
                    result[resource] = spans;
                }

                try
                {
                    var newRelicSpan = ToNewRelicSpan(activity);
                    spans.Add(newRelicSpan);
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                    {
                        var otSpanId = "<unknown>";
                        try
                        {
                            otSpanId = activity.Context.SpanId.ToHexString();
                        }
                        catch
                        {
                        }

                        _logger.LogError(null, ex, $"Error translating Open Telemetry Span {otSpanId} to New Relic Span.");
                    }
                }
            }

            return result;
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
            if (status.StatusCode == StatusCode.Error)
            {
                if (!string.IsNullOrWhiteSpace(status.Description))
                {
                    newRelicSpanAttribs.Add(NewRelicConsts.Tracing.AttribNameErrorMsg, status.Description);
                }
                else
                {
                    newRelicSpanAttribs.Add(NewRelicConsts.Tracing.AttribNameErrorMsg, "Unspecified error");
                }
            }

            var parentSpanId = openTelemetrySpan.ParentSpanId != default
                ? openTelemetrySpan.ParentSpanId.ToHexString()
                : null;

            var spanKind = ActivityKindToString(openTelemetrySpan.Kind);
            if (spanKind != null)
            {
                newRelicSpanAttribs.Add(NewRelicConsts.Tracing.AttribSpanKind, spanKind);
            }

            var source = openTelemetrySpan.Source;
            if (source != null)
            {
                newRelicSpanAttribs.Add(NewRelicConsts.AttributeInstrumentationName, openTelemetrySpan.Source.Name);
                if (source.Version != null)
                {
                    newRelicSpanAttribs.Add(NewRelicConsts.AttributeInstrumentationVersion, source.Version);
                }
            }

            if (openTelemetrySpan.TagObjects != null)
            {
                foreach (var spanAttrib in openTelemetrySpan.TagObjects)
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
                timestamp: new DateTimeOffset(openTelemetrySpan.StartTimeUtc).ToUnixTimeMilliseconds(),
                attributes: newRelicSpanAttribs);

            return newRelicSpan;
        }
    }
}
