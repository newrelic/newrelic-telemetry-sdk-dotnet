using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace SampleAspNetCoreApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var apiKey = Environment.GetEnvironmentVariable("NewRelic:ApiKey");

            if (string.IsNullOrEmpty(apiKey)) 
            {
                Console.WriteLine("NewRelic:ApiKey environment variable is either null, empty or does not exist. The api key might be set from the appsettings.json");
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        var env = hostingContext.HostingEnvironment;
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
