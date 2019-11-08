using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NewRelic.Telemetry.Sdk.Tests
{
    public class LoggingTests
    {
        [Test]
        public void TestLogging()
        {
            var loggerFactory = new LoggerFactory();
            var customLogProvider = new CustomLoggerProvider();

            loggerFactory.AddProvider(customLogProvider);

            Logging.LoggerFactory = loggerFactory;

            Logging.LogDebug("debug level logging message.");
            Logging.LogInformation("information level logging message.");
            Logging.LogWarning("warning level logging message.");
            Logging.LogError("error level logging message.");

            Assert.IsTrue(customLogProvider.LogOutput.ContainsKey("NewRelic.Telemetry"));
            var logs = customLogProvider.LogOutput["NewRelic.Telemetry"];

            Assert.AreEqual(4, logs.Count);
            Assert.Contains("NewRelic: debug level logging message.", logs);
            Assert.Contains("NewRelic: information level logging message.", logs);
            Assert.Contains("NewRelic: warning level logging message.", logs);
            Assert.Contains("NewRelic: error level logging message.", logs);
        }
    }

    public class CustomLoggerProvider : ILoggerProvider
    {
        ConcurrentDictionary<string, ILogger> _loggers = new ConcurrentDictionary<string, ILogger>();

        ConcurrentDictionary<string, List<string>> _logOutput;
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
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CustomLoggerProvider()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
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
            var message = formatter(state, null);
            var logs = Provider.LogOutput.GetOrAdd(Category, new List<string>());
            logs.Add(message);
        }
    }
 }
