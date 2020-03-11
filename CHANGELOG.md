# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- Added Configuration Setting `InstrumentationProvider` which identifies any service that uses the Telemetry SDK to create Span Events.
- `SpanBuilder` allows the reporting error conditions using overload `HasError(string message)`.
- `SpanBuilder` clears the value of the `error.message` attribute when `HasError(false)` is called (ie. no error).

### Changed
- Modified the OpenTelemetry Trace Exporter to set attribute `instrumentation.provider` to "opentelemetry" on all spans.
- Modified the OpenTelemetry Trace Exporter to append `Status.Description` as attribute `error.message` for all spans with that do not have `Status.OK`

## [1.0.0-beta.117] - 2020-01-17
### Added
- Support for sending Metrics to the New Relic endpoint.
### Changed
- Renamed configuration API `WithOverrideEndpointUrl_Trace` to `WithOverrideEndpointUrlTrace`.
- Renamed configuration API` WithAPIKey` to `WithApiKey`.

## [1.0.0-beta] - 2019-12-11
### Added
- Initial beta release of the New Relic Telemetry SDK and OpenTelemetry Exporter for .NET. Supports sending spans to the New Relic endpoint for visualization in the New Relic UI.



[Unreleased]: https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/compare/76cb4c5..HEAD
[1.0.0-beta.117]: https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/compare/v1.0.0-beta..76cb4c5
[1.0.0-beta]: https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/releases/tag/v1.0.0-beta
