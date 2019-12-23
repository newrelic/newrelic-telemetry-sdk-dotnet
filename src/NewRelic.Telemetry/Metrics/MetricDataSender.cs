using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry.Transport;
using System.Linq;

namespace NewRelic.Telemetry.Metrics
{
    /// <summary>
    /// The MetricDataSender is used to send Metric data to New Relic.  It manages the communication 
    /// with the New Relic end points and reports outcomes.
    /// </summary>
    public class MetricDataSender : DataSender<MetricBatch>
    {
        protected override string EndpointUrl => _config.MetricUrl;

        /// <summary>
        /// Creates new MetricDataSender setting the options using an instance of TelemetryConfiguration
        /// to specify settings.
        /// </summary>
        /// <param name="configOptions"></param>
        public MetricDataSender(TelemetryConfiguration configOptions) : base(configOptions)
        {
        }

        /// <summary>
        /// Creates new MetricDataSender setting the options using an instance of TelemetryConfiguration
        /// to specify settings and a Logger Factory that will be used to log information about the
        /// interactions with New Relic endpoints.
        /// </summary>
        /// <param name="configOptions"></param>
        /// <param name="loggerFactory"></param>
        public MetricDataSender(TelemetryConfiguration configOptions, ILoggerFactory loggerFactory) : base(configOptions, loggerFactory)
        {
        }

        /// <summary>
        /// Creates new MetricDataSender obtaining configuration settings from a Configuration Provider 
        /// that is compatible with <see cref="Microsoft.Extensions.Configuration">Microsoft.Extensions.Configuration.</see>
        /// </summary>
        /// <param name="configProvider"></param>
        public MetricDataSender(IConfiguration configProvider) : base(configProvider)
        {
        }

        /// <summary>
        /// Creates new MetricDataSender obtaining configuration settings from a Configuration Provider 
        /// that is compatible with <see cref="Microsoft.Extensions.Configuration">Microsoft.Extensions.Configuration.</see>
        /// It also accepts a <see cref="Microsoft.Extensions.Logging.ILoggerFactory">logger factory</see> 
        /// that will be used to log information about the interactions with New Relic endpoints.
        /// </summary>
        /// <param name="configProvider"></param>
        /// <param name="loggerFactory"></param>
        public MetricDataSender(IConfiguration configProvider, ILoggerFactory loggerFactory) : base(configProvider, loggerFactory)
        {
        }

        protected override bool ContainsNoData(MetricBatch dataToCheck)
        {
            return (dataToCheck?.Metrics?.Count).GetValueOrDefault(0) == 0;
        }

        protected override MetricBatch[] Split(MetricBatch dataToSplit)
        {
            var countMetrics = dataToSplit.Metrics.Count;
            if (countMetrics <= 1)
            {
                return null;
            }

            var targetMetricCount = countMetrics / 2;
            var batch0Metrics = dataToSplit.Metrics.Take(targetMetricCount).ToList();
            var batch1Metrics = dataToSplit.Metrics.Skip(targetMetricCount).ToList();

            var batch0 = new MetricBatch(dataToSplit.CommonProperties, batch0Metrics);
            var batch1 = new MetricBatch(dataToSplit.CommonProperties, batch1Metrics);

            return new[] { batch0, batch1 };
        }
    }
}
