using Newtonsoft.Json;
using IntegrationTests.Fixtures;
using System.Net.Http;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace IntegrationTests
{
    public class OpenTelemetryTraceExporterSmokeTest : IClassFixture<OpenTelemetryUsageApplicationFixture>
    {
        private readonly OpenTelemetryUsageApplicationFixture _fixture;

        private readonly string _insightsQueryApiKey;

        private readonly string _insightsQueryApiEndpoint = "https://insights-api.newrelic.com";

        private readonly string _accountNumber;

        public OpenTelemetryTraceExporterSmokeTest(OpenTelemetryUsageApplicationFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _fixture.TestLogger = output;

            _fixture.Exercise = () =>
            {
                _fixture.MakeRequestToWeatherforecastEndpoint();
            };

            _accountNumber = Environment.GetEnvironmentVariable("NewRelic:AccountNumber");

            Assert.True(!string.IsNullOrEmpty(_accountNumber), "NewRelic:AccountNumber environment variable is either null, empty or does not exist.");

            _insightsQueryApiKey = Environment.GetEnvironmentVariable("NewRelic:InsightsQueryApiKey");

            Assert.True(!string.IsNullOrEmpty(_insightsQueryApiKey), "NewRelic:InsightsQueryApiKey environment variable is either null, empty or does not exist.");

            var insightsQueryApiEndpointFromEnvironmentVariable = Environment.GetEnvironmentVariable("NewRelic:InsightsQueryApiEndpoint");

            if (!string.IsNullOrEmpty(insightsQueryApiEndpointFromEnvironmentVariable))
            {
                _insightsQueryApiEndpoint = insightsQueryApiEndpointFromEnvironmentVariable;
            }

            _fixture.Initialize();

            //Wait 10s for the data to show up on New Relic backend.
            Thread.Sleep(10000);
        }

        [Fact]
        public async void Test()
        {
            using var httpClient = new HttpClient();

            // SELECT * FROM Span WHERE service.name = 'SampleAspNetCoreApp' SINCE 2 minutes ago
            var insightQuery = "SELECT%20*%20FROM%20Span%20WHERE%20service.name%20%3D%20%27SampleAspNetCoreApp%27%20SINCE%202%20minutes%20ago";
            var request = new HttpRequestMessage(HttpMethod.Get, @$"{_insightsQueryApiEndpoint}/v1/accounts/{_accountNumber}/query?nrql={insightQuery}");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("X-Query-Key", _insightsQueryApiKey);

            var result = await httpClient.SendAsync(request);
            var body = await result.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<NewRelicInsightsResponse>(body);

            Assert.NotNull(response);
            Assert.Single(response.Results);
            Assert.Equal(2, response.Results.FirstOrDefault().Events.Count);

            response.Results.FirstOrDefault().Events.ForEach(item =>
            {
                Assert.NotNull(item.Guid);
                Assert.NotNull(item.TraceId);
                Assert.NotNull(item.Name);
                Assert.Equal("SampleAspNetCoreApp", item.ServiceName);
                Assert.Equal("SampleAspNetCoreApp", item.EntityName);
            });

            var traceId = response.Results.FirstOrDefault().Events.FirstOrDefault().TraceId;

            response.Results.FirstOrDefault().Events.ForEach(item =>
            {
                Assert.Equal(traceId, item.TraceId);
            });

        }
    }
}
