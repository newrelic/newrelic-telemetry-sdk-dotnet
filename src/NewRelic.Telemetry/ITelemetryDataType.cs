namespace NewRelic.Telemetry
{
    /// <summary>
    /// Interface used to identify a data type to be sent to a New Relic endpoint.
    /// </summary>
    public interface ITelemetryDataType
    {
        string ToJson();
    }
}
