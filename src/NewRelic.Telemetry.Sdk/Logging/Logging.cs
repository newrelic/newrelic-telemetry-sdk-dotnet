using Microsoft.Extensions.Logging;

namespace NewRelic.Telemetry.Sdk
{
    static public class Logging
    {
        private static ILoggerFactory _factory;
        private const string Category = "NewRelic.Telemetry";
        private static ILogger _logger;

        public static ILoggerFactory LoggerFactory
        {
            set 
            { 
                _factory = value;
                _logger = _factory.CreateLogger(Category);
            }
        }

        public static void LogDebug(string message) 
        {
            _logger?.LogDebug(message);
        }

        public static void LogInformation(string message)
        {
            _logger?.LogInformation(message);
        }

        public static void LogError(string message)
        {
            _logger?.LogError(message);
        }

        public static void LogWarning(string message)
        {
            _logger?.LogWarning(message);
        }
    }
}