using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NewRelic.Telemetry.Metrics;

namespace SampleAspNetCoreApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IConfiguration _config;

        public WeatherForecastController(IConfiguration config) 
        {
            _config = config;
        }


        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var metricBuilder = MetricBuilder.CreateCountMetric("WeatherForecast/Get")
            .WithTimestamp(DateTime.Now)
            .WithValue(1)
            .WithIntervalMs(10)
            .WithAttribute("testAttributeKey1", "testAttributeValue1");
            
            var metric = metricBuilder.Build();

            var metricBatch = MetricBatchBuilder.Create()
            .WithMetric(metric)
            .Build();

            var dataSender = new MetricDataSender(_config);
            var response = dataSender.SendDataAsync(metricBatch).Result;

            HttpClient client = new HttpClient();
            HttpResponseMessage ret = client.GetAsync("http://www.newrelic.com").Result;

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
