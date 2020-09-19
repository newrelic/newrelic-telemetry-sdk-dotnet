// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.Logging;
using NewRelic.Telemetry.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewRelic.Telemetry.Metrics
{
    public class MetricDataSender : DataSender<NewRelicMetricBatch>
    {
        protected override Uri EndpointUrl => _config.MetricUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricDataSender"/> class.
        /// Creates new MetricDataSender setting the options using an instance of TelemetryConfiguration
        /// to specify settings.
        /// </summary>
        /// <param name="configOptions"></param>
        public MetricDataSender(TelemetryConfiguration configOptions)
            : base(configOptions)
        {
        }

        protected override bool ContainsNoData(NewRelicMetricBatch dataToCheck)
        {
            return !dataToCheck.Metrics.Any();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricDataSender"/> class.
        /// Creates new MetricDataSender setting the options using an instance of TelemetryConfiguration
        /// to specify settings and a Logger Factory that will be used to log information about the
        /// interactions with New Relic endpoints.
        /// </summary>
        /// <param name="configOptions"></param>
        /// <param name="loggerFactory"></param>
        public MetricDataSender(TelemetryConfiguration configOptions, ILoggerFactory loggerFactory)
            : base(configOptions, loggerFactory)
        {
        }

        public async Task<Response> SendDataAsync(IEnumerable<NewRelicMetric> metrics)
        {
            var batch = new NewRelicMetricBatch(metrics);

            return await SendDataAsync(batch);
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
                new NewRelicMetricBatch(batch1Metrics, metricBatch.CommonProperties),
            };

            return result;
        }
    }
}
