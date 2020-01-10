using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry.Metrics;
using OpenTelemetry.Collector.AspNetCore;
using OpenTelemetry.Collector.Dependencies;
using OpenTelemetry.Exporter.NewRelic;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Sampler;

namespace SampleAspNetCoreApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddOpenTelemetry((svcProvider, tracerBuilder) =>
            {
                // Make the logger factory available to the dependency injection
                // container so that it may be injected into the OpenTelemetry Tracer.
                var loggerFactory = svcProvider.GetRequiredService<ILoggerFactory>();

                // Adds the New Relic Exporter loading settings from the appsettings.json
                var tracerFactory = TracerFactory.Create(b => b.UseNewRelic(Configuration, loggerFactory)
                                                 .SetSampler(Samplers.AlwaysSample));

                var dependenciesCollector = new DependenciesCollector(new HttpClientCollectorOptions(), tracerFactory);
                var aspNetCoreCollector = new AspNetCoreCollector(tracerFactory.GetTracer(null));
            });

            services.AddSingleton<MetricDataSender, MetricDataSender>();
            services.AddSingleton<CountMetricGenerator, CountMetricGenerator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
