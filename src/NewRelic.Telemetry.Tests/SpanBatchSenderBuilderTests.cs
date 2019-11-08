using NUnit.Framework;

namespace NewRelic.Telemetry.Tests
{
    public class SpanBatchSenderBuilderTests
    {
        [Test]
        public void TestBuild()
        {
            SpanBatchSender result = new SpanBatchSenderBuilder().ApiKey("123").UrlOverride("http://bogus.com").EnableAuditLogging().Build();
            Assert.NotNull(result);
        }
    }
}