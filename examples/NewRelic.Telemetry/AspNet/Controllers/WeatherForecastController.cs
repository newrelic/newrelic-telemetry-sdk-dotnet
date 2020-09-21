using Serilog;
using System.Configuration;
using System.Web.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ASPNetFrameworkApiApplication.Models;
using System.Linq;
using NewRelic.Telemetry;
using NewRelic.Telemetry.Transport;
using NewRelic.Telemetry.Tracing;

namespace ASPNetFrameworkApiApplication.Controllers
{

    [Route("[controller]")]
    public class WeatherForecastController : ApiController
    {

        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        // Handle to the Data Sender which manages the communication with the New Relic endpoint
        private readonly TraceDataSender _spanDataSender;

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
            var telemetryConfig = new TelemetryConfiguration() { ApiKey = apiKey };

            // Instantiate the SpanDataSender which manages the communication with New
            // Relic endpoints
            _spanDataSender = new TraceDataSender(telemetryConfig, loggerFactory);
        }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        [Route("api/WeatherForecast/Get")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            // Each span must have a unique identifier.  In this example, we are using a Guid.
            var spanId = Guid.NewGuid().ToString();
            var spanTimeStamp = DateTimeOffset.UtcNow;
            var spanAttribs = new Dictionary<string, object>();

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
                spanAttribs[NewRelicConsts.Tracing.AttribNameHasError] = true;
                spanAttribs[NewRelicConsts.Tracing.AttribNameErrorMsg] = ex;

                //This ensures that tracking of spans doesn't interfere with the normal execution flow
                throw;
            }
            // In all cases, the span is sent up to the New Relic endpoint.
            finally
            {
                spanAttribs[NewRelicConsts.Tracing.AttribNameName] = "WeatherForecast/Get";
                spanAttribs[NewRelicConsts.Tracing.AttribNameDurationMs] = DateTimeOffset.UtcNow.Subtract(spanTimeStamp).TotalMilliseconds;


                var span = new NewRelicSpan(
                    traceId: Guid.NewGuid().ToString(),
                    spanId: spanId,
                    timestamp: spanTimeStamp.ToUnixTimeMilliseconds(),
                    parentSpanId: null,
                    attributes: spanAttribs);

                // Send it to the New Relic endpoint.
                var newRelicResult = await _spanDataSender.SendDataAsync(new[] { span });

                if (newRelicResult.ResponseStatus == NewRelicResponseStatus.Failure)
                {
                    _logger.LogWarning("There was a problem sending the SpanBatch to New Relic endpoint");
                }
            }
        }
    }
}
