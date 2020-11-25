// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics.Tracing;

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
                    EmitInformationalMessage(message, exception.ToString());
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
                    EmitErrorMessage(message, exception.ToString());
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
                EmitErrorMessage(exception.ToString());
            }
        }

        [NonEvent]
        public void Info(string message, Exception? exception = null)
        {
            if (IsEnabled(EventLevel.Informational, (EventKeywords)(-1)))
            {
                if (exception != null)
                {
                    EmitInformationalMessage(message, exception.ToString());
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
                    EmitWarningMessage(message, exception.ToString());
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
        public void EmitWarningMessage(string message, string error)
        {
            WriteEvent(1, message, error);
        }

        [Event(3, Message = "New Relic: '{0}'", Level = EventLevel.Informational)]
        public void EmitInformationalMessage(string message)
        {
            WriteEvent(1, message);
        }

        [Event(4, Message = "New Relic: '{0}': {1}", Level = EventLevel.Informational)]
        public void EmitInformationalMessage(string message, string error)
        {
            WriteEvent(1, message, error);
        }

        [Event(5, Message = "New Relic: '{0}'", Level = EventLevel.Error)]
        public void EmitErrorMessage(string message)
        {
            WriteEvent(1, message);
        }

        [Event(6, Message = "New Relic: '{0}': {1}", Level = EventLevel.Error)]
        public void EmitErrorMessage(string message, string error)
        {
            WriteEvent(1, message, error);
        }
    }
}
