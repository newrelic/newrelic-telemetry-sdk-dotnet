# New Relic OpenTelemetry Trace Exporter for .NET

The New Relic Data Exporter is a OpenTelemetry Provider that sends data to New Relic.



## Prerequisites
* A valid New Relic <a target="_blank" href="https://docs.newrelic.com/docs/insights/insights-data-sources/custom-data/introduction-event-api#register">Insights Insert API Key</a>.
* A .NET Core 2.0+ or .NET Framework 4.6+ Application

## Getting Started
* Incorporate the [OpenTelemetry.Exporter.NewRelic](https://www.nuget.org/packages/OpenTelemetry.Exporter.NewRelic) NuGet Packge into your project.

## Configuration



**Example: ASP .NET Core  Application** <br/>
In this example, an ASP.NET Core application is configured.

In the `NewRelic` section of the `appsettings.json` file, the New Relic API Key and Service Name are provided. 

During startup, OpenTelemetry is added as a Service which is configured to use the New Relic Data Exporter for Traces.  Additionally, the `AspNetCoreCollector` is configured to collect telemetry information from the ASP.NET pipeline.

appsettings.json 
```JSON
{
	"Logging": {
		"LogLevel": {
			"Default": "Information",
			"Microsoft": "Warning",
			"Microsoft.Hosting.Lifetime": "Information"
		}
	},

	"AllowedHosts": "*",

	"NewRelic": {
		"ApiKey": "YOUR KEY GOES HERE",
		"ServiceName": "SampleAspNetCoreApp"
	}
}
```

Startup.cs <br/>
```CSharp
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

```



**Example: ASP .NET Framework Application** <br/>
In this example, an ASP.NET Framework application is configured.

In the `appSettings` section of the `web.config` file, the New Relic API Key is provided.

in Your ______GLOBAL.ASAX?______


web.config 
```XML
<configuration>
  <appSettings>
    <add key="NewRelic.Telemetry.ApiKey" value="YOUR KEY GOES HERE" />
    <add key="serilog:using:File" value="Serilog.Sinks.File" />
    <add key="serilog:write-to:File.path" value="C:\logs\SerilogExample.log.json" />
  </appSettings>
  ...
</configuration>

```

```CSharp
IS THIS MY GLOBAL ASAX
```






## Limitations
The New Relic Telemetry APIs are rate limited. Please reference the documentation for New Relic Metrics API and New Relic Trace API Requirements and Limits on the specifics of the rate limits.

## Contributing
Full details are available in our CONTRIBUTING.md file. We'd love to get your contributions to improve the Telemetry SDK for .NET! Keep in mind when you submit your pull request, you'll need to sign the CLA via the click-through using CLA-Assistant. You only have to sign the CLA one time per project. To execute our corporate CLA, which is required if your contribution is on behalf of a company, or if you have any questions, please drop us an email at open-source@newrelic.com.


## Open Source License
This project is distributed under the [Apache 2 license](LICENSE).


## Support
New Relic has open-sourced this project. This project is provided AS-IS WITHOUT WARRANTY OR DEDICATED SUPPORT. Issues and contributions should be reported to the project here on GitHub.

We encourage you to bring your experiences and questions to the [Explorers Hub](https://discuss.newrelic.com) where our community members collaborate on solutions and new ideas.
