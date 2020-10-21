# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

## 1.0.0-beta.202 - 2020-10-21

* Fix issue where spans without an error would appear to have an error in the
  UI.
  ([#149](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/pull/149))

## 1.0.0-beta.200 - 2020-10-19

* NuGet package renamed from OpenTelemetry.Exporter.NewRelic to
  NewRelic.OpenTelemetry.
  ([#144](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/pull/144))
* Renamed configuration and moved its namespace from
  `NewRelic.Telemetry.TelemetryConfiguration` to
  `NewRelic.OpenTelemetry.NewRelicExporterOptions`.
  ([#146](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/pull/146))
* Update to OpenTelemetry 0.7.0-beta.1.
  ([#142](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/pull/142))

## 1.0.0-beta.194 - 2020-09-25

* Fix an issue where the serialization of data sent to New Relic would fail.
  Replaced Utf8Json dependency with System.Text.Json (netstandard2.0) and
  Newtonsoft.Json (net452).
  ([#138](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/pull/138))

## 1.0.0-beta.191 - 2020-09-21

* Update to OpenTelemetry 0.6.0-beta.1 ([#119](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/pull/119)).
* Fix `span.kind` not sent on spans ([#117](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/pull/117)).
* Removed dependency on NewRelic.Telemetry and
  Microsoft.Extensions.Configuration.Abstractions packages. Change dependecy on
  Microsoft.Extensions.Logging to Microsoft.Extensions.Logging.Abstractions
  ([#126](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/pull/126)).
* Support `net452` ([#121](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/pull/121)).
* Renamed `UseNewRelic` helper method to `AddNewRelicExporter`.
  `AddNewRelicExporter` now takes a delegate for configuring the exporter.
  Overloads of `AddNewRelicExporter` that took an `IConfiguration` instance or
  a API key have been removed.
  ([#114](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/pull/114)).

## 1.0.0-beta.164 - 2020-08-31

* Update to OpenTelemetry 0.5.0-beta.2 ([#96](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/pull/96)).

## 1.0.0-beta.159 - 2020-07-27

* Update to OpenTelemetry 0.4.0-beta.2 ([#89](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/pull/89)).

## 1.0.0-beta.158 - 2020-07-24

* Update to OpenTelemetry 0.3.0-beta.1 ([#88](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/pull/88)).

## 1.0.0-beta.134 - 2020-03-11

* Modified the OpenTelemetry Trace Exporter to set attribute `instrumentation.provider` to "opentelemetry" on all spans.
* Modified the OpenTelemetry Trace Exporter to append `Status.Description` as attribute `error.message` for all spans that do not have `Status.OK`

## 1.0.0-beta - 2019-12-11

* Initial beta release of the OpenTelemetry Exporter for .NET. Supports sending spans to the New Relic endpoint for visualization in the New Relic UI.
