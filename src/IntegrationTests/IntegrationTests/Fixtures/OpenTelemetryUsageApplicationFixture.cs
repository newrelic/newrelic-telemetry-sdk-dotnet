using System.Net;

namespace IntegrationTests.Fixtures
{
    public class OpenTelemetryUsageApplicationFixture : BaseFixture
    {
        public OpenTelemetryUsageApplicationFixture():base(new OpenTelemetryUsageApplication("SampleAspNetCoreApp", null))
        {
        }

        public void MakeRequestToWeatherforecastEndpoint()
        {
            using var client = new WebClient();
            var response = client.DownloadString("https://localhost:5001/weatherforecast");
        }
    }
}
