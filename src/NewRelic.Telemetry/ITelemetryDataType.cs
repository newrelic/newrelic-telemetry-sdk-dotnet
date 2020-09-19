// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

namespace NewRelic.Telemetry
{
    /// <summary>
    /// Interface used to identify a data type to be sent to a New Relic endpoint.
    /// </summary>
#if INTERNALIZE_TELEMETRY_SDK
    internal
#else
    public
#endif
    interface ITelemetryDataType<T>
        where T : ITelemetryDataType<T>
    {
        string ToJson();

        void SetInstrumentationProvider(string instrumentationProvider);
    }
}
