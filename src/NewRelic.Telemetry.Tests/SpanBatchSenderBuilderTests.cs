using NUnit.Framework;
using NewRelic.Telemetry.Spans;

namespace NewRelic.Telemetry.Tests
{
    public class SpanBatchSenderBuilderTests
    {
        [Test]
        public void TestBuild()
        {
            SpanBatchSender result = new SpanBatchSenderBuilder().WithApiKey("123").WithUrlOverride("http://bogus.com").WithAuditLoggingEnabled().Build();
            Assert.NotNull(result);
        }
    }
}