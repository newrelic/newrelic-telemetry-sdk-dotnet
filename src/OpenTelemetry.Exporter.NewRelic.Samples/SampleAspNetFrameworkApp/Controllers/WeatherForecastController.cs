using ASPNetFrameworkApiApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IEnumerable<WeatherForecast> Get()
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
    }
}
