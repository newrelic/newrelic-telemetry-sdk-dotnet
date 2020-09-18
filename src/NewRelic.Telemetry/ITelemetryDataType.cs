using System.Collections.Generic;

namespace NewRelic.Telemetry
{
    /// <summary>
    /// Interface used to identify a data type to be sent to a New Relic endpoint.
    /// </summary>
    public interface ITelemetryDataType<T> where T:ITelemetryDataType<T>
    {
        string ToJson();
        void SetInstrumentationProvider(string instrumentationProvider);
    }
}
