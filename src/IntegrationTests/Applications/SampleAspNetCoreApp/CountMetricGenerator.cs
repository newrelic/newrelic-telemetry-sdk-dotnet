using Microsoft.Extensions.Configuration;
using NewRelic.Telemetry.Metrics;
using System;
using System.Threading.Tasks;

namespace SampleAspNetCoreApp
{
    public class CountMetricGenerator
    {
        private IConfiguration _config;

        public CountMetricGenerator(IConfiguration configuration) 
        {
            _config = configuration;
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

            var dataSender = new MetricDataSender(_config);
            _ = await dataSender.SendDataAsync(metricBatch);
        }
    }
}