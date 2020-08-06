# New Relic Trace Exporter for OpenTelemetry .NET

[![NuGet](https://img.shields.io/nuget/v/OpenTelemetry.Exporter.NewRelic.svg)](https://www.nuget.org/packages/OpenTelemetry.Exporter.NewRelic)
[![NuGet](https://img.shields.io/nuget/dt/OpenTelemetry.Exporter.NewRelic.svg)](https://www.nuget.org/packages/OpenTelemetry.Exporter.NewRelic)

The New Relic Trace Exporter for OpenTelemetry .NET supports .NET Framework (4.6+) and .NET Core applications.

## Prerequisite
* A [New Relic Insights Insert API key](https://docs.newrelic.com/docs/insights/insights-data-sources/custom-data/introduction-event-api#register).

## Installation

```
dotnet add package OpenTelemetry.Exporter.NewRelic
```

## Configuration

You can configure the `NewRelicTraceExporter` by following the directions below:

* `ApiKey`: Your Insights Insert API key.
* `ServiceName`: Name of the service reporting telemetry.
* `TraceUrlOverride`: New Relic endpoint address.
* `SendTimeout`: Timeout in seconds.

## Next Steps
* Review these [Sample Applications](/examples/OpenTelemetry.Exporter.NewRelic) for guidance on configuration and usage.

## References

* [OpenTelemetry Project](https://opentelemetry.io/)
