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

        private readonly NRSpans.SpanBatchSender _spanBatchSender;
        private readonly Func<Span, string, NRSpans.Span> _spanConverter;
        private string _serviceName;

        private static Func<Span, string, NRSpans.Span> _defaultSpanConverter = SpanConverter.ToNewRelicSpan;

        public NewRelicTraceExporter(string serviceName) : this(_defaultSpanConverter)
        {
            _serviceName = serviceName;

        }

        internal NewRelicTraceExporter(Func<Span, string, NRSpans.Span> spanConverterImpl)
        {

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

            foreach(var otSpan in otSpanBatch)
            {
                try
                {
                    _spanConverter(otSpan, _serviceName);
                }
                catch(Exception ex)
                {
                }
            }

            var nrSpanBatch = spanBatchBuilder.Build();
            throw new NotImplementedException();
            var result = await _spanBatchSender.SendDataAsync(nrSpanBatch);
            //return;
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
