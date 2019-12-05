using Serilog;
using System.Configuration;
using System.Web.Http;
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry;
using NewRelic.Telemetry.Spans;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ASPNetFrameworkApiApplication.Models;
using System.Linq;

namespace ASPNetFrameworkApiApplication.Controllers
{

    [Route("[controller]")]
    public class WeatherForecastController : ApiController
    {
        // Handle to the Data Sender which manages the communication with the New Relic endpoint
        private readonly SpanDataSender _spanDataSender;

        public WeatherForecastController()
        {
            // Obtain the API from the Web.Config file
            var apiKey = ConfigurationManager.AppSettings["NewRelic.Telemetry.ApiKey"];

            // Create a Logger Factory using settings in the Web.Config File
            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.AppSettings();

            var loggerFactory = new LoggerFactory()
                .AddSerilog(loggerConfig.CreateLogger());


            // Create a new Telemetry Configuration Object with the API Key retrieved
            // from the Web.Config
            var telemetryConfig = new TelemetryConfiguration().WithAPIKey(apiKey);

            // Instantiate the SpanDataSender which manages the communication with New
            // Relic endpoints
            _spanDataSender = new SpanDataSender(telemetryConfig, loggerFactory);
        }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var spanBuilder = SpanBuilder.Create(Guid.NewGuid().ToString())
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithName("WeatherForecase/Get");

            try
            {
                var rng = new Random();
                var result = Enumerable.Range(1, 5)
                    .Select(index => new WeatherForecast()
                    {
                        Date = DateTime.Now.AddDays(index),
                        TemperatureC = rng.Next(-20, 55),
                        Summary = Summaries[rng.Next(Summaries.Length)]
                    })
                    .ToArray();

                return result;
            }
            catch (Exception ex)
            {
                spanBuilder.HasError(true);
                spanBuilder.WithAttribute("Exception", ex);
                throw;
            }
            finally
            {
                var span = spanBuilder.Build();

                var spanBatchBuilder = SpanBatchBuilder.Create()
                    .WithTraceId(Guid.NewGuid().ToString());

                spanBatchBuilder.WithSpan(span);

                var spanBatch = spanBatchBuilder.Build();

                await _spanDataSender.SendDataAsync(spanBatch);
            }
        }
    }
}
