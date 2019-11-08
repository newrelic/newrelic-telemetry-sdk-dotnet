using Microsoft.Extensions.Logging;
using System;

namespace NewRelic.Telemetry
{
    static public class Logging
    {
        private static ILoggerFactory _factory;
        private const string Category = "NewRelic.Telemetry";
        private static ILogger _logger;

        private static readonly Func<object, Exception, string> _messageFormatter = new Func<object, Exception, string>(MessageFormatter);
        private const string NEW_RELIC = "NewRelic: ";

        private static string MessageFormatter(object state, Exception error)
        {
            return NEW_RELIC + state.ToString();
        }

        public static ILoggerFactory LoggerFactory
        {
            set 
            { 
                _factory = value;
                _logger = _factory.CreateLogger(Category);
            }
        }

        public static void LogDebug(string message, Exception exception = null)
        {
            _logger?.Log(LogLevel.Debug, 0, message, exception, _messageFormatter);
        }

        public static void LogError(string message, Exception exception = null)
        {
            _logger?.Log(LogLevel.Error, 0, message, exception, _messageFormatter);
        }

        public static void LogInformation(string message, Exception exception = null)
        {
            _logger?.Log(LogLevel.Information, 0, message, exception, _messageFormatter);
        }

        public static void LogWarning(string message, Exception exception = null)
        {
            _logger?.Log(LogLevel.Warning, 0, message, exception, _messageFormatter);
        }
    }
}