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

        // Set these values for yourself
        private const string MyNewRelicInsightsInsertApiKey = "Your Insights Insert API Key. See: https://docs.newrelic.com/docs/insights/insights-data-sources/custom-data/introduction-event-api#register";
        private const string MyServiceName = "SampleConsoleCoreApp";

        static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Debug)
                           .AddConsole();
                }
            );

            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource(ActivitySourceName)
                .AddNewRelicExporter(options =>
                {
                    options.ApiKey = MyNewRelicInsightsInsertApiKey;
                    options.ServiceName = MyServiceName;
                    options.AuditLoggingEnabled = true;
                }, loggerFactory)
                .AddHttpClientInstrumentation()
                .Build();

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

            Console.WriteLine("\nTrace finished, waiting ten seconds to allow trace to be collected and sent to New Relic.");
            System.Threading.Thread.Sleep(10000);
        }
    }
}
