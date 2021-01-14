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

* `ApiKey` (Required): Your Insights Insert API key (required).
* `Endpoint` (Optional): Endpoint to send trace data to. The endpoint defaults to New Relic's
  US data centers. For other use cases refer to
  [OpenTelemetry: Advanced configuration](https://docs.newrelic.com/docs/integrations/open-source-telemetry-integrations/opentelemetry/opentelemetry-advanced-configuration#h2-change-endpoints).
* `ExportProcessorType` (Optional): Whether the exporter should use
  [Batch or Simple exporting processor](https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/sdk.md#built-in-span-processors). Defaults to the batch exporting processor which is recommended for most use cases.
* `BatchExportProcessorOptions` (Optional): Configuration options for the batch exporter.
  Only used if ExportProcessorType is set to Batch.

## Next Steps
* Review these [Sample Applications](/examples/NewRelic.OpenTelemetry) for guidance on configuration and usage.

## Troubleshooting

The [OpenTelemetry SDK](https://github.com/open-telemetry/opentelemetry-dotnet/tree/master/src/OpenTelemetry) uses `EventSource` for its internal logging, and this package also uses `EventSource` for its internal logging so that all of the relevant logs for troubleshooting your OpenTelemetry setup can be found in one place. For more information on how to enable and use this diagnostic logging you can follow the information provided in the [OpenTelemetry Troubleshooting documentation](https://github.com/open-telemetry/opentelemetry-dotnet/tree/master/src/OpenTelemetry#troubleshooting).

## References

* [OpenTelemetry Project](https://opentelemetry.io/)
