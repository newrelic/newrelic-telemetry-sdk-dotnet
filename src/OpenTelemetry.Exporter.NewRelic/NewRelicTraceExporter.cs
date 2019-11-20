using NRTelemetry = NewRelic.Telemetry;
using NRSpans = NewRelic.Telemetry.Spans;
using NRTransport = NewRelic.Telemetry.Spans;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Export;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace OpenTelemetry.Exporter.NewRelic
{


    public class NewRelicTraceExporter : SpanExporter, IDisposable
    {

        private readonly NRSpans.ISpanBatchSender _spanBatchSender;

        private string _serviceName;
        private const string _attribName_url = "http.url";
        
        //TODO: this needs to be replaced;
        private const string _nrEndpointUrl = "https://trace-api.newrelic.com/trace/v1";

        public NewRelicTraceExporter() : this(new NRSpans.SpanBatchSenderBuilder().Build())
        {
        }

        internal NewRelicTraceExporter(NRSpans.ISpanBatchSender spanBatchSender)
        {
            _spanBatchSender = spanBatchSender;
        }

        public NewRelicTraceExporter WithServiceName(string serviceName)
        {
            _serviceName = serviceName;
            return this;
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
                }
            }

            var nrSpanBatch = spanBatchBuilder.Build();

            return await Task.FromResult(_spanBatchSender.SendDataAsync(nrSpanBatch).IsCompleted
                ? ExportResult.Success
                : ExportResult.FailedNotRetryable);
        }

        public override Task ShutdownAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
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

            if (!string.IsNullOrWhiteSpace(_serviceName))
            {
                newRelicSpanBuilder.WithServiceName(_serviceName);
            }

            if (openTelemetrySpan.ParentSpanId != null)
            {
                newRelicSpanBuilder.WithParentId(openTelemetrySpan.ParentSpanId.ToHexString());
            }

            if (openTelemetrySpan.Attributes != null)
            {
                foreach (var spanAttrib in openTelemetrySpan.Attributes)
                {
                    if (string.Equals(spanAttrib.Key, _attribName_url, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(spanAttrib.Value?.ToString(), _nrEndpointUrl, StringComparison.OrdinalIgnoreCase))
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
