# New Relic Trace Exporter for OpenTelemetry .NET

[![NuGet](https://img.shields.io/nuget/v/NewRelic.OpenTelemetry.svg)](https://www.nuget.org/packages/NewRelic.OpenTelemetry)
[![NuGet](https://img.shields.io/nuget/dt/NewRelic.OpenTelemetry.svg)](https://www.nuget.org/packages/NewRelic.OpenTelemetry)

## :exclamation: Deprecation Notice :exclamation:

The NewRelic.OpenTelemetry package has been deprecated and will no longer be
maintained. It included an exporter for sending OpenTelemetry trace data over
New Relic's proprietary ingest protocol.

Rather than developing and maintaining its own OpenTelemetry exporters, New
Relic now supports ingesting data using the OpenTelemetry Protocol (OTLP). OTLP
is an open source and vendor agnostic protocol.

Configure your application to send data to New Relic's OTLP data ingestion
endpoint using the
[OpenTelemetry.Exporter.OpenTelemetryProtocol](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol)
package.

For more information on sending data to New Relic over OTLP see our
[OpenTelemetry quick start guide](https://docs.newrelic.com/docs/more-integrations/open-source-telemetry-integrations/opentelemetry/opentelemetry-quick-start)
.

The New Relic Trace Exporter for OpenTelemetry .NET supports .NET Framework (4.5.2+) and .NET Core applications.

## Prerequisite
* A [New Relic Insights Insert API key](https://docs.newrelic.com/docs/insights/insights-data-sources/custom-data/introduction-event-api#register).

## Installation

```
dotnet add package NewRelic.OpenTelemetry
```

## Configuration

You can configure the exporter with the following options:

* `ApiKey` (Required): Your New Relic
  [Insights Insert API Key](https://docs.newrelic.com/docs/insights/insights-data-sources/custom-data/send-custom-events-event-api#register).
* `Endpoint` (Optional): Endpoint to send trace data to. The endpoint defaults to New Relic's
  US data centers. For other use cases refer to
  [OpenTelemetry: Advanced configuration](https://docs.newrelic.com/docs/integrations/open-source-telemetry-integrations/opentelemetry/opentelemetry-advanced-configuration#h2-change-endpoints).
* `ExportProcessorType` (Optional): Whether the exporter should use
  [Batch or Simple exporting processor](https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/sdk.md#built-in-span-processors). Defaults to the batch exporting processor which is recommended for most use cases.
* `BatchExportProcessorOptions` (Optional): Configuration options for the batch exporter.
  Only used if ExportProcessorType is set to Batch.

## References

* [OpenTelemetry Project](https://opentelemetry.io/)
