# New Relic Telemetry SDK for .NET

[![NuGet](https://img.shields.io/nuget/v/NewRelic.Telemetry.svg)](https://www.nuget.org/packages/NewRelic.Telemetry)
[![NuGet](https://img.shields.io/nuget/dt/NewRelic.Telemetry.svg)](https://www.nuget.org/packages/NewRelic.Telemetry)

The New Relic Telemetry SDK for .NET allows the capture of information about the execution of your application and provides a mechanism to send this information to New Relic.


## Prerequisites
* A valid New Relic <a target="_blank" href="https://docs.newrelic.com/docs/insights/insights-data-sources/custom-data/introduction-event-api#register">Insights Insert API Key</a>.
* A .NET Core 2.0+ or .NET Framework 4.5+ Application
## Getting Started
* Incorporate the [NewRelic.Telemetry](https://www.nuget.org/packages/NewRelic.Telemetry) NuGet Package into your project.

## Next Steps
* Using the [Telemetry SDK for Tracing](./Spans/README.md)
* Review Telemetry SDK [Configuration Options](./TelemetryConfiguration.md)



### Limitations
The New Relic Telemetry APIs are rate limited. Please reference the documentation for the [New Relic Trace API](https://docs.newrelic.com/docs/understand-dependencies/distributed-tracing/trace-api/trace-api-general-requirements-limits) for the specific rate limits.



### Contributing
Full details are available in our [CONTRIBUTING.md](../../CONTRIBUTING.md) file. We'd love to get your contributions to improve the Telemetry SDK for .NET! Keep in mind when you submit your pull request, you'll need to sign the CLA via the click-through using CLA-Assistant. You only have to sign the CLA one time per project. To execute our corporate CLA, which is required if your contribution is on behalf of a company, or if you have any questions, please drop us an email at open-source@newrelic.com.


### Open Source License
This project is distributed under the [Apache 2 license](LICENSE).


### Support
New Relic has open-sourced this project. This project is provided AS-IS WITHOUT WARRANTY OR DEDICATED SUPPORT. Issues and contributions should be reported to the project here on GitHub.

We encourage you to bring your experiences and questions to the [Explorers Hub](https://discuss.newrelic.com) where our community members collaborate on solutions and new ideas.
