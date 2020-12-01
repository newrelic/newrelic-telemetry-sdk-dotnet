// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using NewRelic.Telemetry;

namespace NewRelic.OpenTelemetry.Internal
{
    internal class SelfDiagnosticsLogger : ITelemetryLogger
    {
        public void Debug(string message, Exception? exception = null)
        {
            NewRelicEventSource.Log.Debug(message, exception);
        }

        public void Error(string message, Exception? exception = null)
        {
            NewRelicEventSource.Log.Error(message, exception);
        }

        public void Exception(Exception exception)
        {
            NewRelicEventSource.Log.Exception(exception);
        }

        public void Info(string message, Exception? exception = null)
        {
            NewRelicEventSource.Log.Info(message, exception);
        }

        public void Warning(string message, Exception? exception = null)
        {
            NewRelicEventSource.Log.Warning(message, exception);
        }
    }
}
