using ASPNetFrameworkApiApplication.Models;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace SampleAspNetFrameworkApp.Controllers
{
    public class WeatherForecastController : ApiController
    { 
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var span = WebApiApplication.OTTracer.StartRootSpan("/WeatherForecastController/Get");

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
                span.Status = Status.Internal;
                throw;
            }
            // In all cases, the span is sent up to the New Relic endpoint.
            finally
            {
                span.End();
            }
        }
    }
}
