using NUnit.Framework;
using System.Collections.Generic;

namespace NewRelic.Telemetry.Sdk.Tests
{
    class SpanBatchMarshallerTests
    {
        [Test]
        public void ToJson_EmptySpanBatch() 
        {
            var expectedString = @"[{""common"":{""trace.id"":""traceId""}}]";
            var spanBatch = new SpanBatch(new List<Span>(), new Dictionary<string, object>(), "traceId");
            var marshaller = new SpanBatchMarshaller();
            var jsonString = marshaller.ToJson(spanBatch);
            Assert.AreEqual(expectedString, jsonString);
        }

        [Test]
        public void ToJson_NullSpanBatch()
        {
            var expectedString = @"[{}]";
            var marshaller = new SpanBatchMarshaller();
            var jsonString = marshaller.ToJson(null);
            Assert.AreEqual(expectedString, jsonString);
        }

        [Test]
        public void ToJson_NonEmptySpanBatch()
        {
            var expectedString = @"[{""common"":{""trace.id"":""traceId""},""spans"":[{""id"":""span1"",""trace.id"":""traceId"",""timestamp"":1,""error"":true,""attributes"":{""duration.ms"":67,""name"":""name"",""service.name"":""serviceName"",""parent.id"":""parentId""}}]}]";

            var spanBuilder = Span.GetSpanBuilder("span1");
            var span = spanBuilder.TraceId("traceId").TimeStamp(1L)
                .ServiceName("serviceName").DurationMs(67d).Name("name")
                .ParentId("parentId").Error(true).Build();

            var spanBatch = new SpanBatch(new List<Span>() {span}, new Dictionary<string, object>(), "traceId");
            var marshaller = new Sdk.SpanBatchMarshaller();
            var jsonString = marshaller.ToJson(spanBatch);
            Assert.AreEqual(expectedString, jsonString);
        }

        [Test]
        public void ToJson_SpanBatchWithAttributes()
        {
            var customAttributes = new Dictionary<string, object>
            {
                { "customAtt1", "hello" },
                { "customAtt2", 1 },
                { "customAtt3", 1.2D },
                { "customAtt4", true }
            };

            var expectedString = @"[{""common"":{""trace.id"":""traceId"",""attributes"":{""customAtt1"":""hello"",""customAtt2"":1,""customAtt3"":1.2,""customAtt4"":true}},""spans"":[{""id"":""span1"",""trace.id"":""traceId"",""timestamp"":1,""error"":true,""attributes"":{""customAtt1"":""hello"",""customAtt2"":1,""customAtt3"":1.2,""customAtt4"":true,""duration.ms"":67,""name"":""name"",""service.name"":""serviceName"",""parent.id"":""parentId""}}]}]";

            var spanBuilder = Span.GetSpanBuilder("span1");
            var span = spanBuilder.TraceId("traceId").TimeStamp(1L)
                .ServiceName("serviceName").DurationMs(67d).Name("name")
                .ParentId("parentId").Error(true).Attributes(customAttributes).Build();

            var spanBatch = new SpanBatch(new List<Span>() { span }, customAttributes, "traceId");
            var marshaller = new SpanBatchMarshaller();
            var jsonString = marshaller.ToJson(spanBatch);
            Assert.AreEqual(expectedString, jsonString);
        }

        [TestCase("spanId", null, null, false, ExpectedResult = @"[{""spans"":[{""id"":""spanId""}]}]")]
        [TestCase(null, null, null, false, ExpectedResult = @"[{}]")]
        [TestCase(null, null, null, true, ExpectedResult = @"[{}]")]
        public string ToJson_OmitNullValueProperty(string spanId, string traceId, IDictionary<string, object> attributes, bool nullList)
        {
            var marshaller = new SpanBatchMarshaller();

            var spanBuilder = Span.GetSpanBuilder(spanId);
            var span = spanBuilder.Build();
            var spans = nullList ? null : new List<Span>() { span };
            var spanBatch = new SpanBatch(spans, attributes, traceId);
            return marshaller.ToJson(spanBatch);
        }

        [TestCase("spanId", ExpectedResult = @"[{""spans"":[{""id"":""spanId""}]}]")]
        [TestCase(null, ExpectedResult = @"[{}]")]
        public string ToJson_ListContainsNullSpans(string spanId) 
        {
            var marshaller = new SpanBatchMarshaller();

            var spanBatch = new SpanBatch(
                new List<Span>() 
                { 
                    Span.GetSpanBuilder(null).Build(),
                    Span.GetSpanBuilder(null).Build(),
                    Span.GetSpanBuilder(spanId).Build() 
                }, null, null);

            return marshaller.ToJson(spanBatch);
        }

    }
}
