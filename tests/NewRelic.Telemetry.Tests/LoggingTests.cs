// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NewRelic.Telemetry.Tests
{
    public class LoggingTests
    {
        [Fact]
        public void TestLogging()
        {
            var loggerFactory = new LoggerFactory();
            var customLogProvider = new CustomLoggerProvider();

            loggerFactory.AddProvider(customLogProvider);

            var tl = new TelemetryLogging(loggerFactory);

            var ex = new Exception("Test Exception level logging");

            tl.Debug("debug level logging message.");
            tl.Info("information level logging message.");
            tl.Warning("warning level logging message.");
            tl.Error("error level logging message.");
            tl.Exception(ex);

            Assert.True(customLogProvider.LogOutput.ContainsKey("NewRelic.Telemetry"));
            var logs = customLogProvider.LogOutput["NewRelic.Telemetry"];

            Assert.Equal(5, logs.Count);
            Assert.Contains("NewRelic Telemetry: debug level logging message.", logs);
            Assert.Contains("NewRelic Telemetry: information level logging message.", logs);
            Assert.Contains("NewRelic Telemetry: warning level logging message.", logs);
            Assert.Contains("NewRelic Telemetry: error level logging message.", logs);
            Assert.Contains($"NewRelic Telemetry: Exception {ex.GetType().FullName}: {ex.Message}", logs);
        }
    }
 }
