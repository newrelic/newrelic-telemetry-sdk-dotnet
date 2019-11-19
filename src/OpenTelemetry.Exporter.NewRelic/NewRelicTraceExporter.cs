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

        private string _serviceName;
        private readonly NRSpans.SpanBatchSender _spanBatchSender;


        public NewRelicTraceExporter(string serviceName)
        {
            _serviceName = serviceName;

           

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
                    spanBatchBuilder.WithSpan(otSpan.ToNewRelicSpan(_serviceName));
                }
                catch(Exception ex)
                {
                }
            }

            var nrSpanBatch = spanBatchBuilder.Build();

            Task.Run(()=> { _spanBatchSender.SendDataAsync(nrSpanBatch); }, cancellationToken);
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
