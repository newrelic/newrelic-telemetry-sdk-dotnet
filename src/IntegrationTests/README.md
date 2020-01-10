# Integration tests for New Relic Open Telemetry Exporter for .NET

This test solution validates the integration between the New Relic Open Telemetry Exporter for .NET with an ASP.NET Core 3.0 application.

## Prerequisites
* A valid New Relic <a target="_blank" href="https://docs.newrelic.com/docs/insights/insights-data-sources/custom-data/introduction-event-api#register">Insights Insert API Key</a>.
* A valid New Relic <a target="_blank" href="https://docs.newrelic.com/docs/insights/insights-api/get-data/query-insights-event-data-api#register">Insights Query API Key</a>.
* A valid New Relic <a target="_blank" href="https://docs.newrelic.com/docs/accounts/install-new-relic/account-setup/account-id#finding">Account Id</a>.

## Getting Started
* Build the NewRelic.Telemetry solution. This creates the OpenTelemetry.Exporter.NewRelic and NewRelic.Telemetry nuget packages and  publishes these packages to a Nuget local source located at `.\src\LocalNugetPackageSource`.
* Set the following environment variables with the appropriate values:
	`NewRelic:ApiKey`
	`NewRelic:InsightsQueryApiKey`
	`NewRelic:AccountNumber`

## Next Steps
* Run the test from Visual Studio Test Explorer or using the `dotnet test .\src\IntegrationTests\IntegrationTests --no-build` command


## Limitations
The New Relic Telemetry APIs are rate limited. The execution of the integration tests will count against your rate limit. Please reference the documentation for the [New Relic Trace API](https://docs.newrelic.com/docs/understand-dependencies/distributed-tracing/trace-api/trace-api-general-requirements-limits) for the specific rate limits.


