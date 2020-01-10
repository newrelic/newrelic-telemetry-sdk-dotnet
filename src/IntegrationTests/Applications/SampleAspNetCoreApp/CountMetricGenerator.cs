using NewRelic.Telemetry.Metrics;
using System;
using System.Threading.Tasks;

namespace SampleAspNetCoreApp
{
    public class CountMetricGenerator
    {
        private readonly MetricDataSender _metricDataSender;

        public CountMetricGenerator(MetricDataSender metricDataSender) 
        {
            _metricDataSender = metricDataSender;
        }

        public async Task CreateAsync(string metricName) 
        {
            var metricBuilder = MetricBuilder.CreateCountMetric(metricName)
            .WithTimestamp(DateTime.Now)
            .WithValue(1)
            .WithIntervalMs(10);

            var metric = metricBuilder.Build();

            var metricBatch = MetricBatchBuilder.Create()
            .WithMetric(metric)
            .Build();

            _ = await _metricDataSender.SendDataAsync(metricBatch);
        }
    }
}