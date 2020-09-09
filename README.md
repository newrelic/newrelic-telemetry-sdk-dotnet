[![Community Plus header](https://github.com/newrelic/opensource-website/raw/master/src/images/categories/Community_Plus.png)](https://opensource.newrelic.com/oss-category/#community-plus)


# New Relic Telemetry SDK and OpenTelemetry support for .NET

[![Build status](https://dev.azure.com/NRAzurePipelines/dotnet/_apis/build/status/newrelic.newrelic-telemetry-sdk-dotnet?branchName=master)](https://dev.azure.com/NRAzurePipelines/dotnet/_build/latest?definitionId=17&branchName=master)

This repo contains the New Relic Telemetry SDK and an OpenTelemetry provider for .NET.

## [New Relic OpenTelemetry trace exporter](/src/OpenTelemetry.Exporter.NewRelic)
[OpenTelemetry](https://opentelemetry.io/) is a set of APIs that aim to standardize the collection and reporting of application telemetry information.  The [New Relic trace exporter for OpenTelemetry](/src/OpenTelemetry.Exporter.NewRelic) allows tracing information collected within the OpenTelemetry framework to be reported to New Relic.  This exporter is built using New Relic's Telemetry SDK.

## [New Relic Telemetry SDK](/src/NewRelic.Telemetry)
The [New Relic Telemetry SDK](/src/NewRelic.Telemetry) allows tracking of information about the execution of an application and sends it to the New Relic back-end.  New Relic tools allow the visualization of this information, making it insightful and actionable.

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
* [New Relic Technical Support](https://support.newrelic.com/) 24/7/365 ticketed support. Read more about our [Technical Support Offerings](https://docs.newrelic.com/docs/licenses/license-information/general-usage-licenses/support-plan).


## Contributing
We encourage your contributions to improve [project name]! Keep in mind when you submit your pull request, you'll need to sign the CLA via the click-through using CLA-Assistant. You only have to sign the CLA one time per project.
If you have any questions, or to execute our corporate CLA, required if your contribution is on behalf of a company,  please drop us an email at opensource@newrelic.com.

**A note about vulnerabilities**

As noted in our [security policy](/SECURITY.md), New Relic is committed to the privacy and security of our customers and their data. We believe that providing coordinated disclosure by security researchers and engaging with the security community are important means to achieve our security goals.

If you believe you have found a security vulnerability in this project or any of New Relic's products or websites, we welcome and greatly appreciate you reporting it to New Relic through [HackerOne](https://hackerone.com/newrelic).

If you would like to contribute to this project, please review [these guidelines](./CONTRIBUTING.md).

To [all contributors](https://github.com/newrelic/newrelic-telemetry-sdk-dotnet/graphs/contributors), we thank you!  Without your contribution, this project would not be what it is today.  We also host a community project page dedicated to 
the [PROJECT NAME](https://opensource.newrelic.com/projects/newrelic/newrelic-telemetry-sdk-dotnet).


## Open source license
This project is distributed under the [Apache 2 license](LICENSE).
