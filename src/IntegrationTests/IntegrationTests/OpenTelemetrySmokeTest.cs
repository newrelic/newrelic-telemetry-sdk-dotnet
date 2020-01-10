using Newtonsoft.Json;
using IntegrationTests.Fixtures;
using System.Net.Http;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using System;
using System.Web;

namespace IntegrationTests
{
    public class OpenTelemetrySmokeTest : IClassFixture<OpenTelemetryUsageApplicationFixture>
    {
        private readonly string _insightsQueryApiKey;

        private readonly string _insightsQueryApiEndpoint = "https://insights-api.newrelic.com";

        private readonly string _accountNumber;

        public OpenTelemetrySmokeTest(OpenTelemetryUsageApplicationFixture fixture, ITestOutputHelper output)
        {
            _accountNumber = Environment.GetEnvironmentVariable("NewRelic:AccountNumber");

            Assert.True(!string.IsNullOrEmpty(_accountNumber), "NewRelic:AccountNumber environment variable is either null, empty or does not exist.");

            _insightsQueryApiKey = Environment.GetEnvironmentVariable("NewRelic:InsightsQueryApiKey");

            Assert.True(!string.IsNullOrEmpty(_insightsQueryApiKey), "NewRelic:InsightsQueryApiKey environment variable is either null, empty or does not exist.");

            var insightsQueryApiEndpointFromEnvironmentVariable = Environment.GetEnvironmentVariable("NewRelic:InsightsQueryApiEndpoint");

            if (!string.IsNullOrEmpty(insightsQueryApiEndpointFromEnvironmentVariable))
            {
                _insightsQueryApiEndpoint = insightsQueryApiEndpointFromEnvironmentVariable;
            }

            if (fixture.Initialized)
            {
                return;
            }

            fixture.TestLogger = output;

            fixture.Exercise = () =>
            {
                fixture.MakeRequestToWeatherforecastEndpoint();
            };

            fixture.Initialize();

            //Wait 10s for the data to show up on New Relic backend.
            Thread.Sleep(10000);
        }

        [Fact]
        public async void TraceExporterTest()
        {
            using var httpClient = new HttpClient();
            var insightQuery = HttpUtility.UrlEncode("SELECT * FROM Span WHERE service.name = 'SampleAspNetCoreApp' SINCE 2 minutes ago")
;
            var request = new HttpRequestMessage(HttpMethod.Get, @$"{_insightsQueryApiEndpoint}/v1/accounts/{_accountNumber}/query?nrql={insightQuery}");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("X-Query-Key", _insightsQueryApiKey);

            var result = await httpClient.SendAsync(request);
            var body = await result.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<NewRelicInsightsResponse<NewRelicSpanEvent>>(body);

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

        [Fact]
        public async void MetricTest()
        {
            using var httpClient = new HttpClient();

            var insightQuery = HttpUtility.UrlEncode("SELECT * FROM Metric WHERE metricName = 'WeatherForecast/Get' SINCE 2 minutes ago");
            var request = new HttpRequestMessage(HttpMethod.Get, @$"{_insightsQueryApiEndpoint}/v1/accounts/{_accountNumber}/query?nrql={insightQuery}");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("X-Query-Key", _insightsQueryApiKey);

            var result = await httpClient.SendAsync(request);
            var body = await result.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<NewRelicInsightsResponse<NewRelicMetricEvent>>(body);

            Assert.NotNull(response);
            Assert.Single(response.Results);
            Assert.Single(response.Results.FirstOrDefault().Events);

            var metric = response.Results.FirstOrDefault().Events.FirstOrDefault();
            Assert.True(metric.TimeStamp > 0);
            Assert.Equal("metricAPI", metric.NewRelicSource);
            Assert.Equal("WeatherForecast/Get", metric.MetricName);
        }
    }
}
