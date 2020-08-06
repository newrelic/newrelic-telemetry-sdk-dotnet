
# Using the Telemetry SDK for Tracing
This documentation describes how to use the Telemetry SDK to track information about units of work and to send them to the New Relic endpoint.


## Configuring the SpanDataSender
The ```SpanDataSender``` manages the communication with New Relic Endpoints.  In order to send trace information to New Relic, a ```SpanDataSender``` must be instantiated.
<br/>

**Example: ASP.Net Core Web API application with Settings Based Configuration** <br/>
In the following ASP.Net Core example, configuration and logging providers are dependency-injected into the ```WeatherForecastController``` controller's constructor.  The constructor instantiates a ```SpanDataSender``` adding the configuration and log providers.

appsettings.json
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
    }
  },
  "AllowedHosts": "*",
}
```

startup.cs
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

		// Create the SpanDataSender in support of the TelemetrySDK.
		// Use the Service Provider to resolve the LoggerFactory so that it can
		// be injected into the Telemetry Provider
		services.AddSingleton<SpanDataSender>((svcProvider) =>
		{
			var loggerFactory = svcProvider.GetRequiredService<ILoggerFactory>();

			return new SpanDataSender(Configuration, loggerFactory);
		});
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseRouting();

		app.UseAuthorization();

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});
	}
}
```

Weather Forecast Controller
```CSharp
public WeatherForecastController(ILoggerFactory loggerFactory, SpanDataSender spanDataSender)
{
	_spanDataSender = spanDataSender;
	_logger = loggerFactory.CreateLogger<WeatherForecastController>();
}
```
<br/>

**Example: .NET Framework Web API Application using Web.Config Based Configuration** <br/>
The following example is a .NET Framework application.  The ```WeatherForecastController``` reads the ```web.config``` to obtain the New Relic API Key.  It creates a ```LoggerFactory``` and instantiates the data sender

Web.Config
```xml
<configuration>
  <appSettings>
    <add key="NewRelic.Telemetry.ApiKey" value="YOUR KEY GOES HERE" />
    <add key="serilog:using:File" value="Serilog.Sinks.File" />
    <add key="serilog:write-to:File.path" value="C:\logs\SerilogExample.log.json" />
  </appSettings>
...
</configuration>
```

WeatherForecast Controller
```CSharp 
public class WeatherForecastController : ApiController
{
    // Handle to the Data Sender which manages the communication with the New Relic endpoint
    private readonly SpanDataSender _spanDataSender;

    public WeatherForecastController()
    {
        // Obtain the API from the Web.Config file
        var apiKey = ConfigurationManager.AppSettings["NewRelic.Telemetry.ApiKey"];

        // Create a Logger Factory using settings in the Web.Config File
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.AppSettings();

        var loggerFactory = new LoggerFactory()
            .AddSerilog(loggerConfig.CreateLogger());

        // Create a new Telemetry Configuration Object with the API Key retrieved
        // from the Web.Config
        var telemetryConfig = new TelemetryConfiguration().WithApiKey(apiKey);

        // Instantiate the SpanDataSender which manages the communication with New
        // Relic endpoints
        _spanDataSender = new SpanDataSender(telemetryConfig, loggerFactory);
    } 
}
```
<br/>

## Traces, Spans, and Span Batches

<dl>
	<dt>Traces</dt>
	<dd>Traces describe operations to be tracked.  For example, a trace may describe the processing of an endpoint within a WebAPI application</dd>
	<dt>Spans</dt>
	<dd>Operations are often broken down into smaller units of work.  Spans describe the units of work that comprise an operation.  For example, within the processing of a controller action, various database calls may occur.  Each of these database calls may be tracked as a span that is part of the same trace.</dd>
	<dt>Span Batches</dt>
	<dd>When sending trace information to New Relic, spans are bundled into Span Batches and sent to the endpoint.  The spans within a Span Batch may, or may not be a part of the same trace.
