[![Community Project header](https://github.com/newrelic/opensource-website/raw/master/src/images/categories/Community_Project.png)](https://opensource.newrelic.com/oss-category/#community-project)

❗Notice: This project is in the process of being archived as is and is no longer actively maintained.

Rather than developing a .NET specific OpenTelemetry exporter New Relic has adopted a language agnostic approach that facilitates data collection from all OpenTelemetry data sources.

The current recommended approaches for sending OpenTelemetry data to the New Relic platform are as follows:

* Configure your OpenTelemetry data source to send data to the [OpenTelemetry Collector](https://docs.newrelic.com/docs/integrations/open-source-telemetry-integrations/opentelemetry/introduction-opentelemetry-new-relic/#collector) using the OpenTelemetry Protocol (OTLP) and configure the collector to forward the data using the [New Relic collector exporter](https://github.com/newrelic-forks/opentelemetry-collector-contrib/tree/newrelic-main/exporter/newrelicexporter).
* Configure your OpenTelemetry data source to send data to the native OpenTelemetry Protocol (OTLP) data ingestion endpoint. [OTLP](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/protocol/otlp.md) is an open source gRPC based protocol for sending telemetry data. The protocol is vendor agnostic and open source.

For more details please see:
* [OpenTelemetry quick start](https://docs.newrelic.com/docs/integrations/open-source-telemetry-integrations/opentelemetry/opentelemetry-quick-start/)
* [Introduction to OpenTelemetry with New Relic](https://docs.newrelic.com/docs/integrations/open-source-telemetry-integrations/opentelemetry/introduction-opentelemetry-new-relic/)
* [Native OpenTelemetry Protocol (OTLP) support](https://docs.newrelic.com/whats-new/2021/04/native-support-opentelemetry/)

# New Relic .NET Telemetry SDK

![New Relic .NET Telemetry SDK](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/workflows/New%20Relic%20Telemetry%20SDK%20for%20.NET/badge.svg?branch=main)

The [New Relic .NET Telemetry SDK](/src/NewRelic.Telemetry) allows tracking of information about the execution of an application and sends it to the New Relic back-end.  New Relic tools allow the visualization of this information, making it insightful and actionable.

## Limitations
The New Relic Telemetry APIs are rate limited. Please reference the [documentation](https://github.com/newrelic/newrelic-telemetry-sdk-specs) for New Relic Metric API and Trace API requirements and limits on the specifics of the rate limits.

## Find and use your data

Tips on how to find and query your data in New Relic:
- [Find metric data](https://docs.newrelic.com/docs/data-ingest-apis/get-data-new-relic/metric-api/introduction-metric-api#find-data)
- [Find trace/span data](https://docs.newrelic.com/docs/understand-dependencies/distributed-tracing/trace-api/introduction-trace-api#view-data)

For general querying information, see:
- [Query New Relic data](https://docs.newrelic.com/docs/using-new-relic/data/understand-data/query-new-relic-data)
- [Intro to NRQL](https://docs.newrelic.com/docs/query-data/nrql-new-relic-query-language/getting-started/introduction-nrql)

## Support

Should you need assistance with New Relic products, you are in good hands with several support diagnostic tools and support channels.

This [troubleshooting framework](https://discuss.newrelic.com/t/troubleshooting-frameworks/108787) steps you through common troubleshooting questions.

If the issue has been confirmed as a bug or is a Feature request, please file a Github issue.

**Support Channels**

* [New Relic Documentation](https://docs.newrelic.com/docs/agents/net-agent): Comprehensive guidance for using our agent
* [New Relic Community](https://discuss.newrelic.com/c/support-products-agents/net-agent): The best place to engage in troubleshooting questions
* [New Relic Developer](https://developer.newrelic.com/): Resources for building a custom observability applications
* [New Relic University](https://learn.newrelic.com/): A range of online training for New Relic users of every level


## Contributing
We encourage your contributions to improve the .NET Telemetry SDK! Keep in mind when you submit your pull request, you'll need to sign the CLA via the click-through using CLA-Assistant. You only have to sign the CLA one time per project.
If you have any questions, or to execute our corporate CLA, required if your contribution is on behalf of a company,  please drop us an email at opensource@newrelic.com.

**A note about vulnerabilities**

As noted in our [security policy](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/security/policy), New Relic is committed to the privacy and security of our customers and their data. We believe that providing coordinated disclosure by security researchers and engaging with the security community are important means to achieve our security goals.

If you believe you have found a security vulnerability in this project or any of New Relic's products or websites, we welcome and greatly appreciate you reporting it to New Relic through [HackerOne](https://hackerone.com/newrelic).

If you would like to contribute to this project, please review [these guidelines](./CONTRIBUTING.md).

To [all contributors](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/graphs/contributors), we thank you!  Without your contribution, this project would not be what it is today.  We also host a community project page dedicated to
the [New Relic Telemetry SDK (.NET)](https://opensource.newrelic.com/projects/newrelic/newrelic-telemetry-sdk-dotnet).


## Open source license
This project is distributed under the [Apache 2 license](LICENSE).
