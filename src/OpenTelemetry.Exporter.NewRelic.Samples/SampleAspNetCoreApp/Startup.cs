using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry;
using OpenTelemetry.Collector.AspNetCore;
using OpenTelemetry.Collector.Dependencies;
using OpenTelemetry.Exporter.NewRelic;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Export;
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

            services.AddOpenTelemetry(() =>
            {

                // Adds the New Relic Exporter loading settings from the appsettings.json
                var tracerFactory = TracerFactory.Create(b => b.UseNewRelic(Configuration)
                                                 .SetSampler(Samplers.AlwaysSample));

                var dependenciesCollector = new DependenciesCollector(new HttpClientCollectorOptions(), tracerFactory);
                var aspNetCoreCollector = new AspNetCoreCollector(tracerFactory.GetTracer(null));

                return tracerFactory;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
