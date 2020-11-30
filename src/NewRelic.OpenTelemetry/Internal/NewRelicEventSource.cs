// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Threading;

namespace NewRelic.OpenTelemetry.Internal
{
    [EventSource(Name = "OpenTelemetry-NewRelic")]
    internal class NewRelicEventSource : EventSource
    {
        public static NewRelicEventSource Log = new NewRelicEventSource();

        [NonEvent]
        public void Debug(string message, Exception? exception = null)
        {
            if (IsEnabled(EventLevel.Informational, (EventKeywords)(-1)))
            {
                if (exception != null)
                {
                    EmitInformationalMessageWithError(message, ToInvariantString(exception));
                }
                else
                {
                    EmitInformationalMessage(message);
                }
            }
        }

        [NonEvent]
        public void Error(string message, Exception? exception = null)
        {
            if (IsEnabled(EventLevel.Error, (EventKeywords)(-1)))
            {
                if (exception != null)
                {
                    EmitErrorMessageWithError(message, ToInvariantString(exception));
                }
                else
                {
                    EmitErrorMessage(message);
                }
            }
        }

        [NonEvent]
        public void Exception(Exception exception)
        {
            if (IsEnabled(EventLevel.Error, (EventKeywords)(-1)))
            {
                EmitErrorMessage(ToInvariantString(exception));
            }
        }

        [NonEvent]
        public void Info(string message, Exception? exception = null)
        {
            if (IsEnabled(EventLevel.Informational, (EventKeywords)(-1)))
            {
                if (exception != null)
                {
                    EmitInformationalMessageWithError(message, ToInvariantString(exception));
                }
                else
                {
                    EmitInformationalMessage(message);
                }
            }
        }

        [NonEvent]
        public void Warning(string message, Exception? exception = null)
        {
            if (IsEnabled(EventLevel.Warning, (EventKeywords)(-1)))
            {
                if (exception != null)
                {
                    EmitWarningMessageWithError(message, ToInvariantString(exception));
                }
                else
                {
                    EmitWarningMessage(message);
                }
            }
        }

        [Event(1, Message = "New Relic: '{0}'", Level = EventLevel.Warning)]
        public void EmitWarningMessage(string message)
        {
            WriteEvent(1, message);
        }

        [Event(2, Message = "New Relic: '{0}': {1}", Level = EventLevel.Warning)]
        public void EmitWarningMessageWithError(string message, string error)
        {
            WriteEvent(2, message, error);
        }

        [Event(3, Message = "New Relic: '{0}'", Level = EventLevel.Informational)]
        public void EmitInformationalMessage(string message)
        {
            WriteEvent(3, message);
        }

        [Event(4, Message = "New Relic: '{0}': {1}", Level = EventLevel.Informational)]
        public void EmitInformationalMessageWithError(string message, string error)
        {
            WriteEvent(4, message, error);
        }

        [Event(5, Message = "New Relic: '{0}'", Level = EventLevel.Error)]
        public void EmitErrorMessage(string message)
        {
            WriteEvent(5, message);
        }

        [Event(6, Message = "New Relic: '{0}': {1}", Level = EventLevel.Error)]
        public void EmitErrorMessageWithError(string message, string error)
        {
            WriteEvent(6, message, error);
        }

        // Formats the exception using the same logic that is used within the OTel SDK eventsource
        // loggers so that the log messages can be normalized to use the same culture information
        // as the rest of the log messages. For a more detailed explanation refer to the following link.
        // https://github.com/open-telemetry/opentelemetry-dotnet/pull/944#discussion_r462467096
        private static string ToInvariantString(Exception exception)
        {
            var originalUICulture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                return exception.ToString();
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
            }
        }
    }
}
