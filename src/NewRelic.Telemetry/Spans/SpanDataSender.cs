using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry.Transport;
using System.Linq;

namespace NewRelic.Telemetry.Spans
{
    public class SpanDataSender : DataSender<SpanBatch>
    {
        protected override string EndpointUrl => _config.TraceUrl;

        public SpanDataSender(TelemetryConfiguration configOptions) : base(configOptions)
        {
        }

        public SpanDataSender(TelemetryConfiguration configOptions, ILoggerFactory loggerFactory) : base(configOptions, loggerFactory)
        {
        }

        public SpanDataSender(IConfiguration configProvider) : base(configProvider)
        {
        }
        
        public SpanDataSender(IConfiguration configProvider, ILoggerFactory loggerFactory) : base(configProvider, loggerFactory)
        {
        }

        protected override bool ContainsNoData(SpanBatch dataToCheck)
        {
            return (dataToCheck?.Spans?.Count).GetValueOrDefault(0) == 0;
        }

        protected override SpanBatch[] Split(SpanBatch dataToSplit)
        {
            var countSpans = dataToSplit.Spans.Count;
            if (countSpans <= 1)
            {
                return null;
            }

            var targetSpanCount = countSpans / 2;
            var batch0Spans = dataToSplit.Spans.Take(targetSpanCount).ToList();
            var batch1Spans = dataToSplit.Spans.Skip(targetSpanCount).ToList();

            var batch0 = new SpanBatch(dataToSplit.CommonProperties, batch0Spans);
            var batch1 = new SpanBatch(dataToSplit.CommonProperties, batch1Spans);

            return new[] { batch0, batch1 };
        }
    }
}
