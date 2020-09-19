# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

* Renamed `UseNewRelic` helper method to `AddNewRelicExporter`.
  `AddNewRelicExporter` now takes a delegate for configuring the exporter.
  Overloads of `AddNewRelicExporter` that took an `IConfiguration` instance or
  a API key have been removed.
  ([#114](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/pull/114)).

## [1.0.0-beta.134] - 2020-03-11

### Changed

* Modified the OpenTelemetry Trace Exporter to set attribute `instrumentation.provider` to "opentelemetry" on all spans.
* Modified the OpenTelemetry Trace Exporter to append `Status.Description` as attribute `error.message` for all spans that do not have `Status.OK`

## [1.0.0-beta] - 2019-12-11

### Added

* Initial beta release of the OpenTelemetry Exporter for .NET. Supports sending spans to the New Relic endpoint for visualization in the New Relic UI.

[Unreleased]: https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/compare/v1.0.0-beta.134..HEAD
[1.0.0-beta.134]: https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/compare/76cb4c5..v1.0.0-beta.134
[1.0.0-beta.117]: https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/compare/v1.0.0-beta..76cb4c5
[1.0.0-beta]: https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/releases/tag/v1.0.0-beta
