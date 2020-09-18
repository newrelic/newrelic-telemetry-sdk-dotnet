// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NewRelic.Telemetry.Tests
{
    public class CustomLogger : ILogger
    {
        public CustomLoggerProvider Provider { get; private set; }

        public string Category { get; private set; }

        public CustomLogger(CustomLoggerProvider provider, string category)
        {
            Provider = provider;
            Category = category;
        }

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            var logs = Provider.LogOutput.GetOrAdd(Category, new List<string>());
            logs.Add(message);
        }
    }
 }
