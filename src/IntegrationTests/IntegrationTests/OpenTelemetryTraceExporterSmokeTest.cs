using Newtonsoft.Json;
using IntegrationTests.Fixtures;
using System.Net.Http;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationTests
{
    public class OpenTelemetryTraceExporterSmokeTest : IClassFixture<OpenTelemetryUsageApplicationFixture>
    {
        private readonly OpenTelemetryUsageApplicationFixture _fixture;

        private const string _traceApiKey = "{YOUR_TRACE_API_KEY}";

        private const string _insightsQueryApiKey = "{YOUR_INSIGHT_QUERY_API_KEY}";

        private const string _insightsQueryApiEndpoint = "https://insights-api.newrelic.com";

        private const string _traceEndPointUrl = "https://trace-api.newrelic.com/trace/v1";

        private const string _accountNumber = "{YOUR_ACCOUNT_NUMBER}";

        public OpenTelemetryTraceExporterSmokeTest(OpenTelemetryUsageApplicationFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _fixture.TestLogger = output;

            _fixture.Exercise = () =>
            {
                _fixture.MakeRequestToWeatherforecastEndpoint();
            };

            _fixture.SetEnvironmentVariables(new Dictionary<string, string>()
            {
                {"NewRelic:ApiKey", _traceApiKey},
                {"NewRelic:TraceUrlOverride", _traceEndPointUrl}
            });

            _fixture.Initialize();

            //Wait 10s for the data to show up on New Relic backend.
            Thread.Sleep(10000);
        }

        [Fact(Skip = "Temporarily skipping this test so that the build pipeline won't fail. This is because this test requires setting Api keys manually.")]
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
        }
    }
}
