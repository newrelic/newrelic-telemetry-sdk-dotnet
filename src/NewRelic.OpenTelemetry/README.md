# New Relic Trace Exporter for OpenTelemetry .NET

[![NuGet](https://img.shields.io/nuget/v/NewRelic.OpenTelemetry.svg)](https://www.nuget.org/packages/NewRelic.OpenTelemetry)
[![NuGet](https://img.shields.io/nuget/dt/NewRelic.OpenTelemetry.svg)](https://www.nuget.org/packages/NewRelic.OpenTelemetry)

The New Relic Trace Exporter for OpenTelemetry .NET supports .NET Framework (4.6+) and .NET Core applications.

## Prerequisite
* A [New Relic Insights Insert API key](https://docs.newrelic.com/docs/insights/insights-data-sources/custom-data/introduction-event-api#register).

## Installation

```
dotnet add package NewRelic.OpenTelemetry
```

## Configuration

You can configure the `NewRelicTraceExporter` by following the directions below:

* `ApiKey`: Your Insights Insert API key.
* `ServiceName`: Name of the service reporting telemetry.
* `TraceUrlOverride`: New Relic endpoint address.
* `SendTimeout`: Timeout in seconds.

## Next Steps
* Review these [Sample Applications](/examples/NewRelic.OpenTelemetry) for guidance on configuration and usage.

## References

* [OpenTelemetry Project](https://opentelemetry.io/)
