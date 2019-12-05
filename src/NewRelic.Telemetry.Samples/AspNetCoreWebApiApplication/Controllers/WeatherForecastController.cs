using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry.Spans;

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
		public WeatherForecastController(ILoggerFactory loggerFactory, IConfiguration configProvider)
		{
            // Make logging available to the methods in the controller
            _logger = loggerFactory.CreateLogger<WeatherForecastController>();


            // Instantiate the SpanDataSender which manages the communication with New
            // Relic endpoints
            _spanDataSender = new SpanDataSender(configProvider, loggerFactory);
		}

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
            catch(Exception ex)
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
