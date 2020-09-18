using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry.Tracing;
using NewRelic.Telemetry.Transport;
using NewRelic.Telemetry.Extensions;
using NewRelic.Telemetry;

namespace AspNetCoreWebApiApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };


        private readonly ILogger<WeatherForecastController> _logger;

        private readonly TraceDataSender _spanDataSender;

        /// <summary>
        /// Use dependency injection in the constructor to pass in the Logger Factory and
        /// the Configuration Provider.
        /// </summary>
		public WeatherForecastController(ILoggerFactory loggerFactory, TraceDataSender spanDataSender)
        {
            _spanDataSender = spanDataSender;
            _logger = loggerFactory.CreateLogger<WeatherForecastController>();
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            // Each span must have a unique identifier.  In this example, we are using a Guid.
            var spanId = Guid.NewGuid().ToString();
            var spanTimeStamp = DateTime.UtcNow;
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
                spanAttribs[NewRelicConsts.Tracing.AttribName_HasError] = true;
                spanAttribs[NewRelicConsts.Tracing.AttribName_ErrorMsg] = ex;

                //This ensures that tracking of spans doesn't interfere with the normal execution flow
                throw;
            }
            // In all cases, the span is sent up to the New Relic endpoint.
            finally
            {
                spanAttribs[NewRelicConsts.Tracing.AttribName_Name] = "WeatherForecast/Get";
                spanAttribs[NewRelicConsts.Tracing.AttribName_DurationMs] = DateTime.UtcNow.Subtract(spanTimeStamp).TotalMilliseconds;


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
