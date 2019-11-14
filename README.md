# newrelic-telemetry-sdk-dotnet

## Logging

Logging in the New Relic Telemetry Sdk for .NET is designed to be vendor-agnostic. It works with any logging providers used in the host applications that support `Microsoft.Extensions.Logging` (i.e. Serilog, NLog ...). In order to link the host application logging provider to the Sdk, use the following code:

	//loggerFactory is an instance of type Microsoft.Extensions.Logging.LoggerFactory 
	NewRelic.Telemetry.Logging.LoggerFactory = loggerFactory;

For most .NET web applications, the `loggerFactory` instance is available via dependency injection. It can be wired up with the Sdk inside the `Configure` method in the `Startup.cs`.

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
	{
		NewRelic.Telemetry.Logging.LoggerFactory = loggerFactory;
		/*...User configuration code...*/
	}
