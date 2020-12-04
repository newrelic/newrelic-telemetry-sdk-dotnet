# New Relic Trace Exporter for OpenTelemetry .NET

[![NuGet](https://img.shields.io/nuget/v/NewRelic.OpenTelemetry.svg)](https://www.nuget.org/packages/NewRelic.OpenTelemetry)
[![NuGet](https://img.shields.io/nuget/dt/NewRelic.OpenTelemetry.svg)](https://www.nuget.org/packages/NewRelic.OpenTelemetry)

The New Relic Trace Exporter for OpenTelemetry .NET supports .NET Framework (4.5.2+) and .NET Core applications.

## Prerequisite
* A [New Relic Insights Insert API key](https://docs.newrelic.com/docs/insights/insights-data-sources/custom-data/introduction-event-api#register).

## Installation

```
dotnet add package NewRelic.OpenTelemetry
```

## Configuration

You can configure the exporter with the following options:

* `ApiKey`: Your Insights Insert API key (required).
* `Endpoint`: New Relic endpoint address.
* `ExportProcessorType`: Whether the exporter should use
  [Batch or Simple exporting processor](https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/sdk.md#built-in-span-processors). Defaults to the batch exporting processor which is recommended for most use cases.
* `BatchExportProcessorOptions`: Configuration options for the batch exporter.
  Only used if ExportProcessorType is set to Batch.

## Next Steps
* Review these [Sample Applications](/examples/NewRelic.OpenTelemetry) for guidance on configuration and usage.

## References

* [OpenTelemetry Project](https://opentelemetry.io/)
