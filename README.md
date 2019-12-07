# New Relic Telemetry SDK and OpenTelemetry Support for .NET

This repo contains the New Relic Telemetry SDK and an Open Telemetry Provider for .NET.



### [New Relic Open Telemetry Trace Exporter](./src/OpenTelemetry.Exporter.NewRelic/README.md)
[OpenTelemetry](https://opentelemetry.io/) is a set of APIs that aim to standardize the collection and reporting of application telemetry information.  The [New Relic Trace Exporter for OpenTelemetry](./src/OpenTelemetry.Exporter.NewRelic/README.md) allows tracing information information collected within the OpenTelemetry framework to be reported to New Relic.  The OpenTelemetry Trace Exporter is built using New Relic's Telemetry SDK.



### [New Relic Open Telemetry SDK](./src/NewRelic.Telemetry/README.md)
The [New Relic Telemetry SDK](./src/NewRelic.Telemetry/README.md) allows you track information about the execution of your application and to send it to the New Relic back-end.  New Relic tools allow you to visualize this information, making it insightful, and actionable.


### Limitations
The New Relic Telemetry APIs are rate limited. Please reference the documentation for New Relic Metrics API and New Relic Trace API Requirements and Limits on the specifics of the rate limits.



### Contributing
Full details are available in our [CONTRIBUTING.md](CONTRIBUTING.md) file. We'd love to get your contributions to improve the Telemetry SDK for .NET and for the OpenTelemetry Trace Exporter for .NET! Keep in mind when you submit your pull request, you'll need to sign the CLA via the click-through using CLA-Assistant. You only have to sign the CLA one time per project. To execute our corporate CLA, which is required if your contribution is on behalf of a company, or if you have any questions, please drop us an email at open-source@newrelic.com.


### Open Source License
This project is distributed under the [Apache 2 license](LICENSE).


### Support
New Relic has open-sourced this project. This project is provided AS-IS WITHOUT WARRANTY OR DEDICATED SUPPORT. Issues and contributions should be reported to the project here on GitHub.

We encourage you to bring your experiences and questions to the [Explorers Hub](https://discuss.newrelic.com) where our community members collaborate on solutions and new ideas.
