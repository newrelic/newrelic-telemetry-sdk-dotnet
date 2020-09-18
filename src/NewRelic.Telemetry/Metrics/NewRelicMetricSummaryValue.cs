namespace NewRelic.Telemetry.Metrics
{
    public readonly struct NewRelicMetricSummaryValue
    {
        public double Count { get; }
        public double Sum { get; }
        public double? Min { get; }
        public double? Max { get; }

        public NewRelicMetricSummaryValue(double count, double sum, double? min, double? max)
        {
            Count = count;
            Sum = sum;
            Min = min;
            Max = max;
        }
    }
}
