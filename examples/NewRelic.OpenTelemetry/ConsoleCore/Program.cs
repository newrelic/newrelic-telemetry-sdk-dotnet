using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace SampleConsoleCoreApp
{
    class Program
    {
        private const string ActivitySourceName = "NewRelic.OpenTelemetryExporter.SampleConsoleCoreApp";
        private static readonly ActivitySource SampleActivitySource = new ActivitySource(ActivitySourceName);

        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var nrApiKey = config.GetValue<string>("NewRelic:ApiKey");
            var serviceName = config.GetValue<string>("NewRelic:ServiceName");

            Console.WriteLine($"Using NR API Key {nrApiKey} and service name {serviceName} from configuration.");

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
                    options.ApiKey = nrApiKey;
                    options.ServiceName = serviceName;
                    options.AuditLoggingEnabled = true;
                }, loggerFactory)
                .AddHttpClientInstrumentation()
                .Build())
            {
                Task.Run(() => GenerateSpans());
                Console.WriteLine("Spans are being generated and exported to New Relic. Press enter to stop.");
                Console.ReadLine();
            }

            Console.WriteLine("\nFinished.");
        }

        private static void GenerateSpans()
        {
            while (true)
            {
                using (var activity = SampleActivitySource.StartActivity("SampleConsoleCoreAppSpan"))
                {
                    var message = "Hello, OpenTelemetry with New Relic!";
                    activity?.SetTag("aNumber", 42);
                    activity?.SetTag("message", message);

                    var httpClient = new HttpClient();
                    httpClient.GetAsync("https://www.newrelic.com");

                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
    }
}
