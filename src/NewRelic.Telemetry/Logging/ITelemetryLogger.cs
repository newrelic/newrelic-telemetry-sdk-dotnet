// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;

namespace NewRelic.Telemetry
{
#if INTERNALIZE_TELEMETRY_SDK
    internal
#else
    public
#endif
    interface ITelemetryLogger
    {
        void Debug(string message, Exception? exception = null);

        void Error(string message, Exception? exception = null);

        void Exception(Exception exception);

        void Info(string message, Exception? exception = null);

        void Warning(string message, Exception? exception = null);
    }
}
