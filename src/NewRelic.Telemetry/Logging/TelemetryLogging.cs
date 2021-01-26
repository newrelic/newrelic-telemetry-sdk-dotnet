// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NewRelic.Telemetry
{
    /// <summary>
    /// Manages logging within the Telemetry SDK.
    /// </summary>
    internal class TelemetryLogging : ITelemetryLogger
    {
        private const string Prefix = "NewRelic Telemetry:";
        private const string Category = "NewRelic.Telemetry";

        private readonly ILogger _logger;

        private static string MessageFormatter(object state, Exception error)
        {
            return $"{Prefix} {state} {error}".Trim();
        }

        public TelemetryLogging(ILoggerFactory? loggerFactory)
        {
            _logger = loggerFactory?.CreateLogger(Category) ?? NullLogger.Instance;
        }

        public void Debug(string message, Exception? exception = null)
        {
            _logger.Log(LogLevel.Debug, 0, message, exception, MessageFormatter);
        }

        public void Error(string message, Exception? exception = null)
        {
            _logger.Log(LogLevel.Error, 0, message, exception, MessageFormatter);
        }

        public void Exception(Exception exception)
        {
            _logger.Log(LogLevel.Error, 0, exception.GetType().Name, exception, MessageFormatter);
        }

        public void Info(string message, Exception? exception = null)
        {
            _logger.Log(LogLevel.Information, 0, message, exception, MessageFormatter);
        }

        public void Warning(string message, Exception? exception = null)
        {
            _logger.Log(LogLevel.Warning, 0, message, exception, MessageFormatter);
        }
    }
}
