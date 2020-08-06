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
using NewRelic.Telemetry.Transport;

namespace ASPNetFrameworkApiApplication.Controllers
{

    [Route("[controller]")]
    public class WeatherForecastController : ApiController
    {

        private readonly Microsoft.Extensions.Logging.ILogger _logger;

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

            _logger = loggerFactory.CreateLogger<WeatherForecastController>();
            
            // Create a new Telemetry Configuration Object with the API Key retrieved
            // from the Web.Config
            var telemetryConfig = new TelemetryConfiguration().WithApiKey(apiKey);

            // Instantiate the SpanDataSender which manages the communication with New
            // Relic endpoints
            _spanDataSender = new SpanDataSender(telemetryConfig, loggerFactory);
        }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        [Route("api/WeatherForecast/Get")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {

            // The SpanBuilder is a tool to help Build spans.  Each span must have 
            // a unique identifier.  In this example, we are using a Guid.
            var spanId = Guid.NewGuid().ToString();

            var spanBuilder = SpanBuilder.Create(spanId);

            // We can add additional attribution to a span using helper functions.
            // In this case a timestamp and the controller action name are recorded
            spanBuilder.WithTimestamp(DateTimeOffset.UtcNow)
                .WithName("WeatherForecast/Get");

            // Wrapping the unit of work inside a try/catch is helpful to ensure that
            // spans are always reported to the endpoint, even if they have exceptions.
            try
            {
                // This is the unit of work being tracked by the span.
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
            // If an unhandled exception occurs, it can be denoted on the span.
            catch (Exception ex)
            {
                // In the event of an exception
                spanBuilder.HasError(true);
                spanBuilder.WithAttribute("Exception", ex);

                //This ensures that tracking of spans doesn't interfere with the normal execution flow
                throw;
            }
            // In all cases, the span is sent up to the New Relic endpoint.
            finally
            {
                // Obtain the span from the SpanBuilder.
                var span = spanBuilder.Build();

                // The SpanBatchBuilder is a tool to help create SpanBatches
                // Create a new SpanBatchBuilder and associate the span to it.
                var spanBatchBuilder = SpanBatchBuilder.Create()
                    .WithSpan(span);

                // Since this SpanBatch represents a single trace, identify
                // the TraceId for the entire batch.
                spanBatchBuilder.WithTraceId(Guid.NewGuid().ToString());

                // Obtain the spanBatch from the builder
                var spanBatch = spanBatchBuilder.Build();

                // Send it to the New Relic endpoint.
                var newRelicResult = await _spanDataSender.SendDataAsync(spanBatch);

                if (newRelicResult.ResponseStatus == NewRelicResponseStatus.Failure)
                {
                    _logger.LogWarning("There was a problem sending the SpanBatch to New Relic endpoint");
                }
            }
        }
    }
}
