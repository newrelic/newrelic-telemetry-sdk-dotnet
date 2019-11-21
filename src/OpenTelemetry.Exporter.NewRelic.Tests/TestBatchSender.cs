using System.Collections.Generic;
using NRSpans = NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;
using System.Threading.Tasks;
using System.Linq;

namespace OpenTelemetry.Exporter.NewRelic.Tests
{
    public class TestBatchSender : NRSpans.ISpanBatchSender
    {
        public string TraceUrl => "http://testUrl.com/Test";

        public readonly List<NRSpans.SpanBatch> CapturedSpanBatches = new List<NRSpans.SpanBatch>();

        public Dictionary<string, NRSpans.Span> CapturedSpansDic => CapturedSpanBatches.SelectMany(x => x.Spans).ToDictionary(x => x.Id);

        public Task<Response> SendDataAsync(NRSpans.SpanBatch spanBatch)
        {
            CapturedSpanBatches.Add(spanBatch);

            return Task.FromResult(new Response(true, System.Net.HttpStatusCode.OK));
        }
    }
}