using System.Configuration;
using System.Web.Http;
using OpenTelemetry.Collector.Dependencies;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Exporter.NewRelic;
using OpenTelemetry.Trace.Sampler;
using OpenTelemetry.Trace;

namespace SampleAspNetFrameworkApp
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        // Static handle to the OpenTelemetry Tracer
        public static ITracer OTTracer;

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Obtain the API Key from the Web.Config file
            var apiKey = ConfigurationManager.AppSettings["NewRelic.Telemetry.ApiKey"];

            // Create the tracer factory registering New Relic as the Data Exporter
            var tracerFactory = TracerFactory.Create((b) =>
            {
                b.UseNewRelic(apiKey)
                .AddDependencyCollector()
                .SetSampler(Samplers.AlwaysSample);
            });

            var dependenciesCollector = new DependenciesCollector(new HttpClientCollectorOptions(), tracerFactory);

            // Make the tracer available to the application
            OTTracer = tracerFactory.GetTracer("SampleAspNetFrameworkApp");
        }
    }
}
