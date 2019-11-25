using NewRelic.Telemetry;
using NewRelic.Telemetry.Transport;
using NRSpans = NewRelic.Telemetry.Spans;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Export;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace OpenTelemetry.Exporter.NewRelic
{
    public class NewRelicTraceExporter : SpanExporter
    {
        private readonly NRSpans.SpanDataSender _spanDataSender;

        private const string _attribName_url = "http.url";

        private readonly ILogger _logger;
        private readonly TelemetryConfiguration _config;


        public NewRelicTraceExporter(IConfiguration configProvider) : this(configProvider, null)
        {
        }
        public NewRelicTraceExporter(IConfiguration configProvider, ILoggerFactory loggerFactory) : this(new TelemetryConfiguration(configProvider), loggerFactory)
        {
        }

        public NewRelicTraceExporter(TelemetryConfiguration config) : this(config, null)
        {
           
        }

        public NewRelicTraceExporter(TelemetryConfiguration config, ILoggerFactory loggerFactory) : this(new NRSpans.SpanDataSender(config, loggerFactory),config,loggerFactory)
        {
            
        }

        internal NewRelicTraceExporter(NRSpans.SpanDataSender spanDataSender, TelemetryConfiguration config, ILoggerFactory loggerFactory)
        {
            _spanDataSender = spanDataSender;

            _config = config;

            if (loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger("NewRelicTraceExporter");
            }

        }

        public async override Task<ExportResult> ExportAsync(IEnumerable<Span> otSpanBatch, CancellationToken cancellationToken)
        {
            if (otSpanBatch == null) throw new System.ArgumentNullException(nameof(otSpanBatch));
            if (cancellationToken == null) throw new ArgumentNullException(nameof(cancellationToken));

            var spanBatchBuilder = NRSpans.SpanBatchBuilder.Create();

            foreach (var otSpan in otSpanBatch)
            {
                try
                {
                    spanBatchBuilder.WithSpan(ToNewRelicSpan(otSpan));
                }
                catch (Exception ex)
                {
                    _logger.LogError(null, ex, $"Error translating Open Telemetry Span {otSpan.Context.SpanId.ToHexString()} to New Relic Span.");
                }
            }

            var nrSpanBatch = spanBatchBuilder.Build();
            
            var result = await _spanDataSender.SendDataAsync(nrSpanBatch);
            return result.ResponseStatus == NewRelicResponseStatus.SendSuccess ? ExportResult.Success : ExportResult.FailedNotRetryable;
        }

        public override Task ShutdownAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public void Dispose()
        {
        }

        private NRSpans.Span ToNewRelicSpan(Span openTelemetrySpan)
        {
            if (openTelemetrySpan == null) throw new ArgumentNullException(nameof(openTelemetrySpan));
            if (openTelemetrySpan.Context == null) throw new NullReferenceException($"{nameof(openTelemetrySpan)}.Context");
            if (openTelemetrySpan.Context.SpanId == null) throw new NullReferenceException($"{nameof(openTelemetrySpan)}.Context.SpanId");
            if (openTelemetrySpan.Context.TraceId == null) throw new NullReferenceException($"{nameof(openTelemetrySpan)}.Context.TraceId");
            if (openTelemetrySpan.StartTimestamp == null) throw new NullReferenceException($"{nameof(openTelemetrySpan)}.StartTimestamp");

            var newRelicSpanBuilder = NRSpans.SpanBuilder.Create(openTelemetrySpan.Context.SpanId.ToHexString())
                   .WithTraceId(openTelemetrySpan.Context.TraceId.ToHexString())
                   .WithExecutionTimeInfo(openTelemetrySpan.StartTimestamp, openTelemetrySpan.EndTimestamp)   //handles Nulls
                   .HasError(!openTelemetrySpan.Status.IsOk)
                   .WithName(openTelemetrySpan.Name);       //Handles Nulls

            if (!string.IsNullOrWhiteSpace(_config.ServiceName))
            {
                newRelicSpanBuilder.WithServiceName(_config.ServiceName);
            }

            if (openTelemetrySpan.ParentSpanId != null)
            {
                newRelicSpanBuilder.WithParentId(openTelemetrySpan.ParentSpanId.ToHexString());
            }

            if (openTelemetrySpan.Attributes != null)
            {
                foreach (var spanAttrib in openTelemetrySpan.Attributes)
                {
                    //Filter out calls to New Relic endpoint
                    if (string.Equals(spanAttrib.Key, _attribName_url, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(spanAttrib.Value?.ToString(), _config.TraceUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                    newRelicSpanBuilder.WithAttribute(spanAttrib.Key, spanAttrib.Value);
                }
            }

            return newRelicSpanBuilder.Build();
        }
    }
}
