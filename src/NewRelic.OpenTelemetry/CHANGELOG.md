# Changelog

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

## [1.0.0] - 2021-02-10

* Initial release supporting exporting OpenTelemetry trace data to New Relic.

[1.0.0]: https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/tree/OpenTelemetry_v1.0.0
