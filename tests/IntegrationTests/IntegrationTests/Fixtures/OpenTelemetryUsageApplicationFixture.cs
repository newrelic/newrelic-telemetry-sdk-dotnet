using System.Net;

namespace IntegrationTests.Fixtures
{
    public class OpenTelemetryUsageApplicationFixture : BaseFixture
    {
        public OpenTelemetryUsageApplicationFixture():base(new OpenTelemetryUsageApplication("SampleAspNetCoreApp"))
        {
        }

        public void MakeRequestToWeatherforecastEndpoint()
        {
            using var client = new WebClient();
            var response = client.DownloadString("http://localhost:5000/weatherforecast");
        }
    }
}
