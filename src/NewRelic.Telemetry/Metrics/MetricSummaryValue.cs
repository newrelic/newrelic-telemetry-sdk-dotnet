namespace NewRelic.Telemetry.Metrics
{
    /// <summary>
    /// Represents the aggregation values for a Summary Metric.
    /// </summary>
    public struct MetricSummaryValue
    {
        /// <summary>
        /// The number of observations that were aggregated.
        /// Must be a positive number.
        /// </summary>
        public double Count { get; private set; }

        /// <summary>
        /// The sum of the values that were observed.
        /// </summary>
        public double Sum { get; private set; }

        /// <summary>
        /// The lowest value observed.
        /// </summary>
        public double? Min { get; private set; }

        /// <summary>
        /// The highest value observed.
        /// </summary>
        public double? Max { get; private set; }

        internal MetricSummaryValue(double count, double sum, double min, double max)
            : this(count,sum)
        {
            Count = count;
            Sum = sum;
            Min = min;
            Max = max;
        }

        internal MetricSummaryValue(double count, double sum)
        {
            Count = count;
            Sum = sum;
            Min = null;
            Max = null;
        }
    }
}