</dd>
</dl>
<br/>


## The SpanBuilder
The `SpanBuilder` is a tool to help build new Spans.  In this example, a `SpanBuilder` is instantiated with a `SpanId` and it is associated to a trace with a `TraceId`.  The SpanId is required and must be unique.

```CSharp
var traceId = Guid.NewGuid().ToString();
var spanId = Guid.NewGuid().ToString();

var spanBuilder = SpanBuilder.Create(spanId)
	.WithTraceId(traceId);
```

**Intrinsic Attribution** <br>
Use the SpanBuilder methods to add additional information about the span. In this example, start-time, duration, and a name are added as additional attribution to the span.
```CSharp
spanBuilder.WithTimestamp(DateTime.UtcNow)
	.WithDurationMs(3192)
	.WithName("Get 5-day Forecast");
```

The following methods support adding intrinsic attributes to a span.

| Method						| Attribute							|Description																														|
| -----------					| -----------						|-------																															|
| ```WithTraceId			```	| trace.id							|Associates the span to a specific trace.  If the span batch contains spans from multiple traces, this field is required.		|
| ```WithTimeStamp			```	| timestamp							|The start time (unix timestamp in milliseconds)																					|
| ```WithDurationMs		```	| duration							|The duration (in ms) for the span's execution (including sub/child spans)															|
| ```WithExecutionTimeInfo```	| timestamp and Duration			|Given StartTime and and EndTime, calculates the duration and records the timestamp												|
| ```WithName				```	| name								|Identifies the span with a meaningful name.  This value should describe the operation, but is not a unique identifier				|
| ```WithServiceName		```	| serviceName						|Identifies the service for which the span is being recorded																		|


**Custom Attribution** <br>
The SpanBuilder also supports adding custom attributes to the span.  In the example below, the relative url is added to a custom attribute on the span.

```CSharp
spanBuilder.WithAttribute("RelativeURL", "/Weather/Forecast");
```

* Attribute Names are limited to 255-bytes.
* Attribute Values are limited to 4096-bytes.

**Errors** <br>
A span may be marked as having an error.  In this example, if the work being measured by the span encounters an exception, the span will be marked as having an error and the exception added as a custom attribute.


```CSharp
var traceId = Guid.NewGuid().ToString();
var spanId = Guid.NewGuid().ToString();

var spanBuilder = SpanBuilder.Create(spanId)
	.WithTraceId(traceId);

try
{
	//Do the work that the span is measuring
}
catch (Exception ex)
{
	spanBuilder.HasError(true);
	spanBuilder.WithAttribute("Exception", ex);
	throw;
}
finally
{
	//TODO: Attach span to batch and send to New Relic.
}
```

We recommend the use of `try/catch/finally` blocks so that if an exception occurs, the span information can still be sent to the New Relic Back End.

**Parent Spans** <br/>
When a span describes a sub-unit of work, it may be linked to its parent unit via the ParentId attribute.  In this example a child span is associated to a parent span.  Both spans are part of the same trace.

```CSharp
var traceId = Guid.NewGuid().ToString();

// Create the parent Span
var parentSpanId = Guid.NewGuid().ToString();
var parentSpanBuilder = SpanBuilder.Create(parentSpanId)
	.WithTraceId(traceId)
	.WithName("5-day forecast");

// Create the child span and assocaite it to the parent span
var childSpanId = Guid.NewGuid().ToString();
var childSpanBuilder = SpanBuilder.Create(childSpanId)
	.WithTraceId(traceId)
	.WithName("Consult The Weather Oracle")
	.WithParentId(parentSpanId);
```
<br/>

**Obtaining the Span from the SpanBuilder** <br/>
When the building of a span is complete, invoke the `Build()` method on the `SpanBuilder` to obtain the `Span` object.

