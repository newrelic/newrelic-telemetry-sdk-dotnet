# Integration tests for New Relic Open Telemetry Exporter for .NET

This test solution provides an end-to-end validation for the integration between the New Relic Open Telemetry Exporter for .NET with ASP.NET Core 3.0 applications.

## Prerequisites
* A valid New Relic <a target="_blank" href="https://docs.newrelic.com/docs/insights/insights-data-sources/custom-data/introduction-event-api#register">Insights Insert API Key</a>.
* A valid New Relic <a target="_blank" href="https://docs.newrelic.com/docs/insights/insights-api/get-data/query-insights-event-data-api#register">Insights Query API Key</a>.
* A valid New Relic <a target="_blank" href="https://docs.newrelic.com/docs/accounts/install-new-relic/account-setup/account-id#finding">Account Id</a>.

## Getting Started
* Build the NewRelic.Telemetry solution. This action will result creating the OpenTelemetry.Exporter.NewRelic and NewRelic.Telemetry nuget packages in their respective output directories then pushing these packages to a Nuget local source located at this relative path `.\src\LocalNugetPackageSource`. This build process will also build the sample test application using these new nuget packages from the local source.
* Set the following environment variables with the appropriate values:
	`NewRelic:ApiKey`
	`NewRelic:InsightsQueryApiKey`
	`NewRelic:AccountNumber`

## Next Steps
* Run the test from Visual Studio Test Explorer or using the `dotnet test .\src\IntegrationTests\IntegrationTests --no-build` command


### Limitations
The New Relic Telemetry APIs are rate limited. Please reference the documentation for the [New Relic Trace API](https://docs.newrelic.com/docs/understand-dependencies/distributed-tracing/trace-api/trace-api-general-requirements-limits) for the specific rate limits.



### Contributing
Full details are available in our [CONTRIBUTING.md](../../CONTRIBUTING.md) file. We'd love to get your contributions to improve the Telemetry SDK for .NET! Keep in mind when you submit your pull request, you'll need to sign the CLA via the click-through using CLA-Assistant. You only have to sign the CLA one time per project. To execute our corporate CLA, which is required if your contribution is on behalf of a company, or if you have any questions, please drop us an email at open-source@newrelic.com.


### Open Source License
This project is distributed under the [Apache 2 license](LICENSE).


### Support
New Relic has open-sourced this project. This project is provided AS-IS WITHOUT WARRANTY OR DEDICATED SUPPORT. Issues and contributions should be reported to the project here on GitHub.

We encourage you to bring your experiences and questions to the [Explorers Hub](https://discuss.newrelic.com) where our community members collaborate on solutions and new ideas.
