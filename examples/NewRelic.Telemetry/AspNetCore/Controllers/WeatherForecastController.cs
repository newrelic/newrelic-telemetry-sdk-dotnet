using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry.Spans;
using NewRelic.Telemetry.Transport;

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

        private readonly SpanDataSender _spanDataSender;

        /// <summary>
        /// Use dependency injection in the constructor to pass in the Logger Factory and
        /// the Configuration Provider.
        /// </summary>
		public WeatherForecastController(ILoggerFactory loggerFactory, SpanDataSender spanDataSender)
        {
            _spanDataSender = spanDataSender;
            _logger = loggerFactory.CreateLogger<WeatherForecastController>();
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {

            // The SpanBuilder is a tool to help Build spans.  Each span must have 
            // a unique identifier.  In this example, we are using a Guid.
            var spanId = Guid.NewGuid().ToString();

            var spanBuilder = SpanBuilder.Create(spanId);

            // We can add additional attribution to a span using helper functions.
            // In this case a timestamp and the controller action name are recorded
            spanBuilder.WithTimestamp(DateTimeOffset.UtcNow)
                .WithName("WeatherForecase/Get");

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
