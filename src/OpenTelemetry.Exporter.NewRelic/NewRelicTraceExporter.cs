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
    public static class SpanConverter
    {
        private const string _attribName_url = "http.url";
        private const string _NewRelicTraceEndpoint = "https://trace-api.newrelic.com/trace/v1";

        public static NRSpans.Span ToNewRelicSpan(this Span otSpan, string serviceName)
        {
            if (otSpan == null) throw new ArgumentNullException(nameof(otSpan));
            if (otSpan.Context == null) throw new NullReferenceException($"{nameof(otSpan)}.Context");
            if (otSpan.Context.SpanId == null) throw new NullReferenceException($"{nameof(otSpan)}.Context.SpanId");
            if (otSpan.Context.TraceId == null) throw new NullReferenceException($"{nameof(otSpan)}.Context.TraceId");
            if (otSpan.StartTimestamp == null) throw new NullReferenceException($"{nameof(otSpan)}.StartTimestamp");

            var spanBuilder = NRSpans.SpanBuilder.Create(otSpan.Context.SpanId.ToHexString())
                   .WithTraceId(otSpan.Context.TraceId.ToHexString())
                   .WithExecutionTimeInfo(otSpan.StartTimestamp, otSpan.EndTimestamp)   //handles Nulls
                   .HasError(!otSpan.Status.IsOk)
                   .WithName(otSpan.Name);       //Handles Nulls


            if (!string.IsNullOrWhiteSpace(serviceName))
            {
                spanBuilder.WithServiceName(serviceName);
            }

            if(otSpan.ParentSpanId != null)
            {
                spanBuilder.WithParentId(otSpan.ParentSpanId.ToHexString());
            }

            if (otSpan.Attributes != null)
            {
                foreach (var spanAttrib in otSpan.Attributes)
                {
                    if(string.Equals(spanAttrib.Key, _attribName_url,StringComparison.OrdinalIgnoreCase) 
                        && string.Equals(spanAttrib.Value?.ToString(),_NewRelicTraceEndpoint, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                    spanBuilder.WithAttribute(spanAttrib.Key, spanAttrib.Value);
                }
            }

            return spanBuilder.Build();
        }
    }


    public class NewRelicTraceExporter : SpanExporter, IDisposable
    {

        private readonly string _serviceName;
        private readonly NRSpans.SpanBatchSender _spanBatchSender;


        public NewRelicTraceExporter(string serviceName)
        {
            _serviceName = serviceName;
        }


        public async override Task<ExportResult> ExportAsync(IEnumerable<Span> otSpanBatch, CancellationToken cancellationToken)
        {

            if (otSpanBatch == null) throw new System.ArgumentNullException(nameof(otSpanBatch));
            if (cancellationToken == null) throw new ArgumentNullException(nameof(cancellationToken));

            var spanBatchBuilder = NRSpans.SpanBatchBuilder.Create();

            foreach(var otSpan in otSpanBatch)
            {
                spanBatchBuilder.WithSpan(otSpan.ToNewRelicSpan(_serviceName));
            }

            var nrSpanBatch = spanBatchBuilder.Build();

            throw new NotImplementedException();

        }

        public override Task ShutdownAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }


    }
}