```CSharp
var traceId = Guid.NewGuid().ToString();
var spanId = Guid.NewGuid().ToString();

var spanBuilder = SpanBuilder.Create(parentSpanId)
	.WithTraceId(traceId)
	.WithName("5-day forecast");

var span = spanBuilder.Build();
```

## The SpanBatchBuilder
The `SpanBatchBuilder` is a tool that manages a collection of spans to be sent to the New Relic endpoint.

This example is a single trace with two spans that are related.  Since all of the spans on the SpanBatch belong to the same Trace, the TraceId is set on the SpanBatch, as opposed to on the individual spans.

```CSharp
var traceId = Guid.NewGuid().ToString();

// Create the parent Span
var parentSpanId = Guid.NewGuid().ToString();
var parentSpan = SpanBuilder.Create(parentSpanId)
	.WithName("5-day forecast")
	.Build();

// Create the child span and assocaite it to the parent span
var childSpanId = Guid.NewGuid().ToString();
var childSpan = SpanBuilder.Create(childSpanId)
	.WithName("Consult The Weather Oracle")
	.WithParentId(parentSpanId)
	.Build();

// Bundle the two spans into a span batch
var spanBatchBuilder = SpanBatchBuilder.Create().
	.WithTraceId(traceId)
	.WithSpan(parentSpan)
	.WithSpan(childSpan);

````

**Obtaining the SpanBatch from the SpanBatchBuilder** <br/>
When the building of a `SpanBatch` is complete, invoke the `Build()` method on the `SpanBatchBuilder` to obtain the `SpanBatch` object.


```CSharp
var spanBatchBuilder = SpanBatchBuilder.Create();
	.WithTraceId(traceId)
	.WithSpan(parentSpan)
	.WithSpan(childSpan);

var spanBatch = spanBatchBuilder.Build();
```
<br/>

## Sending Data to the New Relic Trace endpoint
A SpanBatch is sent to New Relic by calling the `SendDataAsync` method on the `SpanDataSender`.

```CSharp
// Configure the SpanDataSender
var spanDataSender = new SpanDataSender(new TelemetryConfiguration()
	.WithApiKey("YOUR KEY HERE"));

// Track a unit of work with a trace and two spans
var traceId = Guid.NewGuid().ToString();

var parentSpanId = Guid.NewGuid().ToString();
var parentSpan = SpanBuilder.Create(parentSpanId)
	.WithName("5-day forecast").Build();

var childSpanId = Guid.NewGuid().ToString();
var childSpan = SpanBuilder.Create(childSpanId)
	.WithName("Consult The Weather Oracle")
	.WithParentId(parentSpanId).Build();

var spanBatch = SpanBatchBuilder.Create().
	.WithTraceId(traceId)
	.WithSpan(parentSpan)
	.WithSpan(childSpan)
	.Build();

// Send the spans to the New Relic Trace Endpoint
var newRelicResult = await _spanDataSender.SendDataAsync(spanBatch);
````
<br/>

#### Interpreting the results
The `SendDataAsync` method returns a `Response` object with the following properties.


**ResponseStatus** <br/>
Indicates the outcome of the communication with the New Relic endpoint.

<dl>
	<dt>Success</dt>
	<dd>SpanBatch was sent to the New Relic endpoint.  Further validation will occur on the backend.</dd>
	<dt>DidNotSend_NoData</dt>
	<dd>A request was made to send a SpanBatch that did not have any Spans. Not an error condition, but may indicate an unanticipated code path.</dd>
	<dt>Failure</dt>
	<dd>There was a failure during the communication with the endpoint. The other `Response` properties may indicate reasons for this failure.</dd>
</dl>
<br/>

**HttpStatusCode** <br/>
The outcome of the Http communication with the New Relic endpoint, if available.
<br/>

**Message** <br/>
Any additional information that is available to describe the outcome.
<br/>

## Next Steps
* Review these [Sample Applications](/examples/NewRelic.Telemetry) for guidance on configuration and usage of the New Relic Telemetry SDK for Tracing.
