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
<br/>
<br/>

**Example: ASP .NET Framework Application** <br/>
In this example, an ASP.NET Framework application is configured.

In the `appSettings` section of the `web.config` file, the New Relic API Key is provided.  In the Global.asax, the data exporter is configured and a tracer is instantiated.  The controller action creates the span and handles any exceptions that may occur.


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

Global.asax
```CSharp
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
```

WeatherForecastController
```CSharp
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
```
<br/>
<br/>

## Next Steps
* Review these [Sample Applications](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/tree/master/src/OpenTelemetry.Exporter.NewRelic.Samples) for guidance on configuration and usage of the OpenTelemetry Exporter for New Relic.