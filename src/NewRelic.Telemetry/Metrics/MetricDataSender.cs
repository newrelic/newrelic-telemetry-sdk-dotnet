using Microsoft.Extensions.Logging;
using NewRelic.Telemetry.Metrics;
using NewRelic.Telemetry.Transport;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utf8Json;
using Utf8Json.Resolvers;

namespace NewRelic.Telemetry.Metrics
{
    public class MetricDataSender : DataSender<NewRelicMetricBatch>
    {
        protected override string EndpointUrl => _config.MetricUrl;

        protected override bool ContainsNoData(NewRelicMetricBatch dataToCheck)
        {
            return !dataToCheck.Metrics.Any();
        }

        private static readonly NewRelicMetricBatch[] _emptyMetricBatchArray = new NewRelicMetricBatch[0];

        protected override NewRelicMetricBatch[] Split(NewRelicMetricBatch metricBatch)
        {
            var countMetrics = metricBatch.Metrics.Count();
            if (countMetrics <= 1)
            {
                return _emptyMetricBatchArray;
            }

            var targetMetricCount = countMetrics / 2;
            var batch0Metrics = metricBatch.Metrics.Take(targetMetricCount).ToList();
            var batch1Metrics = metricBatch.Metrics.Skip(targetMetricCount).ToList();

            var result = new[]
            {
                new NewRelicMetricBatch(batch0Metrics, metricBatch.CommonProperties),
                new NewRelicMetricBatch(batch1Metrics, metricBatch.CommonProperties)
            };

            return result;
        }

        public MetricDataSender(TelemetryConfiguration config, ILoggerFactory? loggerFactory) : base(config, loggerFactory)
        {
        }

        public async Task<Response> SendDataAsync(IEnumerable<NewRelicMetric> metrics)
        {
            var batch = new NewRelicMetricBatch(metrics, null);

            return await SendDataAsync(batch);
        }
    }
}
