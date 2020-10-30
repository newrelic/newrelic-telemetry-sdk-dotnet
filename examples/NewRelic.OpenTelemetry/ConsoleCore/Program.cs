using System;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace SampleConsoleCoreApp
{
    class Program
    {
        private const string ActivitySourceName = "NewRelic.OpenTelemetryExporter.SampleConsoleCoreApp";
        private static readonly ActivitySource SampleActivitySource = new ActivitySource(ActivitySourceName);

        // Set these values by environment variable
        private static readonly string MyNewRelicInsightsInsertApiKey = Environment.GetEnvironmentVariable("NR_API_KEY");
        private static readonly string MyServiceName = Environment.GetEnvironmentVariable("NR_SAMPLE_APP_NAME") ?? "SampleConsoleCoreApp";

        static void Main(string[] args)
        {

            if (MyNewRelicInsightsInsertApiKey == null)
            {
                Console.WriteLine("Please provide your New Relic Insights Insert API key by setting the 'NR_API_KEY' environment variable.");
                return;
            }

            var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Debug)
                           .AddConsole();
                }
            );

            using (var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource(ActivitySourceName)
                .AddNewRelicExporter(options =>
                {
                    options.ApiKey = MyNewRelicInsightsInsertApiKey;
                    options.ServiceName = MyServiceName;
                    options.AuditLoggingEnabled = true;
                }, loggerFactory)
                .AddHttpClientInstrumentation()
                .Build())
            {
                using (var activity = SampleActivitySource.StartActivity("SampleConsoleCoreAppSpan"))
                {
                    Console.WriteLine("Creating root of trace.");

                    var message = "Hello, OpenTelemetry with New Relic!";
                    activity?.SetTag("aNumber", 42);
                    activity?.SetTag("message", message);

                    Console.WriteLine("\nMaking an external HTTP request which will be added as a child span of the trace.");

                    var httpClient = new HttpClient();
                    var request = httpClient.GetAsync("https://www.newrelic.com");

                    Console.WriteLine($"Web request result: {request.Result.StatusCode}");
                }
            }

            Console.WriteLine("\nFinished.");
        }
    }
}
