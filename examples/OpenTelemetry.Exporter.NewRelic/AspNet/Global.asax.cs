using System;
using System.Configuration;
using System.Web.Http;
using NewRelic.Telemetry;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace SampleAspNetFrameworkApp
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private IDisposable openTelemetry;

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Obtain the API Key from the Web.Config file
            var apiKey = ConfigurationManager.AppSettings["NewRelic.Telemetry.ApiKey"];

            // Initialize OpenTelemetry and register the New Relic Exporter
            this.openTelemetry = Sdk.CreateTracerProviderBuilder()
                .UseNewRelic(config =>
                    config
                        .WithApiKey(apiKey)
                        .WithServiceName("aspnet-example"))
                .AddAspNetInstrumentation()
                .AddHttpClientInstrumentation()
                .Build();
        }

        protected void Application_End()
        {
            this.openTelemetry?.Dispose();
        }
    }
}
