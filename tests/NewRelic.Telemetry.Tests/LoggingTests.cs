// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace NewRelic.Telemetry.Tests
{
    public class LoggingTests
    {
        [Test]
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

            Assert.IsTrue(customLogProvider.LogOutput.ContainsKey("NewRelic.Telemetry"));
            var logs = customLogProvider.LogOutput["NewRelic.Telemetry"];

            Assert.AreEqual(5, logs.Count);
            Assert.Contains("NewRelic Telemetry: debug level logging message.", logs);
            Assert.Contains("NewRelic Telemetry: information level logging message.", logs);
            Assert.Contains("NewRelic Telemetry: warning level logging message.", logs);
            Assert.Contains("NewRelic Telemetry: error level logging message.", logs);
            Assert.Contains($"NewRelic Telemetry: Exception {ex.GetType().FullName}: {ex.Message}", logs);
        }
    }

    public class CustomLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new ConcurrentDictionary<string, ILogger>();

        private ConcurrentDictionary<string, List<string>> _logOutput;

        public ConcurrentDictionary<string, List<string>> LogOutput
        {
            get
            {
                if (_logOutput == null)
                {
                    _logOutput = new ConcurrentDictionary<string, List<string>>();
                }

                return _logOutput;
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, new CustomLogger(this, categoryName));
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                _disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    public class CustomLogger : ILogger
    {
        public CustomLoggerProvider Provider { get; private set; }
        public string Category { get; private set; }

        public CustomLogger(CustomLoggerProvider Provider, string Category)
        {
            this.Provider = Provider;
            this.Category = Category;
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
