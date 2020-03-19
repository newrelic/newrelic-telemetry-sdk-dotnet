# New Relic Telemetry SDK and OpenTelemetry support for .NET

This repo contains the New Relic Telemetry SDK and an OpenTelemetry provider for .NET.


### [New Relic OpenTelemetry trace exporter](./src/OpenTelemetry.Exporter.NewRelic/README.md)
[OpenTelemetry](https://opentelemetry.io/) is a set of APIs that aim to standardize the collection and reporting of application telemetry information.  The [New Relic trace exporter for OpenTelemetry](./src/OpenTelemetry.Exporter.NewRelic/README.md) allows tracing information collected within the OpenTelemetry framework to be reported to New Relic.  This exporter is built using New Relic's Telemetry SDK.


### [New Relic Telemetry SDK](./src/NewRelic.Telemetry/README.md)
The [New Relic Telemetry SDK](./src/NewRelic.Telemetry/README.md) allows tracking of information about the execution of an application and sends it to the New Relic back-end.  New Relic tools allow the visualization of this information, making it insightful and actionable.


### Limitations
The New Relic Telemetry APIs are rate limited. Please reference the [documentation](https://github.com/newrelic/newrelic-telemetry-sdk-specs) for New Relic Metric API and Trace API requirements and limits on the specifics of the rate limits.

### Find and use your data

Tips on how to find and query your data in New Relic:
- [Find metric data](https://docs.newrelic.com/docs/data-ingest-apis/get-data-new-relic/metric-api/introduction-metric-api#find-data)
- [Find trace/span data](https://docs.newrelic.com/docs/understand-dependencies/distributed-tracing/trace-api/introduction-trace-api#view-data)

For general querying information, see:
- [Query New Relic data](https://docs.newrelic.com/docs/using-new-relic/data/understand-data/query-new-relic-data)
- [Intro to NRQL](https://docs.newrelic.com/docs/query-data/nrql-new-relic-query-language/getting-started/introduction-nrql)

### Packages

| Package | MyGet (CI) | NuGet (releases) |
| ------- | ---------- | ---------------- |
| NewRelic.Telemetry | [![MyGet CI][myget-image-NewRelic-Telemetry]][myget-url-NewRelic-Telemetry] | [![NuGet Release][nuget-image-NewRelic-Telemetry]][nuget-url-NewRelic-Telemetry] |
| OpenTelemetry.Exporter.NewRelic | [![MyGet CI][myget-image-OpenTelemetry-Exporter-NewRelic]][myget-url-OpenTelemetry-Exporter-NewRelic] | [![NuGet Release][nuget-image-OpenTelemetry-Exporter-NewRelic]][nuget-url-OpenTelemetry-Exporter-NewRelic] |

### Building
CI builds are run on Azure Pipelines: 
[![Build status](https://dev.azure.com/NRAzurePipelines/dotnet/_apis/build/status/newrelic.newrelic-telemetry-sdk-dotnet?branchName=master)](https://dev.azure.com/NRAzurePipelines/dotnet/_build/latest?definitionId=17&branchName=master)


### Contributing
Full details are available in our [CONTRIBUTING.md](CONTRIBUTING.md) file. We'd love to get your contributions to improve the Telemetry SDK for .NET and for the OpenTelemetry Trace Exporter for .NET! Keep in mind when you submit your pull request, you'll need to sign the CLA via the click-through using CLA-Assistant. You only have to sign the CLA one time per project. To execute our corporate CLA, which is required if your contribution is on behalf of a company, or if you have any questions, please drop us an email at open-source@newrelic.com.


### Open source license
This project is distributed under the [Apache 2 license](LICENSE).


### Support
New Relic has open-sourced this project. This project is provided AS-IS WITHOUT WARRANTY OR DEDICATED SUPPORT. Issues and contributions should be reported to the project here on GitHub.

We encourage you to bring your experiences and questions to the [Explorers Hub](https://discuss.newrelic.com) where our community members collaborate on solutions and new ideas.

[myget-image-NewRelic-Telemetry]:                   https://img.shields.io/myget/newrelic/vpre/NewRelic.Telemetry.svg
[myget-url-NewRelic-Telemetry]:                     https://www.myget.org/feed/newrelic/package/nuget/NewRelic.Telemetry
[nuget-image-NewRelic-Telemetry]:                   https://img.shields.io/nuget/vpre/NewRelic.Telemetry.svg
[nuget-url-NewRelic-Telemetry]:                     https://www.nuget.org/packages/NewRelic.Telemetry

[myget-image-OpenTelemetry-Exporter-NewRelic]:      https://img.shields.io/myget/newrelic/vpre/OpenTelemetry.Exporter.NewRelic.svg
[myget-url-OpenTelemetry-Exporter-NewRelic]:        https://www.myget.org/feed/newrelic/package/nuget/OpenTelemetry.Exporter.NewRelic
[nuget-image-OpenTelemetry-Exporter-NewRelic]:      https://img.shields.io/nuget/vpre/OpenTelemetry.Exporter.NewRelic.svg
[nuget-url-OpenTelemetry-Exporter-NewRelic]:        https://www.nuget.org/packages/OpenTelemetry.Exporter.NewRelic
