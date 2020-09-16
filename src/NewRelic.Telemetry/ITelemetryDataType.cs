﻿// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

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
