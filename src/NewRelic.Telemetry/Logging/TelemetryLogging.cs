// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using Microsoft.Extensions.Logging;

namespace NewRelic.Telemetry
{
     /// <summary>
     /// Manages logging within the Telemetry SDK.
     /// </summary>
    public class TelemetryLogging
    {
        private const string Prefix = "NewRelic Telemetry:";
        private const string Category = "NewRelic.Telemetry";

        private readonly ILogger? _logger;

        private static string MessageFormatter(object state, Exception error)
        {
            return $"{Prefix} {state} {error}".Trim();
        }

        internal TelemetryLogging(ILoggerFactory? loggerFactory)
        {
            if (loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger(Category);
            }
        }

        internal void Debug(string message, Exception? exception = null)
        {
            _logger?.Log(LogLevel.Debug, 0, message, exception, MessageFormatter);
        }

        internal void Error(string message, Exception? exception = null)
        {
            _logger?.Log(LogLevel.Error, 0, message, exception, MessageFormatter);
        }

        internal void Exception(Exception ex)
        {
            _logger?.Log(LogLevel.Error, 0, ex.GetType().Name, ex, MessageFormatter);
        }

        internal void Info(string message, Exception? exception = null)
        {
            _logger?.Log(LogLevel.Information, 0, message, exception, MessageFormatter);
        }

        internal void Warning(string message, Exception? exception = null)
        {
            _logger?.Log(LogLevel.Warning, 0, message, exception, MessageFormatter);
        }
    }
}