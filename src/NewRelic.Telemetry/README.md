# New Relic Telemetry SDK for .NET

The New Relic Telemetry SDK for .NET sends Telemetry Data to New Relic.


### Prerequisites
* A valid New Relic <a target="_blank" href="https://docs.newrelic.com/docs/insights/insights-data-sources/custom-data/introduction-event-api#register">Insights Insert API Key</a>.
* A .NET Core 2.0+ or .NET Framework 4.5+ Application
### Getting Started
* Incorporate the [NewRelic.Telemetry](https://www.nuget.org/packages/NewRelic.Telemetry) **VERIFY THIS** NuGet Packge into your project.


### Traces
##### Settings Based Configuration
We recommend configuring the Telemetry SDK using ```appsettings.json```.  To do so, the add a ```NewRelic``` configuration section with your New Relic API Key.  Below is an example ASP.Net Core configuration.

```JSON
{
  "NewRelic": {
    "ApiKey" : "/* YOUR API KEY GOES HERE */"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }SampleAspNetCoreWebApi
  },
  "AllowedHosts": "*",
}
```

##### Invoking Data Sender
The ```SpanDataSender``` manages the communication with New Relic Endpoints.  In order to send trace information to New Relic, a ```SpanDataSender``` must be instantiated.


> ##### Example: ASP.Net Core Web API application with Settings Based Configuration
> The following ASP.Net Core example, configuration and logging providers are dependency-injected into the ```WeatherForecastController``` controller's constructor.  The constructor instantiates a ```SpanDataSender``` passing the configurtion and log providers.

> ```CSharp
> /// <summary>
> /// Use dependency injection in the constructor to pass in the Logger Factory and
> /// the Configuration Provider.
> /// </summary>
> public WeatherForecastController(ILoggerFactory loggerFactory, IConfiguration configProvider)
> {
>     // Make logging available to the methods in the controller
>     _logger = loggerFactory.CreateLogger<WeatherForecastController>();
> 
>     // Instantiate the SpanDataSender which manages the communication with New
>     // Relic endpoints
>     _spanDataSender = new SpanDataSender(configProvider, loggerFactory);
> }
>```


> ##### Example: .NET Framework Web API Application using Web.Config Based Configuration
> The following example is a .NET Framework application.  The ```WeatherForecastController``` reads the ```web.config``` to obtain the New Relic API Key.  It creates a ```LoggerFactory``` and instantiates the data sender
> 
> ```CSharp 
> public class WeatherForecastController : ApiController
> {
>     // Handle to the Data Sender which manages the communication with the New Relic endpoint
>     private readonly SpanDataSender _spanDataSender;
> 
>     public WeatherForecastController()
>     {
>         // Obtain the API from the Web.Config file
>         var apiKey = ConfigurationManager.AppSettings["NewRelic.Telemetry.ApiKey"];
> 
>         // Create a Logger Factory using settings in the Web.Config File
>         var loggerConfig = new LoggerConfiguration()
>             .ReadFrom.AppSettings();
> 
>         var loggerFactory = new LoggerFactory()
>             .AddSerilog(loggerConfig.CreateLogger());
> 
>         // Create a new Telemetry Configuration Object with the API Key retrieved
>         // from the Web.Config
>         var telemetryConfig = new TelemetryConfiguration().WithAPIKey(apiKey);
> 
>         // Instantiate the SpanDataSender which manages the communication with New
>         // Relic endpoints
>         _spanDataSender = new SpanDataSender(telemetryConfig, loggerFactory);
>     } 
> }
> ```







##### Building SpanBatches and Spans
###### The difference between SpanBatches and Traces
##### Sending Data to the New Relic Endpoint

### Example




### Limitations
The New Relic Telemetry APIs are rate limited. Please reference the documentation for the [New Relic Trace API](https://docs.newrelic.com/docs/data-ingest-apis/get-data-new-relic/metric-api/metric-api-limits-restricted-attributes) for the specifcs  rate limits.



### Contributing
Full details are available in our [CONTRIBUTING.md](../../CONTRIBUTING.md) file. We'd love to get your contributions to improve the Telemetry SDK for .NET! Keep in mind when you submit your pull request, you'll need to sign the CLA via the click-through using CLA-Assistant. You only have to sign the CLA one time per project. To execute our corporate CLA, which is required if your contribution is on behalf of a company, or if you have any questions, please drop us an email at open-source@newrelic.com.


### Open Source License
This project is distributed under the [Apache 2 license](LICENSE).


### Support
New Relic has open-sourced this project. This project is provided AS-IS WITHOUT WARRANTY OR DEDICATED SUPPORT. Issues and contributions should be reported to the project here on GitHub.

We encourage you to bring your experiences and questions to the [Explorers Hub](https://discuss.newrelic.com) where our community members collaborate on solutions and new ideas.
